using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common;
using GearZone.Application.Features.Payout.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GearZone.Application.Features.Payout
{
    public class PayoutService : IPayoutService
    {
        private const int MaxRetryCount = 3;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayoutClient _payoutClient;
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IPayoutBatchRepository _payoutBatchRepository;
        private readonly IPayoutTransactionRepository _payoutTransactionRepository;
        private readonly IPayoutItemRepository _payoutItemRepository;
        private readonly ILogger<PayoutService> _logger;

        public PayoutService(
            IUnitOfWork unitOfWork,
            IPayoutClient payoutClient,
            ISubOrderRepository subOrderRepository,
            IPayoutBatchRepository payoutBatchRepository,
            IPayoutTransactionRepository payoutTransactionRepository,
            IPayoutItemRepository payoutItemRepository,
            ILogger<PayoutService> logger)
        {
            _unitOfWork = unitOfWork;
            _payoutClient = payoutClient;
            _subOrderRepository = subOrderRepository;
            _payoutBatchRepository = payoutBatchRepository;
            _payoutTransactionRepository = payoutTransactionRepository;
            _payoutItemRepository = payoutItemRepository;
            _logger = logger;
        }

        public async Task<Guid> GenerateWeeklyBatchAsync(
        DateTime endDate,
        CancellationToken ct = default)
        {
            // 1. Tính period
            var periodEnd = endDate.Date;
            var periodStart = periodEnd.AddDays(-7);

            _logger.LogInformation(
                "[Payout] Generating batch {Start:dd/MM} - {End:dd/MM}",
                periodStart, periodEnd);

            // 2. Kiểm tra trùng
            var exists = await _payoutBatchRepository.ExistsByPeriodAsync(
                periodStart, periodEnd, ct);

            if (exists)
                throw new InvalidOperationException($"Batch for {periodStart:dd/MM} - {periodEnd:dd/MM} already exists.");

            // 3. Lấy orders đủ điều kiện (SubOrders)
            var eligibleSubOrders = await _subOrderRepository
                .GetEligibleForPayoutAsync(periodStart, periodEnd, ct);

            _logger.LogInformation(
                "[Payout] Found {Count} eligible orders", eligibleSubOrders.Count);

            // 4. Group by store
            var storeGroups = eligibleSubOrders
                .GroupBy(o => o.StoreId)
                .ToList();

            // 5. Build batch
            var weekNum = GetWeekNumber(periodStart);
            var batch = new PayoutBatch
            {
                Id = Guid.NewGuid(),
                BatchCode = $"BATCH-{periodStart:yyyy}-W{weekNum:D2}",
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Status = PayoutBatchStatus.PendingApproval,
                TotalStores = storeGroups.Count,
                CreatedAt = DateTime.UtcNow,
            };

            // 6. Build transactions + items
            var transactions = new List<PayoutTransaction>();

            foreach (var group in storeGroups)
            {
                var store = group.First().Store;
                var orders = group.ToList();

                var items = orders.Select(o => new PayoutItem
                {
                    Id = Guid.NewGuid(),
                    SubOrderId = o.Id,
                    GrandTotal = o.Subtotal,
                    CommissionAmount = o.CommissionAmount,
                    NetAmount = o.Subtotal - o.CommissionAmount,
                    IsExcluded = false,
                }).ToList();

                var transaction = new PayoutTransaction
                {
                    Id = Guid.NewGuid(),
                    PayoutBatchId = batch.Id,
                    StoreId = group.Key,
                    BankName = store.BankName,
                    BankAccountNumber = store.BankAccountNumber,
                    BankAccountName = store.BankAccountName,
                    BankBin = store.BankBin,
                    OrderCount = orders.Count,
                    GrossAmount = orders.Sum(o => o.Subtotal),
                    CommissionAmount = orders.Sum(o => o.CommissionAmount),
                    NetAmount = orders.Sum(o => o.Subtotal - o.CommissionAmount),
                    Status = PayoutTransactionStatus.Queued,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    Items = items,
                };

                transactions.Add(transaction);
            }

            // 7. Gán tổng vào batch
            batch.TotalGrossAmount = transactions.Sum(t => t.GrossAmount);
            batch.TotalCommissionAmount = transactions.Sum(t => t.CommissionAmount);
            batch.TotalNetAmount = transactions.Sum(t => t.NetAmount);
            batch.Transactions = transactions;

            // 8. Lock orders
            var subOrderIds = eligibleSubOrders.Select(o => o.Id).ToList();
            await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                subOrderIds, PayoutStatus.Locked, ct);

            // 9. Save batch (cascade save transactions + items)
            await _payoutBatchRepository.AddAsync(batch, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[Payout] Batch {Code} created. Stores: {S}, Net: {N}",
                batch.BatchCode, batch.TotalStores, batch.TotalNetAmount);

            return batch.Id;
        }

        // ────────────────────────────────────────────────────────────
        public async Task ApproveBatchAsync(
            Guid batchId,
            string adminId,
            CancellationToken ct = default)
        {
            var batch = await _payoutBatchRepository.GetByIdAsync(batchId, ct)
                ?? throw new KeyNotFoundException($"PayoutBatch with id {batchId} not found");

            if (batch.Status != PayoutBatchStatus.PendingApproval)
                throw new InvalidOperationException(
                    $"Cannot approve batch in status: {batch.Status}");

            batch.Status = PayoutBatchStatus.Approved;
            batch.ApprovedByAdminId = adminId;
            batch.ApprovedAt = DateTime.UtcNow;

            await _payoutBatchRepository.UpdateAsync(batch);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[Payout] Batch {Code} approved by {Admin}",
                batch.BatchCode, adminId);
        }

        // ────────────────────────────────────────────────────────────
        public async Task ProcessPayoutBatchAsync(
            string batchCode,
            CancellationToken ct = default)
        {
            // 1. Load batch kèm transactions
            var batch = await _payoutBatchRepository.Query()
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.BatchCode == batchCode, ct)
                ?? throw new KeyNotFoundException($"PayoutBatch with code {batchCode} not found");

            if (batch.Status != PayoutBatchStatus.Approved)
                throw new InvalidOperationException($"Batch {batch.BatchCode} is not Approved. Current: {batch.Status}");

            // 2. Chuyển sang Processing
            batch.Status = PayoutBatchStatus.Processing;
            await _payoutBatchRepository.UpdateAsync(batch);
            await _unitOfWork.SaveChangesAsync(ct);

            // 3. Lấy transactions cần xử lý
            var queued = batch.Transactions
                .Where(t => t.Status == PayoutTransactionStatus.Queued)
                .ToList();

            _logger.LogInformation(
                "[Payout] Processing batch {Code}: {Count} transactions",
                batch.BatchCode, queued.Count);

            // 4. Map → PayoutRequestDto
            var requests = queued.Select(t => new PayoutRequestDto
            {
                Description = $"GearZone {batch.BatchCode} - {t.BankAccountName}",
                Amount = (long)t.NetAmount,
                ToAccountNumber = t.BankAccountNumber,
                ToBin = t.BankBin,
            }).ToList();

            // 5. Gọi PayOS batch API
            var result = await _payoutClient.CreateBatchPayoutAsync(requests);

            // 6. Xử lý kết quả
            if (result.IsSuccess)
            {
                foreach (var t in queued)
                {
                    t.Status = PayoutTransactionStatus.Success;
                    t.PayOSTransactionId = result.ReferenceId;
                    t.ProcessedAt = DateTime.UtcNow;
                }

                // Đánh dấu orders đã paid
                var txIds = queued.Select(t => t.Id).ToList();
                var subOrderIds = await _payoutItemRepository
                    .GetSubOrderIdsByTransactionIdsAsync(txIds, ct);

                await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                    subOrderIds, PayoutStatus.Paid, ct);
            }
            else
            {
                // Batch API fail toàn bộ → đánh dấu từng cái Failed
                foreach (var t in queued)
                {
                    t.Status = PayoutTransactionStatus.Failed;
                    t.FailureReason = result.ErrorMessage;
                }

                _logger.LogWarning(
                    "[Payout] Batch {Code} PayOS call failed: {Err}",
                    batch.BatchCode, result.ErrorMessage);
            }

            // 7. Update transactions
            await _payoutTransactionRepository.UpdateRangeAsync(queued, ct);

            // 8. Tính lại batch status
            RecalculateBatchStatus(batch);
            await _payoutBatchRepository.UpdateAsync(batch);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[Payout] Batch {Code} done → {Status}. S:{S} F:{F}",
                batch.BatchCode, batch.Status,
                batch.SuccessCount, batch.FailedCount);
        }

        // ────────────────────────────────────────────────────────────
        public async Task ProcessPayoutTransactionAsync(
            string transactionCode,
            CancellationToken ct = default)
        {
            throw new NotImplementedException("Not Implemented by batch context only yet.");
        }

        public async Task RetryTransactionAsync(
            Guid transactionId,
            CancellationToken ct = default)
        {
            var transaction = await _payoutTransactionRepository
                .GetByIdWithDetailsAsync(transactionId, ct)
                ?? throw new KeyNotFoundException($"PayoutTransaction with id {transactionId} not found");

            if (transaction.Status != PayoutTransactionStatus.Failed &&
                transaction.Status != PayoutTransactionStatus.ManualRequired)
                throw new InvalidOperationException(
                    $"Transaction {transactionId} is not in a retryable state.");

            if (transaction.RetryCount >= MaxRetryCount)
            {
                transaction.Status = PayoutTransactionStatus.ManualRequired;
                await _payoutTransactionRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync(ct);
                _logger.LogWarning(
                    "[Payout] Tx {Id} exceeded max retries → ManualRequired",
                    transactionId);
                return;
            }

            // Thử lại
            transaction.Status = PayoutTransactionStatus.Processing;
            transaction.RetryCount += 1;
            await _payoutTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync(ct);

            try
            {
                var request = new PayoutRequestDto
                {
                    Description = $"GearZone RETRY {transaction.Batch.BatchCode}",
                    Amount = (long)transaction.NetAmount,
                    ToAccountNumber = transaction.BankAccountNumber,
                    ToBin = transaction.BankBin,
                };

                var result = await _payoutClient.CreatePayoutAsync(request);

                if (result.IsSuccess)
                {
                    transaction.Status = PayoutTransactionStatus.Success;
                    transaction.PayOSTransactionId = result.ReferenceId;
                    transaction.ProcessedAt = DateTime.UtcNow;
                    transaction.FailureReason = null;

                    var subOrderIds = await _payoutItemRepository
                        .GetSubOrderIdsByTransactionIdAsync(transactionId, ct);
                    await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                        subOrderIds, PayoutStatus.Paid, ct);

                    // Recalculate parent batch
                    await RecalculateParentBatchAsync(
                        transaction.PayoutBatchId, ct);
                }
                else
                {
                    transaction.Status = transaction.RetryCount >= MaxRetryCount
                        ? PayoutTransactionStatus.ManualRequired
                        : PayoutTransactionStatus.Failed;
                    transaction.FailureReason =
                        $"[Retry {transaction.RetryCount}] {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                transaction.Status = PayoutTransactionStatus.Failed;
                transaction.FailureReason = ex.Message;
                _logger.LogError(ex,
                    "[Payout] Exception retrying transaction {Id}", transactionId);
            }

            await _payoutTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // ────────────────────────────────────────────────────────────
        public async Task RetryAllFailedTransactionsAsync(
            CancellationToken ct = default)
        {
            var failedTransactions = await _payoutTransactionRepository
                .GetFailedWithRetryRemainingAsync(MaxRetryCount, ct);

            _logger.LogInformation(
                "[Payout] Retrying {Count} failed transactions",
                failedTransactions.Count);

            foreach (var transaction in failedTransactions)
            {
                await RetryTransactionAsync(transaction.Id, ct);
            }
        }

        // ────────────────────────────────────────────────────────────
        public async Task HoldBatchAsync(
            Guid batchId,
            string reason,
            CancellationToken ct = default)
        {
            var batch = await _payoutBatchRepository.GetByIdAsync(batchId, ct)
                ?? throw new KeyNotFoundException($"PayoutBatch with id {batchId} not found");

            if (batch.Status != PayoutBatchStatus.PendingApproval)
                throw new InvalidOperationException(
                    $"Can only hold batches in PendingApproval state.");

            batch.Status = PayoutBatchStatus.OnHold;
            batch.HoldReason = reason;

            await _payoutBatchRepository.UpdateAsync(batch);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // ────────────────────────────────────────────────────────────
        public async Task ExcludeTransactionAsync(
            Guid transactionId,
            string reason,
            CancellationToken ct = default)
        {
            var transaction = await _payoutTransactionRepository
                .GetByIdAsync(transactionId, ct)
                ?? throw new KeyNotFoundException($"PayoutTransaction with id {transactionId} not found");

            if (transaction.Status != PayoutTransactionStatus.Queued)
                throw new InvalidOperationException(
                    "Can only exclude Queued transactions.");

            transaction.Status = PayoutTransactionStatus.Excluded;
            transaction.ExcludeReason = reason;
            await _payoutTransactionRepository.UpdateAsync(transaction);

            // Unlock orders thuộc transaction này → trả về Unpaid
            var subOrderIds = await _payoutItemRepository
                .GetSubOrderIdsByTransactionIdAsync(transactionId, ct);
            await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                subOrderIds, PayoutStatus.Unpaid, ct);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static void RecalculateBatchStatus(PayoutBatch batch)
        {
            var total = batch.Transactions.Count;
            var success = batch.Transactions.Count(t =>
                t.Status == PayoutTransactionStatus.Success);
            var excluded = batch.Transactions.Count(t =>
                t.Status == PayoutTransactionStatus.Excluded);
            var failed = batch.Transactions.Count(t =>
                t.Status == PayoutTransactionStatus.Failed ||
                t.Status == PayoutTransactionStatus.ManualRequired);

            batch.SuccessCount = success;
            batch.FailedCount = failed;
            batch.CompletedAt = DateTime.UtcNow;

            batch.Status = (success + excluded == total)
                ? PayoutBatchStatus.Completed
                : PayoutBatchStatus.PartialFailed;
        }

        private async Task RecalculateParentBatchAsync(
            Guid batchId,
            CancellationToken ct)
        {
            var batch = await _payoutBatchRepository
                .GetByIdWithTransactionsAsync(batchId, ct);
            if (batch is null) return;

            RecalculateBatchStatus(batch);
            await _payoutBatchRepository.UpdateAsync(batch);
        }

        private static int GetWeekNumber(DateTime date)
        {
            var cal = System.Globalization.CultureInfo
                .InvariantCulture.Calendar;
            return cal.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }
    }
}

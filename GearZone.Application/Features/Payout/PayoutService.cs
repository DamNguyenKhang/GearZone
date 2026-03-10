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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayoutClient _payoutClient;
        private readonly ILogger<PayoutService> _logger;

        public PayoutService(IUnitOfWork unitOfWork, IPayoutClient payoutClient, ILogger<PayoutService> logger)
        {
            _unitOfWork = unitOfWork;
            _payoutClient = payoutClient;
            _logger = logger;
        }

        public async Task<Result<Guid>> GenerateWeeklyBatchAsync(DateTime endDate, CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork.OrderRepository.GetQueryable()
                .Include(o => o.Store)
                .Where(o => o.Status == OrderStatus.Delivered &&
                            o.PayoutStatus == PayoutStatus.Unpaid &&
                            (o.UpdatedAt ?? o.CreatedAt) <= endDate)
                .ToListAsync(cancellationToken);

            if (!orders.Any())
            {
                return Result<Guid>.Failure(new Error("PayoutService.NoOrders", "No eligible orders found for payout."));
            }

            var batch = new PayoutBatch
            {
                BatchCode = "PAY-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper(),
                PeriodStart = orders.Min(o => o.UpdatedAt ?? o.CreatedAt),
                PeriodEnd = endDate,
                Status = PayoutBatchStatus.PendingApproval,
                CreatedAt = DateTime.UtcNow,
                Transactions = new List<PayoutTransaction>()
            };

            var ordersByStore = orders.GroupBy(o => o.StoreId);

            foreach (var storeGroup in ordersByStore)
            {
                var store = storeGroup.First().Store;
                var transaction = new PayoutTransaction
                {
                    StoreId = store.Id,
                    BankName = string.IsNullOrEmpty(store.BankName) ? "Unknown" : store.BankName,
                    BankAccountNumber = string.IsNullOrEmpty(store.BankAccountNumber) ? "Unknown" : store.BankAccountNumber,
                    BankAccountName = string.IsNullOrEmpty(store.BankAccountName) ? "Unknown" : store.BankAccountName,
                    Status = PayoutTransactionStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<PayoutItem>()
                };

                foreach (var order in storeGroup)
                {
                    var item = new PayoutItem
                    {
                        OrderId = order.Id,
                        GrandTotal = order.GrandTotal,
                        CommissionAmount = order.CommissionAmount,
                        NetAmount = order.GrandTotal - order.CommissionAmount
                    };

                    transaction.Items.Add(item);
                    
                    // Mark order as pending payout so it won't be picked up again
                    order.PayoutStatus = PayoutStatus.Pending;
                }

                transaction.OrderCount = transaction.Items.Count;
                transaction.GrossAmount = transaction.Items.Sum(i => i.GrandTotal);
                transaction.CommissionAmount = transaction.Items.Sum(i => i.CommissionAmount);
                transaction.NetAmount = transaction.Items.Sum(i => i.NetAmount);

                batch.Transactions.Add(transaction);
            }

            batch.TotalGrossAmount = batch.Transactions.Sum(t => t.GrossAmount);
            batch.TotalCommissionAmount = batch.Transactions.Sum(t => t.CommissionAmount);
            batch.TotalNetAmount = batch.Transactions.Sum(t => t.NetAmount);
            batch.TotalStores = batch.Transactions.Count;

            await _unitOfWork.PayoutBatchRepository.AddAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(batch.Id);
        }

        public async Task<Result<bool>> ProcessPayoutTransactionAsync(string transactionCode, CancellationToken cancellationToken = default)
        {
            // Wait, PayoutTransaction doesn't have a TransactionCode property, but batch has BatchCode.
            // Let's assume we pass the string representation of Guid.
            if (!Guid.TryParse(transactionCode, out var transactionId))
            {
                 return Result<bool>.Failure(new Error("PayoutService.InvalidId", "Invalid transaction ID format."));
            }

            var transaction = await _unitOfWork.PayoutTransactionRepository.GetQueryable()
                .Include(t => t.Store)
                .Include(t => t.Items)
                .ThenInclude(i => i.Order)
                .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

            if (transaction == null)
            {
                return Result<bool>.Failure(new Error("PayoutService.NotFound", "Transaction not found."));
            }

            if (transaction.Status != PayoutTransactionStatus.Pending)
            {
                return Result<bool>.Failure(new Error("PayoutService.InvalidState", "Transaction is not in Pending state."));
            }

            var requestDto = new PayoutRequestDto
            {
                Amount = (long)transaction.NetAmount,
                Description = $"PY {transaction.Id.ToString().Substring(0, 8).ToUpper()}",
                ToAccountNumber = transaction.BankAccountNumber,
                ToBin = "970436" // Default mock BIN
            };

            transaction.Status = PayoutTransactionStatus.Processing;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var payoutResult = await _payoutClient.CreatePayoutAsync(requestDto);

            if (payoutResult.Success)
            {
                transaction.Status = PayoutTransactionStatus.Processing; 
                transaction.PayOSTransactionId = payoutResult.ReferenceId;
                // Next we wait for webhook to mark as Completed
            }
            else
            {
                transaction.Status = PayoutTransactionStatus.Failed;
                transaction.FailureReason = payoutResult.ErrorMessage;
                // Revert orders payout status? Wait, usually we just update transaction status so it can be retried or handled manually.
            }

            transaction.ProcessedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ProcessPayoutBatchAsync(string batchCode, CancellationToken cancellationToken = default)
        {
            var batch = await _unitOfWork.PayoutBatchRepository.GetQueryable()
                .Include(b => b.Transactions)
                .FirstOrDefaultAsync(b => b.BatchCode == batchCode, cancellationToken);

            if (batch == null)
            {
                return Result<bool>.Failure(new Error("PayoutService.NotFound", "Batch not found."));
            }

            if (batch.Status != PayoutBatchStatus.PendingApproval)
            {
                return Result<bool>.Failure(new Error("PayoutService.InvalidState", "Batch is not in Pending Approval state."));
            }

            batch.Status = PayoutBatchStatus.Processing;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var transaction in batch.Transactions.Where(t => t.Status == PayoutTransactionStatus.Pending))
            {
                await ProcessPayoutTransactionAsync(transaction.Id.ToString(), cancellationToken);
            }

            // Webhook will figure out when all transactions are done and complete the batch.
            return Result<bool>.Success(true);
        }
    }
}

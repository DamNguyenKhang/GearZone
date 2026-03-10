using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public class PayoutTransactionRepository : Repository<PayoutTransaction, Guid>, IPayoutTransactionRepository
{
    public PayoutTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PayoutTransaction?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Batch)
            .Include(x => x.Store)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<PayoutTransaction>> GetByBatchIdAsync(Guid batchId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Store)
            .Where(x => x.PayoutBatchId == batchId)
            .ToListAsync(ct);
    }

    public async Task<List<PayoutTransaction>> GetFailedWithRetryRemainingAsync(int maxRetry, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.Status == PayoutTransactionStatus.Failed && x.RetryCount < maxRetry)
            .ToListAsync(ct);
    }

    public Task UpdateRangeAsync(List<PayoutTransaction> transactions, CancellationToken ct = default)
    {
        _dbSet.UpdateRange(transactions);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<PayoutTransaction>> GetByStoreIdAsync(Guid storeId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(x => x.Batch)
            .Where(x => x.StoreId == storeId);
            
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PayoutTransaction>(items, totalCount, page, pageSize);
    }
}

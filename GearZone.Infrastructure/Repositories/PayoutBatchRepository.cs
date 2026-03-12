using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public class PayoutBatchRepository : Repository<PayoutBatch, Guid>, IPayoutBatchRepository
{
    public PayoutBatchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PayoutBatch?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Transactions)
                .ThenInclude(t => t.Store)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsByPeriodAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(x => x.PeriodStart == periodStart && x.PeriodEnd == periodEnd, ct);
    }

    public async Task<PagedResult<PayoutBatch>> GetPagedAsync(int page, int pageSize, PayoutBatchStatus? status = null, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PayoutBatch>(items, totalCount, page, pageSize);
    }
}

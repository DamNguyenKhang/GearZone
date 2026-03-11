using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutBatchRepository : IRepository<PayoutBatch, Guid>
{
    Task<PayoutBatch?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByPeriodAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);

    Task<PagedResult<PayoutBatch>> GetPagedAsync(int page, int pageSize, PayoutBatchStatus? status = null, CancellationToken ct = default);
}

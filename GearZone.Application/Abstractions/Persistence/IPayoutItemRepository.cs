using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutItemRepository : IRepository<PayoutItem, Guid>
{
    Task<List<PayoutItem>> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default);

    Task<List<Guid>> GetOrderIdsByTransactionIdAsync(Guid transactionId, CancellationToken ct = default);

    Task<List<Guid>> GetOrderIdsByTransactionIdsAsync(List<Guid> transactionIds, CancellationToken ct = default);
}

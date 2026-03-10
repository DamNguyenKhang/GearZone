using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutItemRepository : IRepository<PayoutItem, Guid>
{
}

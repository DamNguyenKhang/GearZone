using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;

namespace GearZone.Infrastructure.Repositories;

public class PayoutItemRepository : Repository<PayoutItem, Guid>, IPayoutItemRepository
{
    public PayoutItemRepository(ApplicationDbContext context) : base(context)
    {
    }
}

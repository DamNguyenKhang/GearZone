using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;

namespace GearZone.Infrastructure.Repositories;

public class PayoutBatchRepository : Repository<PayoutBatch, Guid>, IPayoutBatchRepository
{
    public PayoutBatchRepository(ApplicationDbContext context) : base(context)
    {
    }
}

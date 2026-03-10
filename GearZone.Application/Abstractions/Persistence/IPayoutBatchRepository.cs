using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutBatchRepository : IRepository<PayoutBatch, Guid>
{
}

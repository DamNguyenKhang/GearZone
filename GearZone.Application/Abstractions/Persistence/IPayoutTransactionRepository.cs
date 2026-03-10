using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutTransactionRepository : IRepository<PayoutTransaction, Guid>
{
}

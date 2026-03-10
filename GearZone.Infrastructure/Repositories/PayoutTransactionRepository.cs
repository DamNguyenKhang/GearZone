using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;

namespace GearZone.Infrastructure.Repositories;

public class PayoutTransactionRepository : Repository<PayoutTransaction, Guid>, IPayoutTransactionRepository
{
    public PayoutTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }
}

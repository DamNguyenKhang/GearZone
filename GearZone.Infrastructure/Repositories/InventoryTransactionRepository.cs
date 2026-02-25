using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class InventoryTransactionRepository : Repository<InventoryTransaction, Guid>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

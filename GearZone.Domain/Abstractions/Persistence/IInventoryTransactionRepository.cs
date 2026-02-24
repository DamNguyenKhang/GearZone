using GearZone.Domain.Entities;
using System;

namespace GearZone.Domain.Abstractions.Persistence
{
    public interface IInventoryTransactionRepository : IRepository<InventoryTransaction, Guid>
    {
    }
}

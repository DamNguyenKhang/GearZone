using GearZone.Application.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IInventoryTransactionRepository : IRepository<InventoryTransaction, Guid>
    {
    }
}

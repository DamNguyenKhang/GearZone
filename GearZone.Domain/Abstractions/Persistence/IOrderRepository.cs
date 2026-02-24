using GearZone.Domain.Entities;
using System;

namespace GearZone.Domain.Abstractions.Persistence
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
    }
}

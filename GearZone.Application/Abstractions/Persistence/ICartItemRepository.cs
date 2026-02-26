using GearZone.Domain.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ICartItemRepository : IRepository<CartItem, Guid>
    {
    }
}

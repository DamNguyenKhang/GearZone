using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class CartItemRepository : Repository<CartItem, Guid>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

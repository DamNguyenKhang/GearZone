using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class CartRepository : Repository<Cart, Guid>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

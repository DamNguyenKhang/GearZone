using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class OrderItemRepository : Repository<OrderItem, Guid>, IOrderItemRepository
    {
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

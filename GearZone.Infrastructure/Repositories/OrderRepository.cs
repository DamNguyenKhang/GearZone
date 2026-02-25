using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

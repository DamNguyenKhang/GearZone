using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product, Guid>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

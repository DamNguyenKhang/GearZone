using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class ProductImageRepository : Repository<ProductImage, Guid>, IProductImageRepository
    {
        public ProductImageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

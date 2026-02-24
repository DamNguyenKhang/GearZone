using GearZone.Domain.Entities;
using System;

namespace GearZone.Domain.Abstractions.Persistence
{
    public interface IProductVariantRepository : IRepository<ProductVariant, Guid>
    {
    }
}

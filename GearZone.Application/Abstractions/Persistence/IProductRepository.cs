using GearZone.Application.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductRepository : IRepository<Product, Guid>
    {
    }
}

using System;
using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductRepository : IRepository<Product, Guid>
    {
        Task<PagedResult<CatalogProductDto>> GetFilteredProductsAsync(ProductFilterDto filter);
    }
}

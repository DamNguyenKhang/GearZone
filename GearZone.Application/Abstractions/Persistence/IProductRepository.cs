using System;
using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductRepository : IRepository<Product, Guid>
    {
        Task<PagedResult<Product>> GetFilteredProductsAsync(ProductFilterDto filter);
        Task<PagedResult<Product>> GetAdminProductsAsync(AdminProductQueryDto query);
        Task<AdminProductStatsDto> GetAdminProductStatsAsync();
        Task<Product?> GetAdminProductDetailAsync(Guid id);
    }
}

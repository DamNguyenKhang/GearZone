using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Catalog
{
    public interface ICatalogService
    {
        Task<PagedResult<CatalogProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<CatalogFilterSidebarDto> GetFiltersForCategoryAsync(string categorySlug);
        Task<List<CatalogCategoryDto>> GetCategoriesAsync();
        Task<ProductDetailDto?> GetProductDetailBySlugAsync(string slug);
        Task<List<CatalogProductDto>> GetRelatedProductsAsync(int categoryId, Guid currentProductId, int limit = 5);
    }

    public class CatalogCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public List<CatalogCategoryDto> SubCategories { get; set; } = new();
    }

    public class CatalogFilterSidebarDto
    {
        public List<BrandFilterDto> Brands { get; set; } = new();
        public List<CategoryAttributeFilterDto> Attributes { get; set; } = new();
    }

    public class BrandFilterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    public class CategoryAttributeFilterDto
    {
        public string Name { get; set; } = string.Empty;
        public string FilterType { get; set; } = string.Empty;
        public List<AttributeValueFilterDto> Values { get; set; } = new();
    }

    public class AttributeValueFilterDto
    {
        public string Value { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}

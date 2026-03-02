using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public class ProductCardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string PrimaryImageUrl { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int? DiscountPercent { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductImageDetailDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class StoreInfoDto
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }

    public class BreadcrumbItem
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
    }

    public class ProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string> Specs { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Price
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        // Rating (placeholder for Sprint 3)
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Relations
        public List<ProductImageDetailDto> Images { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public StoreInfoDto Store { get; set; } = new();
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public string CategoryName { get; set; } = string.Empty;
    }

    public interface ICatalogService
    {
        Task<List<ProductCardDto>> GetProductCardsAsync(int page = 1, int pageSize = 12);
        Task<List<ProductCardDto>> GetFlashSaleProductsAsync(int count = 4);
        Task<ProductCardDto?> GetProductCardByIdAsync(Guid productId);
        Task<ProductDetailDto?> GetProductDetailBySlugAsync(string slug);
        Task<List<ProductDetailDto>> GetProductsForCompareAsync(List<Guid> productIds);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public class ProductCardViewModel
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

        // Computed display properties
        public string FormattedPrice => MinPrice.ToString("#,0").Replace(",", ".") + " ₫";
        public string? FormattedOriginalPrice => OriginalPrice?.ToString("#,0").Replace(",", ".") + " ₫";
        public int FullStars => (int)Math.Floor(AverageRating);
        public bool HasHalfStar => AverageRating - FullStars >= 0.25 && AverageRating - FullStars < 0.75;
        public int EmptyStars => 5 - FullStars - (HasHalfStar ? 1 : 0);
    }

    public class ProductVariantViewModel
    {
        public Guid Id { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    // ProductImageViewModel is defined in IProductImageService.cs

    public class StoreInfoViewModel
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

    public class ProductDetailViewModel
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

        // Rating
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Relations
        public List<ProductImageViewModel> Images { get; set; } = new();
        public List<ProductVariantViewModel> Variants { get; set; } = new();
        public StoreInfoViewModel Store { get; set; } = new();
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public string CategoryName { get; set; } = string.Empty;
    }

    public interface ICatalogService
    {
        Task<List<ProductCardViewModel>> GetProductCardsAsync(int page = 1, int pageSize = 12);
        Task<List<ProductCardViewModel>> GetFlashSaleProductsAsync(int count = 4);
        Task<ProductCardViewModel?> GetProductCardByIdAsync(Guid productId);
        Task<ProductDetailViewModel?> GetProductDetailBySlugAsync(string slug);
        Task<List<ProductDetailViewModel>> GetProductsForCompareAsync(List<Guid> productIds);
    }
}

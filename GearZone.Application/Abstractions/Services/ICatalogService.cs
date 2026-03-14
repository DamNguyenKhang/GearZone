using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface ICatalogService
    {
        Task<PagedResult<CatalogProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<CatalogFilterSidebarDto> GetFiltersForCategoryAsync(string categorySlug);
        Task<List<CatalogCategoryDto>> GetCategoriesAsync();
        Task<StoreProfileDto?> GetStoreProfileAsync(string slug, string? currentUserId = null);

        // Follow
        Task<bool> ToggleFollowAsync(string userId, Guid storeId);
        Task<bool> IsFollowingAsync(string userId, Guid storeId);
        Task<int> GetFollowerCountAsync(Guid storeId);

        // Chat
        Task<ChatMessageDto> SendMessageAsync(string userId, Guid storeId, string content);
        Task<List<ChatMessageDto>> GetMessagesAsync(string userId, Guid storeId, int page = 1, int pageSize = 50);

        Task<ProductDetailDto?> GetProductDetailBySlugAsync(string slug);
        Task<List<CatalogProductDto>> GetRelatedProductsAsync(int categoryId, Guid currentProductId, int limit = 5);
        Task<List<ProductSuggestionDto>> GetProductSuggestionsAsync(string query, int limit = 5);
        Task<ProductComparisonDto?> GetProductComparisonAsync(int categoryId, List<Guid> productIds);
    }

    public class StoreProfileDto
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string Province { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalSold { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public int FollowerCount { get; set; }
        public bool IsFollowing { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string SenderUserId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsFromStore { get; set; }
        
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

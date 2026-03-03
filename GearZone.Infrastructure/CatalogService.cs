using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GearZone.Infrastructure
{
    public class CatalogService : ICatalogService
    {
        private readonly ApplicationDbContext _context;

        public CatalogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductCardViewModel>> GetProductCardsAsync(int page = 1, int pageSize = 12)
        {
            var products = await GetBaseProductQuery()
                .Where(p => p.Status == "Active")
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products.Select(MapToCardViewModel).ToList();
        }

        public async Task<List<ProductCardViewModel>> GetFlashSaleProductsAsync(int count = 4)
        {
            var products = await GetBaseProductQuery()
                .Where(p => p.Status == "Active")
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            return products.Select(MapToCardViewModel).ToList();
        }

        public async Task<ProductCardViewModel?> GetProductCardByIdAsync(Guid productId)
        {
            var product = await GetBaseProductQuery()
                .Where(p => p.Id == productId)
                .FirstOrDefaultAsync();

            return product == null ? null : MapToCardViewModel(product);
        }

        public async Task<ProductDetailViewModel?> GetProductDetailBySlugAsync(string slug)
        {
            var product = await GetBaseProductQuery()
                .Include(p => p.Category)
                    .ThenInclude(c => c.Parent)
                .Where(p => p.Slug == slug)
                .FirstOrDefaultAsync();

            if (product == null) return null;

            return MapToDetailViewModel(product);
        }

        public async Task<List<ProductDetailViewModel>> GetProductsForCompareAsync(List<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
                return new List<ProductDetailViewModel>();

            // Limit to 3 products max
            var ids = productIds.Take(3).ToList();

            var products = await GetBaseProductQuery()
                .Include(p => p.Category)
                    .ThenInclude(c => c.Parent)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            return products.Select(MapToDetailViewModel).ToList();
        }

        // ========== Private Helpers ==========

        /// <summary>
        /// Base query với Include chung (Images, Variants, Store, Category) + filter IsDeleted + AsNoTracking.
        /// Mỗi caller chỉ cần thêm Where/OrderBy/Take riêng.
        /// </summary>
        private IQueryable<Product> GetBaseProductQuery()
        {
            return _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Store)
                .Include(p => p.Category)
                .AsNoTracking();
        }

        // ========== Private Mapping Methods ==========

        /// <summary>
        /// Map Product entity → ProductDetailViewModel (dùng chung cho GetProductDetailBySlugAsync & GetProductsForCompareAsync)
        /// </summary>
        private static ProductDetailViewModel MapToDetailViewModel(Product product)
        {
            var activeVariants = product.Variants
                .Where(v => v.IsActive && !v.IsDeleted)
                .OrderBy(v => v.Price)
                .ToList();

            // Parse SpecsJson
            var specs = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(product.SpecsJson) && product.SpecsJson != "{}")
            {
                try
                {
                    specs = JsonSerializer.Deserialize<Dictionary<string, string>>(product.SpecsJson)
                            ?? new Dictionary<string, string>();
                }
                catch
                {
                    // Invalid JSON, keep empty
                }
            }

            return new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Specs = specs,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                MinPrice = activeVariants.Any() ? activeVariants.Min(v => v.Price) : 0m,
                MaxPrice = activeVariants.Any() ? activeVariants.Max(v => v.Price) : 0m,
                // TODO: Rating được override bởi ReviewService trong PageModel
                AverageRating = 0,
                ReviewCount = 0,
                Images = product.Images
                    .OrderBy(i => i.IsPrimary ? 0 : 1)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => new ProductImageViewModel
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder
                    }).ToList(),
                Variants = activeVariants.Select(v => new ProductVariantViewModel
                {
                    Id = v.Id,
                    VariantName = v.VariantName,
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    IsActive = v.IsActive
                }).ToList(),
                Store = new StoreInfoViewModel
                {
                    Id = product.Store?.Id ?? Guid.Empty,
                    StoreName = product.Store?.StoreName ?? string.Empty,
                    Slug = product.Store?.Slug ?? string.Empty,
                    LogoUrl = product.Store?.LogoUrl
                },
                Breadcrumbs = BuildBreadcrumbs(product.Category),
                CategoryName = product.Category?.Name ?? string.Empty
            };
        }

        /// <summary>
        /// Map Product entity → ProductCardViewModel (cho card hiển thị)
        /// </summary>
        private static ProductCardViewModel MapToCardViewModel(Product product)
        {
            var primaryImage = product.Images
                .OrderBy(i => i.IsPrimary ? 0 : 1)
                .ThenBy(i => i.SortOrder)
                .FirstOrDefault();

            var activeVariants = product.Variants
                .Where(v => v.IsActive && !v.IsDeleted)
                .ToList();

            var minPrice = activeVariants.Any()
                ? activeVariants.Min(v => v.Price)
                : 0m;

            return new ProductCardViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                PrimaryImageUrl = primaryImage?.ImageUrl ?? string.Empty,
                MinPrice = minPrice,
                OriginalPrice = null,
                DiscountPercent = null,
                AverageRating = 0,
                ReviewCount = 0,
                StoreName = product.Store?.StoreName ?? string.Empty,
                CategoryName = product.Category?.Name ?? string.Empty
            };
        }

        private static List<BreadcrumbItem> BuildBreadcrumbs(Category? category)
        {
            var items = new List<BreadcrumbItem>();
            var current = category;
            while (current != null)
            {
                items.Insert(0, new BreadcrumbItem
                {
                    Name = current.Name,
                    Slug = current.Slug
                });
                current = current.Parent;
            }
            return items;
        }
    }
}

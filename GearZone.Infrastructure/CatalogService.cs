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

        public async Task<List<ProductCardDto>> GetProductCardsAsync(int page = 1, int pageSize = 12)
        {
            var products = await _context.Products
                .Where(p => p.Status == "Active" && !p.IsDeleted)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Store)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task<List<ProductCardDto>> GetFlashSaleProductsAsync(int count = 4)
        {
            var products = await _context.Products
                .Where(p => p.Status == "Active" && !p.IsDeleted)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Store)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductCardDto?> GetProductCardByIdAsync(Guid productId)
        {
            var product = await _context.Products
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Store)
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDetailDto?> GetProductDetailBySlugAsync(string slug)
        {
            var product = await _context.Products
                .Where(p => p.Slug == slug && !p.IsDeleted)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Store)
                .Include(p => p.Category)
                    .ThenInclude(c => c.Parent)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (product == null) return null;

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

            // Build breadcrumbs
            var breadcrumbs = BuildBreadcrumbs(product.Category);

            return new ProductDetailDto
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
                AverageRating = 0,
                ReviewCount = 0,
                Images = product.Images
                    .OrderBy(i => i.IsPrimary ? 0 : 1)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => new ProductImageDetailDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder
                    }).ToList(),
                Variants = activeVariants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    VariantName = v.VariantName,
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    IsActive = v.IsActive
                }).ToList(),
                Store = new StoreInfoDto
                {
                    Id = product.Store?.Id ?? Guid.Empty,
                    StoreName = product.Store?.StoreName ?? string.Empty,
                    Slug = product.Store?.Slug ?? string.Empty,
                    LogoUrl = product.Store?.LogoUrl
                },
                Breadcrumbs = breadcrumbs,
                CategoryName = product.Category?.Name ?? string.Empty
            };
        }

        public async Task<List<ProductDetailDto>> GetProductsForCompareAsync(List<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
                return new List<ProductDetailDto>();

            // Limit to 3 products max
            var ids = productIds.Take(3).ToList();

            var products = await _context.Products
                .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Store)
                .Include(p => p.Category)
                    .ThenInclude(c => c.Parent)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<ProductDetailDto>();
            foreach (var product in products)
            {
                var activeVariants = product.Variants
                    .Where(v => v.IsActive && !v.IsDeleted)
                    .OrderBy(v => v.Price)
                    .ToList();

                var specs = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(product.SpecsJson) && product.SpecsJson != "{}")
                {
                    try
                    {
                        specs = JsonSerializer.Deserialize<Dictionary<string, string>>(product.SpecsJson)
                                ?? new Dictionary<string, string>();
                    }
                    catch { }
                }

                result.Add(new ProductDetailDto
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
                    AverageRating = 0,
                    ReviewCount = 0,
                    Images = product.Images
                        .OrderBy(i => i.IsPrimary ? 0 : 1)
                        .ThenBy(i => i.SortOrder)
                        .Select(i => new ProductImageDetailDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            IsPrimary = i.IsPrimary,
                            SortOrder = i.SortOrder
                        }).ToList(),
                    Variants = activeVariants.Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        VariantName = v.VariantName,
                        Sku = v.Sku,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity,
                        IsActive = v.IsActive
                    }).ToList(),
                    Store = new StoreInfoDto
                    {
                        Id = product.Store?.Id ?? Guid.Empty,
                        StoreName = product.Store?.StoreName ?? string.Empty,
                        Slug = product.Store?.Slug ?? string.Empty,
                        LogoUrl = product.Store?.LogoUrl
                    },
                    Breadcrumbs = BuildBreadcrumbs(product.Category),
                    CategoryName = product.Category?.Name ?? string.Empty
                });
            }

            return result;
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

        private static ProductCardDto MapToDto(Product product)
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

            return new ProductCardDto
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
    }
}

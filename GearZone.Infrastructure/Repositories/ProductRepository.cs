using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Enums;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product, Guid>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<CatalogProductDto>> GetFilteredProductsAsync(ProductFilterDto filter)
        {
            // Use AsNoTracking for read-only query performance
            // REMOVED: All .Include() calls to avoid Cartesian explosion and over-fetching
            var query = _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active);

            if (!string.IsNullOrEmpty(filter.CategorySlug))
            {
                query = query.Where(p => p.Category.Slug == filter.CategorySlug);
            }

            if (filter.BrandSlugs != null && filter.BrandSlugs.Any())
            {
                query = query.Where(p => filter.BrandSlugs.Contains(p.Brand.Slug));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);
            }

            if (filter.InStockOnly == true)
            {
                query = query.Where(p => p.Variants.Any(v => v.StockQuantity > 0));
            }

            if (filter.Attributes != null && filter.Attributes.Any())
            {
                foreach (var attr in filter.Attributes)
                {
                    var attrName = attr.Key;
                    var attrValues = attr.Value;
                    if (attrValues != null && attrValues.Any())
                    {
                        query = query.Where(p => p.Variants.Any(v => 
                            v.AttributeValues.Any(av => 
                                av.CategoryAttribute.Name == attrName && 
                                attrValues.Contains(av.CategoryAttributeOption.Value)
                            )
                        ));
                    }
                }
            }

            // Efficient CountAsync: Navigation loading is stripped out by EF for counts
            var totalCount = await query.CountAsync();

            // Ordering - Add ThenBy(p => p.Id) for deterministic pagination
            query = (filter.SortBy?.ToLower()) switch
            {
                "newest" => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id),
                "price_asc" => query.OrderBy(p => p.BasePrice).ThenBy(p => p.Id),
                "price_desc" => query.OrderByDescending(p => p.BasePrice).ThenBy(p => p.Id),
                _ => query.OrderByDescending(p => p.SoldCount).ThenBy(p => p.Id) // Default "popular"
            };

            // PRODUCTION OPTIMIZED PROJECTION
            // Maps directly to DTO to fetch ONLY required columns and avoid Cartesian joins
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new CatalogProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    BrandName = p.Brand.Name,
                    BasePrice = p.BasePrice,
                    ImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() 
                               ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? "",
                    Rating = 0, // Placeholder
                    ReviewCount = 0,
                    StoreName = p.Store.StoreName,
                    StoreLogoUrl = p.Store.LogoUrl,
                    IsInStock = p.Variants.Any(v => v.StockQuantity > 0),
                    HighlightTags = p.Variants
                        .SelectMany(v => v.AttributeValues)
                        .Select(av => av.CategoryAttributeOption.Value)
                        .Distinct()
                        .Take(3)
                        .ToList()
                })
                .ToListAsync();

            return new PagedResult<CatalogProductDto>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<PagedResult<Product>> GetAdminProductsAsync(AdminProductQueryDto queryDto)
        {
            var query = _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                .Include(p => p.Images)
                .AsQueryable();

            // Search across Name, SKU (any variant) and StoreName simultaneously
            if (!string.IsNullOrEmpty(queryDto.SearchTerm))
            {
                var searchTerm = queryDto.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Variants.Any(v => v.Sku.ToLower().Contains(searchTerm)) ||
                    p.Store.StoreName.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(queryDto.Status))
            {
                if (Enum.TryParse<ProductStatus>(queryDto.Status, true, out var status))
                {
                    query = query.Where(p => p.Status == status);
                }
            }

            if (queryDto.CategoryId.HasValue && queryDto.CategoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == queryDto.CategoryId.Value);
            }

            if (queryDto.BrandId.HasValue && queryDto.BrandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == queryDto.BrandId.Value);
            }

            if (queryDto.StoreId.HasValue && queryDto.StoreId.Value != Guid.Empty)
            {
                query = query.Where(p => p.StoreId == queryDto.StoreId.Value);
            }

            if (queryDto.MinPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= queryDto.MinPrice.Value);
            }

            if (queryDto.MaxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= queryDto.MaxPrice.Value);
            }

            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= queryDto.StartDate.Value);
            }

            if (queryDto.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= queryDto.EndDate.Value);
            }

            if (queryDto.OutOfStock)
            {
                query = query.Where(p => !p.Variants.Any(v => v.StockQuantity > 0));
            }

            // Filter by selected attribute option IDs (products must have at least one matching variant)
            if (queryDto.AttributeOptionIds != null && queryDto.AttributeOptionIds.Any())
            {
                query = query.Where(p =>
                    p.Variants.Any(v =>
                        v.AttributeValues.Any(av =>
                            queryDto.AttributeOptionIds.Contains(av.CategoryAttributeOptionId))));
            }

            // Dynamic sort: SortBy controls column, SortDirection controls asc/desc
            // If no SortBy, default to newest first
            var sortBy = (queryDto.SortBy ?? "").ToLower();
            var isAsc = (queryDto.SortDirection ?? "").ToLower() == "asc";

            query = sortBy switch
            {
                "stock" => isAsc
                    ? query.OrderBy(p => p.Variants.Sum(v => (int?)v.StockQuantity) ?? 0).ThenBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.Variants.Sum(v => (int?)v.StockQuantity) ?? 0).ThenByDescending(p => p.CreatedAt),
                "price" => isAsc
                    ? query.OrderBy(p => p.BasePrice).ThenBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.BasePrice).ThenByDescending(p => p.CreatedAt),
                "createdat" => isAsc
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // default
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<AdminProductStatsDto> GetAdminProductStatsAsync()
        {
            var totalProducts = await _context.Products.CountAsync();
            var activeProducts = await _context.Products.CountAsync(p => p.Status == ProductStatus.Active);
            var pendingApproval = await _context.Products.CountAsync(p => p.Status == ProductStatus.Pending);
            
            // Out of stock means no variant has stock > 0
            var outOfStock = await _context.Products.CountAsync(p => !p.Variants.Any(v => v.StockQuantity > 0));

            return new AdminProductStatsDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                PendingApproval = pendingApproval,
                OutOfStock = outOfStock
            };
        }

        public async Task<Product?> GetAdminProductDetailAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}

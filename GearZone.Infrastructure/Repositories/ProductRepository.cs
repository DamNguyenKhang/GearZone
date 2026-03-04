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

        public async Task<PagedResult<Product>> GetFilteredProductsAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active);

            if (!string.IsNullOrEmpty(filter.CategorySlug))
            {
                // This could be enhanced to include child categories if needed
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
                                attrValues.Contains(av.Value)
                            )
                        ));
                    }
                }
            }

            // Ordering
            query = filter.SortBy switch
            {
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "price_asc" => query.OrderBy(p => p.BasePrice),
                "price_desc" => query.OrderByDescending(p => p.BasePrice),
                // "rating" => query.OrderByDescending(p => p.AverageRating), // Add later if Rating is implemented
                _ => query.OrderByDescending(p => p.SoldCount) // Default "popular"
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<PagedResult<Product>> GetAdminProductsAsync(AdminProductQueryDto queryDto)
        {
            var query = _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(queryDto.SearchTerm))
            {
                var searchTerm = queryDto.SearchTerm.ToLower();
                if (queryDto.SearchType == "SKU")
                {
                    query = query.Where(p => p.Variants.Any(v => v.Sku.ToLower().Contains(searchTerm)));
                }
                else if (queryDto.SearchType == "Store")
                {
                    query = query.Where(p => p.Store.StoreName.ToLower().Contains(searchTerm));
                }
                else // Name
                {
                    query = query.Where(p => p.Name.ToLower().Contains(searchTerm));
                }
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

            query = query.OrderByDescending(p => p.CreatedAt);

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
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}

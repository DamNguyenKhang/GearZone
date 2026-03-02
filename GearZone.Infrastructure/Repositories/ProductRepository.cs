using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Enums;

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
    }
}

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
    }
}

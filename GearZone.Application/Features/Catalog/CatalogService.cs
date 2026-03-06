using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryAttributeRepository _categoryAttributeRepository;

        public CatalogService(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ICategoryAttributeRepository categoryAttributeRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _categoryAttributeRepository = categoryAttributeRepository;
        }

        public async Task<PagedResult<CatalogProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            return await _productRepository.GetFilteredProductsAsync(filter);
        }

        public async Task<List<CatalogCategoryDto>> GetCategoriesAsync()
        {
            return await _categoryRepository.Query()
                .Where(c => c.IsActive && !c.IsDeleted && c.ParentId == null)
                .Select(c => new CatalogCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    SubCategories = c.Children
                        .Where(child => child.IsActive && !child.IsDeleted)
                        .Select(child => new CatalogCategoryDto
                        {
                            Id = child.Id,
                            Name = child.Name,
                            Slug = child.Slug
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<CatalogFilterSidebarDto> GetFiltersForCategoryAsync(string categorySlug)
        {
            var result = new CatalogFilterSidebarDto();

            // Find category and eagerly check if it has children
            var category = await _categoryRepository.Query()
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Slug == categorySlug && c.IsActive);

            if (category == null) return result;

            bool isParent = category.Children != null && category.Children.Any(c => c.IsActive && !c.IsDeleted);

            if (isParent)
            {
                // ── PARENT CATEGORY: aggregate brands from all active children ──
                var childIds = category.Children!
                    .Where(c => c.IsActive && !c.IsDeleted)
                    .Select(c => c.Id)
                    .ToList();

                // Include the parent itself in case it has direct products
                childIds.Add(category.Id);

                result.Brands = await _productRepository.Query()
                    .Where(p => childIds.Contains(p.CategoryId)
                              && p.Brand.IsApproved
                              && p.Status == GearZone.Domain.Enums.ProductStatus.Active
                              && !p.IsDeleted)
                    .GroupBy(p => p.Brand)
                    .Select(g => new BrandFilterDto
                    {
                        Name = g.Key.Name,
                        Slug = g.Key.Slug,
                        ProductCount = g.Count()
                    })
                    .OrderByDescending(b => b.ProductCount)
                    .ToListAsync();

                // Do NOT return dynamic attributes for parent categories:
                // each subcategory has its own unrelated attributes (VRAM vs Socket vs Panel Type…)
                // Filters shown: Brand + Price range only (Price is always rendered on frontend)
            }
            else
            {
                // ── LEAF CATEGORY: original behavior — brands + dynamic attributes ──
                result.Brands = await _productRepository.Query()
                    .Where(p => p.CategoryId == category.Id
                              && p.Brand.IsApproved
                              && p.Status == GearZone.Domain.Enums.ProductStatus.Active
                              && !p.IsDeleted)
                    .GroupBy(p => p.Brand)
                    .Select(g => new BrandFilterDto
                    {
                        Name = g.Key.Name,
                        Slug = g.Key.Slug,
                        ProductCount = g.Count()
                    })
                    .ToListAsync();

                var attributes = await _categoryAttributeRepository.Query()
                    .Where(a => a.CategoryId == category.Id && a.IsFilterable)
                    .OrderBy(a => a.DisplayOrder)
                    .ToListAsync();

                foreach (var attr in attributes)
                {
                    var attrDto = new CategoryAttributeFilterDto
                    {
                        Name = attr.Name,
                        FilterType = attr.FilterType
                    };

                    attrDto.Values = await _productRepository.Query()
                        .Where(p => p.CategoryId == category.Id
                                  && p.Status == GearZone.Domain.Enums.ProductStatus.Active
                                  && !p.IsDeleted)
                        .SelectMany(p => p.Variants)
                        .SelectMany(v => v.AttributeValues)
                        .Where(av => av.CategoryAttributeId == attr.Id)
                        .GroupBy(av => av.CategoryAttributeOption)
                        .Select(g => new AttributeValueFilterDto
                        {
                            Value = g.Key.Value,
                            ProductCount = g.Select(x => x.Variant.ProductId).Distinct().Count()
                        })
                        .OrderBy(x => x.Value)
                        .ToListAsync();

                    if (attrDto.Values.Any())
                        result.Attributes.Add(attrDto);
                }
            }

            return result;
        }

        public async Task<ProductDetailDto?> GetProductDetailBySlugAsync(string slug)
        {
            var product = await _productRepository.Query()
                .AsNoTracking()
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == GearZone.Domain.Enums.ProductStatus.Active && !p.IsDeleted);

            if (product == null) return null;

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                BasePrice = product.BasePrice,
                SoldCount = product.SoldCount,
                BrandName = product.Brand.Name,
                BrandSlug = product.Brand.Slug,
                CategoryName = product.Category.Name,
                CategorySlug = product.Category.Slug,
                StoreId = product.StoreId,
                StoreName = product.Store.StoreName,
                ImageUrls = product.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).ToList(),
            };

            // Process Variants & Generate Attribute Selections
            if (product.Variants.Any())
            {
                var allAttributeValues = product.Variants.SelectMany(v => v.AttributeValues).ToList();

                // 1. Group by Attribute to build the Shopee-style selection UI
                dto.AttributeSelections = allAttributeValues
                    .GroupBy(av => av.CategoryAttributeId)
                    .Select(g => new AttributeSelectionDto
                    {
                        AttributeId = g.Key,
                        Name = g.First().CategoryAttribute.Name,
                        Options = g.Select(av => av.CategoryAttributeOption)
                                   .DistinctBy(opt => opt.Id)
                                   .Select(opt => new AttributeOptionDto
                                   {
                                       OptionId = opt.Id,
                                       Value = opt.Value
                                   }).ToList()
                    }).ToList();

                // 2. Map Variants for JS matching
                dto.Variants = product.Variants.Select(v => new VariantDetailDto
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    SelectedOptionIds = v.AttributeValues.Select(av => av.CategoryAttributeOptionId).ToList()
                }).ToList();

                // 3. Build Specifications = SpecsJson first, then variant attribute aggregations
                var specs = new List<SpecificationDto>();

                // 3a. Parse SpecsJson (static technical specs from product)
                var rawJson = product.SpecsJson;
                if (!string.IsNullOrWhiteSpace(rawJson) && rawJson != "{}")
                {
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(rawJson);
                        if (parsed != null)
                        {
                            specs.AddRange(parsed.Select(kv => new SpecificationDto
                            {
                                Name = kv.Key,
                                Value = kv.Value
                            }));
                        }
                    }
                    catch { /* ignore malformed JSON */ }
                }

                // 3b. Aggregate variant attribute values (e.g. "Memory Type: DDR4, DDR5")
                //     Only add if that attribute name is not already in SpecsJson
                var existingNames = new HashSet<string>(specs.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);
                var variantAttributeSpecs = allAttributeValues
                    .GroupBy(av => av.CategoryAttribute.Name)
                    .Where(g => !existingNames.Contains(g.Key))
                    .Select(g => new SpecificationDto
                    {
                        Name = g.Key,
                        Value = string.Join(", ", g.Select(av => av.CategoryAttributeOption.Value).Distinct())
                    });

                specs.AddRange(variantAttributeSpecs);
                dto.Specifications = specs;
            }

            return dto;
        }

        public async Task<List<CatalogProductDto>> GetRelatedProductsAsync(int categoryId, Guid currentProductId, int limit = 5)
        {
            var relatedProducts = await _productRepository.Query()
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId 
                         && p.Id != currentProductId 
                         && p.Status == GearZone.Domain.Enums.ProductStatus.Active 
                         && !p.IsDeleted)
                .OrderByDescending(p => p.SoldCount) // Best selling in the same category
                .Take(limit)
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
                    IsInStock = p.Variants.Where(v => v.IsActive && !v.IsDeleted).Any(v => v.StockQuantity > 0),
                    HighlightTags = p.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .SelectMany(v => v.AttributeValues)
                        .Select(av => av.CategoryAttributeOption.Value)
                        .Distinct()
                        .Take(3)
                        .ToList()
                })
                .ToListAsync();

            return relatedProducts;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
                .Include(c => c.Children)
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

            // Find category
            var category = await _categoryRepository.Query()
                .FirstOrDefaultAsync(c => c.Slug == categorySlug && c.IsActive);

            if (category == null) return result;

            // 1. Get Brands for this category
            result.Brands = await _productRepository.Query()
                .Where(p => p.CategoryId == category.Id && p.Brand.IsApproved && p.Status == GearZone.Domain.Enums.ProductStatus.Active && !p.IsDeleted)
                .GroupBy(p => p.Brand)
                .Select(g => new BrandFilterDto
                {
                    Name = g.Key.Name,
                    Slug = g.Key.Slug,
                    ProductCount = g.Count()
                })
                .ToListAsync();

            // 2. Get Dynamic Attributes for this category
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

                // Get distinct values and counts for this attribute in this category
                attrDto.Values = await _productRepository.Query()
                    .Where(p => p.CategoryId == category.Id && p.Status == GearZone.Domain.Enums.ProductStatus.Active && !p.IsDeleted)
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
                {
                    result.Attributes.Add(attrDto);
                }
            }

            return result;
        }
    }
}

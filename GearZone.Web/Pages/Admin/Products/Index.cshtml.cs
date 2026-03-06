using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Web.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        private readonly IAdminProductService _productService;
        private readonly IAdminCategoryService _categoryService;
        private readonly IAdminStoreService _storeService;
        private readonly IAdminBrandService _brandService;

        public IndexModel(IAdminProductService productService, IAdminCategoryService categoryService, IAdminStoreService storeService, IAdminBrandService brandService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _storeService = storeService;
            _brandService = brandService;
        }

        [BindProperty(SupportsGet = true)]
        public AdminProductQueryDto Query { get; set; } = new AdminProductQueryDto();

        public PagedResult<AdminProductDto> Products { get; set; } = new PagedResult<AdminProductDto>();
        public AdminProductStatsDto Stats { get; set; } = new AdminProductStatsDto();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stores { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Brands { get; set; } = new List<SelectListItem>();

        /// <summary>Attributes for the currently selected category, used to render dynamic filters.</summary>
        public List<CategoryAttributeDto> CategoryAttributes { get; set; } = new();

        public async Task OnGetAsync()
        {
            Stats = await _productService.GetProductStatsAsync();
            Products = await _productService.GetProductsAsync(Query);

            // Build hierarchical category list: roots first, then their children with "└ " prefix
            var categories = await _categoryService.GetAllCategoriesListAsync();
            var roots = categories.Where(c => c.ParentId == null).OrderBy(c => c.Name);
            var categoryItems = new List<SelectListItem>();
            foreach (var root in roots)
            {
                categoryItems.Add(new SelectListItem(root.Name, root.Id.ToString()));
                var children = categories
                    .Where(c => c.ParentId == root.Id)
                    .OrderBy(c => c.Name);
                foreach (var child in children)
                {
                    categoryItems.Add(new SelectListItem($"└ {child.Name}", child.Id.ToString()));
                }
            }
            Categories = categoryItems;

            var stores = await _storeService.GetAllStoresAsync();
            Stores = stores.Select(s => new SelectListItem(s.StoreName, s.Id.ToString())).ToList();

            var brands = await _brandService.GetAllBrandsListAsync();
            Brands = brands.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();

            // Load attributes for the selected category (if any)
            if (Query.CategoryId.HasValue && Query.CategoryId.Value > 0)
            {
                CategoryAttributes = await _categoryService.GetAttributesByCategoryIdAsync(Query.CategoryId.Value);
            }
        }

        /// <summary>AJAX endpoint: returns category attributes as JSON for dynamic filter rendering.</summary>
        public async Task<JsonResult> OnGetCategoryAttributesAsync(int categoryId)
        {
            if (categoryId <= 0)
                return new JsonResult(new List<object>());

            var attrs = await _categoryService.GetAttributesByCategoryIdAsync(categoryId);
            var result = attrs
                .Where(a => a.IsFilterable)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.FilterType,
                    Options = a.Options.Select(o => new { o.Id, o.Value })
                });
            return new JsonResult(result);
        }
    }
}


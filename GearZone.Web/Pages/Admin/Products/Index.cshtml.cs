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

        public IndexModel(IAdminProductService productService, IAdminCategoryService categoryService, IAdminStoreService storeService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _storeService = storeService;
        }

        [BindProperty(SupportsGet = true)]
        public AdminProductQueryDto Query { get; set; } = new AdminProductQueryDto();

        public PagedResult<AdminProductDto> Products { get; set; } = new PagedResult<AdminProductDto>();
        public AdminProductStatsDto Stats { get; set; } = new AdminProductStatsDto();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stores { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            Stats = await _productService.GetProductStatsAsync();
            Products = await _productService.GetProductsAsync(Query);

            var categories = await _categoryService.GetAllCategoriesListAsync();
            Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

            var stores = await _storeService.GetAllStoresAsync();
            Stores = stores.Select(s => new SelectListItem(s.StoreName, s.Id.ToString())).ToList();
        }
    }
}

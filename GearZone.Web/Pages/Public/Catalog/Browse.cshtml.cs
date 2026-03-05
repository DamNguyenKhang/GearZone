using GearZone.Application.Features.Catalog;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class BrowseModel : PageModel
    {
        private readonly ICatalogService _catalogService;

        public BrowseModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [BindProperty(SupportsGet = true)]
        public ProductFilterDto Filter { get; set; } = new ProductFilterDto();

        public CatalogFilterSidebarDto Sidebar { get; set; } = new CatalogFilterSidebarDto();

        public Application.Common.Models.PagedResult<CatalogProductDto> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {

            Sidebar = await _catalogService.GetFiltersForCategoryAsync(Filter.CategorySlug);
            Products = await _catalogService.GetProductsAsync(Filter);

            return Page();
        }

        public async Task<IActionResult> OnGetLoadMoreAsync()
        {
            Products = await _catalogService.GetProductsAsync(Filter);
            return new PartialViewResult
            {
                ViewName = "_ProductGridPartial",
                ViewData = new ViewDataDictionary<List<CatalogProductDto>>(ViewData, Products.Items) { ["ViewMode"] = Filter.ViewMode }
            };
        }
    }
}

using GearZone.Application.Features.Catalog;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

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
        public ProductFilterDto Filter { get; set; } = new ProductFilterDto { PageSize = 12 };

        public CatalogFilterSidebarDto Sidebar { get; set; } = new CatalogFilterSidebarDto();

        public Application.Common.Models.PagedResult<CatalogProductDto> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Filter.CategorySlug))
            {
                // Default fallback if needed
                Filter.CategorySlug = "graphics-cards";
            }

            Sidebar = await _catalogService.GetFiltersForCategoryAsync(Filter.CategorySlug);
            Products = await _catalogService.GetProductsAsync(Filter);

            return Page();
        }

        public async Task<IActionResult> OnGetLoadMoreAsync()
        {
            // For AJAX infinite scroll
            Products = await _catalogService.GetProductsAsync(Filter);
            return Partial("_ProductGridPartial", Products.Items);
        }
    }
}

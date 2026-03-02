using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace GearZone.Web.Pages
{
    public class ProductDetailModel : PageModel
    {
        private readonly ICatalogService _catalogService;

        public ProductDetailModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public ProductDetailDto Product { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return RedirectToPage("/Index");

            var product = await _catalogService.GetProductDetailBySlugAsync(slug);

            if (product == null)
                return NotFound();

            Product = product;
            return Page();
        }
    }
}

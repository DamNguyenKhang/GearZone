using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearZone.Application.Features.Catalog;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class ProductDetailModel : PageModel
    {
        private readonly ICatalogService _catalogService;

        public ProductDetailModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public ProductDetailDto Product { get; set; } = default!;
        public List<CatalogProductDto> RelatedProducts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToPage("/Index");
            }

            var product = await _catalogService.GetProductDetailBySlugAsync(slug);
            
            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            RelatedProducts = await _catalogService.GetRelatedProductsAsync(Product.CategoryId, Product.Id, 4);

            return Page();
        }
    }
}

using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Web.Pages
{
    public class CompareModel : PageModel
    {
        private readonly ICatalogService _catalogService;

        public CompareModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public List<ProductDetailViewModel> Products { get; set; } = new();
        public List<string> AllSpecKeys { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Page();

            var productIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Take(3)
                .ToList();

            if (!productIds.Any())
                return Page();

            Products = await _catalogService.GetProductsForCompareAsync(productIds);

            // Merge all spec keys across products
            AllSpecKeys = Products
                .SelectMany(p => p.Specs.Keys)
                .Distinct()
                .ToList();

            return Page();
        }
    }
}

using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class CreateModel : PageModel
    {
        private readonly ISellerProductService _productService;
        private readonly ISellerStoreService _storeService;

        public CreateModel(ISellerProductService productService, ISellerStoreService storeService)
        {
            _productService = productService;
            _storeService = storeService;
        }

        [BindProperty]
        public CreateProductDto Input { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> BrandOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadMetadataAsync();
            
            // Initialize with one default variant and one spec row
            Input.Variants.Add(new ProductVariantDto { VariantName = "Default", StockQuantity = 0 });


            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadMetadataAsync();
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var store = await _storeService.GetStoreByOwnerIdAsync(userId!);
            
            if (store == null) return RedirectToPage("/StoreOwner/Dashboard");

            try
            {
                await _productService.CreateProductAsync(Input, store.Id, userId!);
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadMetadataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                await LoadMetadataAsync();
                return Page();
            }
        }

        public async Task<JsonResult> OnGetAttributesAsync(int categoryId)
        {
            var attributes = await _productService.GetCategoryAttributesAsync(categoryId);
            return new JsonResult(attributes);
        }

        private async Task LoadMetadataAsync()
        {
            var allCategories = await _productService.GetCategoriesAsync();
            
            // Build hierarchy for display
            CategoryOptions = allCategories
                .Where(c => c.ParentId != null || !allCategories.Any(child => child.ParentId == c.Id)) // Only leaves or parents if they have no children (though we usually want leaves)
                .Select(c => {
                    var parent = c.ParentId.HasValue ? allCategories.FirstOrDefault(pc => pc.Id == c.ParentId.Value) : null;
                    var text = parent != null ? $"{parent.Name} > {c.Name}" : c.Name;
                    return new SelectListItem { Value = c.Id.ToString(), Text = text };
                })
                .OrderBy(s => s.Text)
                .ToList();

            var brands = await _productService.GetBrandsAsync();
            BrandOptions = brands.Select(b => new SelectListItem 
            { 
                Value = b.Id.ToString(), 
                Text = b.Name 
            }).ToList();
        }
    }
}

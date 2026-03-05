using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Threading.Tasks;

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
            Input.Specifications.Add(new ProductSpecDto { Key = "", Value = "" });

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
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred during creation: {ex.Message}");
                await LoadMetadataAsync();
                return Page();
            }
        }

        private async Task LoadMetadataAsync()
        {
            var categories = await _productService.GetCategoriesAsync();
            CategoryOptions = categories.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList();

            var brands = await _productService.GetBrandsAsync();
            BrandOptions = brands.Select(b => new SelectListItem 
            { 
                Value = b.Id.ToString(), 
                Text = b.Name 
            }).ToList();
        }
    }
}

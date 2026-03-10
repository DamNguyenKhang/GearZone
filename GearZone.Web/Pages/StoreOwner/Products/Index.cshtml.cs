using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly ISellerProductService _productService;
        private readonly ISellerStoreService _storeService;

        public IndexModel(ISellerProductService productService, ISellerStoreService storeService)
        {
            _productService = productService;
            _storeService = storeService;
        }

        public List<SellerProductListDto> Products { get; set; } = new();
        public Store Store { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Public/Auth/Login");

            Store = await _storeService.GetStoreByOwnerIdAsync(userId);
            if (Store == null) return RedirectToPage("/StoreOwner/Dashboard");

            Products = await _productService.GetProductsByStoreAsync(Store.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var store = await _storeService.GetStoreByOwnerIdAsync(userId!);
            
            if (store == null) return RedirectToPage("/StoreOwner/Dashboard");

            try
            {
                await _productService.ToggleProductStatusAsync(id, store.Id);
                TempData["SuccessMessage"] = "Product status updated!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}

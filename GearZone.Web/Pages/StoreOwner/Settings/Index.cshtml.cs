using System.Security.Claims;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Settings
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly ISellerStoreService _storeService;

        public Store? Store { get; set; }

        public IndexModel(ISellerStoreService storeService)
        {
            _storeService = storeService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            Store = await _storeService.GetStoreByOwnerIdAsync(userId);
            if (Store == null)
            {
                return RedirectToPage("/Public/Auth/Login"); // Or handle "no store found" properly
            }
            
            return Page();
        }
    }
}

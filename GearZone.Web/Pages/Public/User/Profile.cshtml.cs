using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Public.User
{
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ISellerStoreService _sellerStoreService;

        public ProfileModel(IAuthService authService, ISellerStoreService sellerStoreService)
        {
            _authService = authService;
            _sellerStoreService = sellerStoreService;
        }

        public UserDto? CurrentUser { get; set; }
        public string ActiveTab { get; set; } = "orders";
        public Store? UserStore { get; set; }

        public async Task<IActionResult> OnGetAsync(string? tab = "orders")
        {
            CurrentUser = await _authService.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }
            
            UserStore = await _sellerStoreService.GetStoreByOwnerIdAsync(CurrentUser.Id);
            
            ActiveTab = tab?.ToLower() ?? "orders";
            return Page();
        }
    }
}

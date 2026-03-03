using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Pages.Public.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.User
{
    public class RegisterSellerModel : PageModel
    {
        private readonly ISellerStoreService _sellerStoreService;
        private readonly IAuthService _authService;

        public RegisterSellerModel(ISellerStoreService sellerStoreService, IAuthService authService)
        {
            _sellerStoreService = sellerStoreService;
            _authService = authService;
        }

        [BindProperty]
        public StoreRegistrationViewModel Input { get; set; } = new StoreRegistrationViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var storeRegistrationDto = new StoreRegistrationDto
            {
                OwnerUserId = user.Id,
                StoreName = Input.StoreName,
                TaxCode = Input.TaxCode,
                Phone = Input.Phone,
                Email = Input.Email,
                AddressLine = Input.AddressLine,
                Province = Input.Province
            };

            await _sellerStoreService.ApplyForStoreAsync(storeRegistrationDto);

            TempData["SuccessMessage"] = "Your application to become a seller has been submitted and is pending approval.";
            return RedirectToPage("/Public/User/Profile");
        }
    }
}

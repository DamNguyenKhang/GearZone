using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            public string StoreName { get; set; } = string.Empty;
            public string TaxCode { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string AddressLine { get; set; } = string.Empty;
            public string Province { get; set; } = string.Empty;
        }

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

            var store = new Store
            {
                Id = Guid.NewGuid(),
                OwnerUserId = user.Id,
                StoreName = Input.StoreName,
                Slug = Input.StoreName.ToLower().Replace(" ", "-"), 
                TaxCode = Input.TaxCode,
                Phone = Input.Phone,
                Email = Input.Email,
                AddressLine = Input.AddressLine,
                Province = Input.Province,
                Status = StoreStatus.Pending,
                CommissionRate = 0.05m,
                CreatedAt = DateTime.UtcNow
            };

            await _sellerStoreService.ApplyForStoreAsync(store);

            TempData["SuccessMessage"] = "Your application to become a seller has been submitted and is pending approval.";
            return RedirectToPage("/Public/User/Profile");
        }
    }
}

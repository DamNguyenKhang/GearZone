using GearZone.Domain.Entities;
using GearZone.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.User
{
    [Authorize]
    public class RegisterSellerModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterSellerModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            var user = await _userManager.GetUserAsync(User);
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
                Status = "Pending",
                CommissionRate = 0.05m,
                CreatedAt = DateTime.UtcNow
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your application to become a seller has been submitted and is pending approval.";
            return RedirectToPage("/Public/User/Profile");
        }
    }
}

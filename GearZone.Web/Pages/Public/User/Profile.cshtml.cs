using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Public.User
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUser? CurrentUser { get; set; }
        public string ActiveTab { get; set; } = "orders";

        public async Task<IActionResult> OnGetAsync(string? tab = "orders")
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }
            ActiveTab = tab?.ToLower() ?? "orders";
            return Page();
        }
    }
}

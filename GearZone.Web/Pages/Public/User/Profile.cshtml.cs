using GearZone.Domain.Entities;
using GearZone.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GearZone.Web.Pages.Public.User
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ApplicationUser? CurrentUser { get; set; }
        public string ActiveTab { get; set; } = "orders";
        public Store? UserStore { get; set; }

        public async Task<IActionResult> OnGetAsync(string? tab = "orders")
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }
            
            UserStore = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerUserId == CurrentUser.Id);
            
            ActiveTab = tab?.ToLower() ?? "orders";
            return Page();
        }
    }
}

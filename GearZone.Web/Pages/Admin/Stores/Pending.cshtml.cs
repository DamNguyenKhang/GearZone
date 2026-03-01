using GearZone.Domain.Entities;
using GearZone.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Web.Pages.Admin.Stores
{
    [Authorize(Roles = "Super Admin")]
    public class PendingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PendingModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Store> PendingStores { get; set; } = default!;

        public async Task OnGetAsync()
        {
            PendingStores = await _context.Stores
                .Include(s => s.OwnerUser)
                .Where(s => s.Status == "Pending")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var store = await _context.Stores
                .Include(s => s.OwnerUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (store == null)
            {
                return NotFound();
            }

            store.Status = "Approved";
            store.ApprovedAt = DateTime.UtcNow;

            var user = store.OwnerUser;
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                if (!roles.Contains("Store Owner"))
                {
                    await _userManager.AddToRoleAsync(user, "Store Owner");
                }
                
                // Optionally remove from customer role
                if (roles.Contains("Customer"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Customer");
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Store {store.StoreName} has been approved. The owner is now a Store Owner.";
            return RedirectToPage("./Pending");
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, string reason)
        {
            var store = await _context.Stores.FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            store.Status = "Rejected";
            store.RejectReason = reason;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Store {store.StoreName} has been rejected.";
            return RedirectToPage("./Pending");
        }
    }
}

using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Stores
{
    [Authorize(Roles = "Super Admin")]
    public class PendingModel : PageModel
    {
        private readonly IAdminStoreService _adminStoreService;

        public PendingModel(IAdminStoreService adminStoreService)
        {
            _adminStoreService = adminStoreService;
        }

        public IEnumerable<Store> PendingStores { get; set; } = default!;

        public async Task OnGetAsync()
        {
            PendingStores = await _adminStoreService.GetPendingStoresAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var success = await _adminStoreService.ApproveStoreAsync(id);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"Store has been approved. The owner is now a Store Owner.";
            return RedirectToPage("./Pending");
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, string reason)
        {
            var success = await _adminStoreService.RejectStoreAsync(id, reason);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"Store has been rejected.";
            return RedirectToPage("./Pending");
        }
    }
}

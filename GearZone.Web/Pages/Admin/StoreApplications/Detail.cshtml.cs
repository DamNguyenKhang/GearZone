using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.StoreApplications
{
    public class DetailModel : PageModel
    {
        private readonly IAdminStoreService _adminStoreService;

        public DetailModel(IAdminStoreService adminStoreService)
        {
            _adminStoreService = adminStoreService;
        }

        public StoreApplicationDto? StoreApplication { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            StoreApplication = await _adminStoreService.GetStoreApplicationByIdAsync(id);

            if (StoreApplication == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var success = await _adminStoreService.ApproveStoreAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to approve store application.";
                return RedirectToPage(new { id });
            }
            TempData["SuccessMessage"] = "Store application has been successfully approved.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, string rejectReason)
        {
            var success = await _adminStoreService.RejectStoreAsync(id, rejectReason);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to reject store application.";
                return RedirectToPage(new { id });
            }
            TempData["SuccessMessage"] = "Store application has been rejected.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRequestInfoAsync(Guid id, string informNote)
        {
            var success = await _adminStoreService.RequestInfoAsync(id, informNote);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to send request info.";
                return RedirectToPage(new { id });
            }
            TempData["SuccessMessage"] = "Information request has been successfully sent to the seller.";
            return RedirectToPage(new { id });
        }
    }
}

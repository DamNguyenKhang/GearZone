using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections;

namespace GearZone.Web.Pages.Admin.StoreManagement
{
    public class IndexModel : PageModel
    {
        private readonly IAdminStoreService _adminStoreService;
        public StoreApplicationDto StoreApplication { get; set; } = new StoreApplicationDto();


        public IndexModel(IAdminStoreService adminStoreService)
        {
            _adminStoreService = adminStoreService;
        }

        [BindProperty(SupportsGet = true)]
        public StoreApplicationQueryDto Query { get; set; } = new StoreApplicationQueryDto();

        public PagedResult<StoreApplicationDto> StoreApplications { get; set; } = new() { Items = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 };
        public StoreApplicationStatsDto Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (Query.PageNumber < 1) Query.PageNumber = 1;
            if (Query.PageSize < 1) Query.PageSize = 10;

            StoreApplications = await _adminStoreService.GetStoreApplicationsAsync(Query);
            Stats = await _adminStoreService.GetStoreApplicationStatsAsync();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(Guid storeId, StoreStatus status, string reason = "")
        {
            var success = await _adminStoreService.UpdateStoreStatusAsync(storeId, status, reason);
           
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to change store application's status.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Store application's status  has been rejected.";
            return RedirectToPage();
        }
    }
}

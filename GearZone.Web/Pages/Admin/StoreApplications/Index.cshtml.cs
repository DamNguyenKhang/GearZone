using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.StoreApplications
{
    public class IndexModel : PageModel
    {
        private readonly IAdminStoreService _adminStoreService;

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
    }
}

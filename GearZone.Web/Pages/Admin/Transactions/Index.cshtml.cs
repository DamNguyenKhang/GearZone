using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using System.Threading.Tasks;
using GearZone.Application.Common.Models; // Keep this as PagedResult is used
using GearZone.Application.Features.Admin.Dtos; // Keep this as AdminPlatformTransactionSummaryDto and PlatformTransactionDto are used

namespace GearZone.Web.Pages.Admin.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly IAdminPlatformService _platformService;

        public IndexModel(IAdminPlatformService platformService)
        {
            _platformService = platformService;
        }

        [BindProperty(SupportsGet = true)]
        public PlatformTransactionQuery Query { get; set; } = new();

        public PagedResult<PlatformTransactionDto> Transactions { get; set; }
        public AdminPlatformTransactionSummaryDto Summary { get; set; }

        public async Task OnGetAsync()
        {
            Transactions = await _platformService.GetTransactionsAsync(Query);
            Summary = await _platformService.GetTransactionSummaryAsync(Query);
        }
    }
}

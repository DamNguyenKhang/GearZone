using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Admin.Wallet
{
    public class IndexModel : PageModel
    {
        private readonly IAdminWalletService _walletService;

        public IndexModel(IAdminWalletService walletService)
        {
            _walletService = walletService;
        }

        public WalletSummaryDto Summary { get; set; } = new();
        public PagedResult<WalletTransactionDto> Transactions { get; set; } = new();
        public List<WalletTransactionDto> BalanceHistory { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public WalletTransactionQuery Query { get; set; } = new();

        [BindProperty]
        public TopupWalletDto TopupInput { get; set; } = new();

        public async Task OnGetAsync()
        {
            Summary = await _walletService.GetWalletSummaryAsync();
            Transactions = await _walletService.GetTransactionsAsync(Query);
            BalanceHistory = await _walletService.GetBalanceHistoryAsync(days: 30);
        }

        public async Task<IActionResult> OnPostTopupAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload page data on validation failure
                Summary = await _walletService.GetWalletSummaryAsync();
                Transactions = await _walletService.GetTransactionsAsync(Query);
                BalanceHistory = await _walletService.GetBalanceHistoryAsync(days: 30);
                return Page();
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

            await _walletService.RecordTopupAsync(TopupInput, adminId);

            TempData["SuccessMessage"] = $"Topup đ{TopupInput.Amount:N0} recorded successfully with status Pending.";
            return RedirectToPage();
        }
    }
}

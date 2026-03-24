using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin
{
    [Authorize(Roles = "Super Admin")]
    public class SellerPayableSummaryModel : PageModel
    {
        private readonly IAdminPayoutService _adminPayoutService;
        private readonly IPayoutService _payoutService;
        private readonly IAdminPlatformService _platformService;

        public SellerPayableSummaryModel(
            IAdminPayoutService adminPayoutService,
            IPayoutService payoutService,
            IAdminPlatformService platformService)
        {
            _adminPayoutService = adminPayoutService;
            _payoutService = payoutService;
            _platformService = platformService;
        }

        public List<AdminSellerPayableSummaryDto> Payables { get; set; } = new();
        public decimal CurrentWalletBalance { get; private set; }
        public bool IsWalletBalanceAvailable { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string RangeType { get; set; } = "this-week";

        [BindProperty(SupportsGet = true)]
        public DateTime? CustomStart { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CustomEnd { get; set; }

        [BindProperty]
        public List<Guid> SelectedStoreIds { get; set; } = new();

        public DateTime CurrentStart { get; private set; }
        public DateTime CurrentEnd { get; private set; }

        public async Task OnGetAsync()
        {
            CalculateDates();
            Payables = await _adminPayoutService.GetSellerPayableSummaryAsync(CurrentStart, CurrentEnd);
            await LoadWalletBalanceAsync();
        }

        public async Task<IActionResult> OnPostProcessBulkAsync()
        {
            CalculateDates();

            var storeIds = SelectedStoreIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!storeIds.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one seller.";
                return RedirectToCurrentRange();
            }

            return await ProcessSelectedStoresAsync(storeIds);
        }

        public async Task<IActionResult> OnPostProcessSingleAsync(Guid storeId)
        {
            CalculateDates();

            if (storeId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid seller.";
                return RedirectToCurrentRange();
            }

            return await ProcessSelectedStoresAsync([storeId]);
        }

        private async Task<IActionResult> ProcessSelectedStoresAsync(IReadOnlyCollection<Guid> storeIds)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            try
            {
                var payables = await _adminPayoutService.GetSellerPayableSummaryAsync(CurrentStart, CurrentEnd);
                var requiredAmount = payables
                    .Where(x => storeIds.Contains(x.StoreId))
                    .Sum(x => x.TotalNetAmount);

                var walletBalance = await GetCurrentWalletBalanceAsync();
                if (walletBalance < requiredAmount)
                {
                    TempData["ErrorMessage"] = $"Số dư ví hiện tại ({walletBalance:N0}đ) không đủ để payout ({requiredAmount:N0}đ). Vui lòng topup trước khi xử lý.";
                    return RedirectToPage("/Admin/Wallet/Index");
                }

                var batchCode = await _payoutService.GenerateApprovedBatchForStoresAsync(
                    CurrentStart,
                    CurrentEnd,
                    storeIds,
                    adminId);

                await _payoutService.ProcessPayoutBatchAsync(batchCode, HttpContext.RequestAborted);

                var paged = await _adminPayoutService.GetPayoutBatchesAsync(new AdminPayoutBatchQueryDto
                {
                    SearchTerm = batchCode,
                    PageNumber = 1,
                    PageSize = 5
                });

                var processedBatch = paged.Items.FirstOrDefault(x => x.BatchCode == batchCode);
                if (processedBatch == null)
                {
                    TempData["InfoMessage"] = $"Batch '{batchCode}' đã được xử lý. Hãy kiểm tra danh sách payout batches để xem trạng thái.";
                }
                else if (processedBatch.Status == GearZone.Domain.Enums.PayoutBatchStatus.Completed)
                {
                    TempData["SuccessMessage"] = $"Payout thành công. Batch '{batchCode}' hoàn tất ({processedBatch.SuccessCount} giao dịch).";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Payout thất bại một phần/toàn phần. Batch '{batchCode}' có {processedBatch.FailedCount} giao dịch lỗi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to process payout: {ex.Message}";
            }

            return RedirectToCurrentRange();
        }

        private RedirectToPageResult RedirectToCurrentRange()
        {
            return RedirectToPage(new
            {
                RangeType,
                CustomStart = RangeType == "custom" ? CustomStart : null,
                CustomEnd = RangeType == "custom" ? CustomEnd : null
            });
        }

        private void CalculateDates()
        {
            var now = DateTime.UtcNow;

            switch (RangeType?.Trim().ToLowerInvariant())
            {
                case "last-week":
                {
                    var currentWeekStart = StartOfWeek(now.Date, DayOfWeek.Monday);
                    CurrentStart = currentWeekStart.AddDays(-7);
                    CurrentEnd = currentWeekStart.AddTicks(-1);
                    break;
                }
                case "custom":
                {
                    CurrentStart = (CustomStart ?? now.AddDays(-7)).Date;
                    CurrentEnd = (CustomEnd ?? now).Date.AddDays(1).AddTicks(-1);
                    break;
                }
                case "this-week":
                default:
                {
                    CurrentStart = StartOfWeek(now.Date, DayOfWeek.Monday);
                    CurrentEnd = now;
                    break;
                }
            }
        }

        private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-diff).Date;
        }

        private async Task LoadWalletBalanceAsync()
        {
            try
            {
                CurrentWalletBalance = await GetCurrentWalletBalanceAsync();
                IsWalletBalanceAvailable = true;
            }
            catch
            {
                CurrentWalletBalance = 0m;
                IsWalletBalanceAvailable = false;
            }
        }

        private async Task<decimal> GetCurrentWalletBalanceAsync()
        {
            var summary = await _platformService.GetTransactionSummaryAsync(new PlatformTransactionQuery
            {
                PageNumber = 1,
                PageSize = 1
            });
            return summary.WalletBalance;
        }
    }
}

using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Orders
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public PagedResult<SellerChatOrderListItemDto> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            ViewData["Title"] = "Orders";
            ViewData["PageHeader"] = "Orders";
            ViewData["ActivePage"] = "Orders";
            ViewData["Breadcrumb"] = new[] { "Orders" };

            Orders = await _chatService.GetSellerChatOrdersAsync(userId, new SellerChatOrderQueryDto
            {
                SearchTerm = SearchTerm,
                PageNumber = PageNumber,
                PageSize = 10
            });

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var ok = await _chatService.ApproveSellerOrderAsync(userId, subOrderId);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order approved successfully."
                : "Cannot approve this order. Only pending orders can be approved.";

            return RedirectToPage(new { SearchTerm, PageNumber });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var ok = await _chatService.RejectSellerOrderAsync(userId, subOrderId);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order rejected successfully."
                : "Cannot reject this order. Only pending orders can be rejected.";

            return RedirectToPage(new { SearchTerm, PageNumber });
        }
    }
}

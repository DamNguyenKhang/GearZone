using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Orders
{
    [Authorize(Roles = "Store Owner")]
    public class DetailModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public SellerChatOrderDetailDto OrderDetail { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            if (subOrderId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage("/StoreOwner/Orders/Index");
            }

            var detail = await _chatService.GetSellerChatOrderDetailAsync(userId, subOrderId);
            if (detail == null)
            {
                TempData["ErrorMessage"] = "Order not found or you do not have permission.";
                return RedirectToPage("/StoreOwner/Orders/Index");
            }

            OrderDetail = detail;

            ViewData["Title"] = $"Order #{detail.OrderCode}";
            ViewData["PageHeader"] = "Order Detail";
            ViewData["ActivePage"] = "Orders";
            ViewData["Breadcrumb"] = new[] { "Orders", $"#{detail.OrderCode}" };

            return Page();
        }
    }
}

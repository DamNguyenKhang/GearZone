using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Orders.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.User.Orders
{
    [Authorize]
    public class TrackModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public TrackModel(IOrderService orderService, IAuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        public UserOrderTrackingDto Tracking { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid subOrderId)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            if (subOrderId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders" });
            }

            var tracking = await _orderService.GetUserOrderTrackingAsync(user.Id, subOrderId);
            if (tracking == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders" });
            }

            Tracking = tracking;
            return Page();
        }
    }
}

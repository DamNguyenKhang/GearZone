using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class PayOSCheckoutModel : PageModel
    {
        private readonly IOrderService _orderService;

        public PayOSCheckoutModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public string CheckoutUrl { get; set; } = string.Empty;
        public Order CurrentOrder { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid orderId, string checkoutUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            if (string.IsNullOrEmpty(checkoutUrl) || orderId == Guid.Empty)
            {
                return RedirectToPage("/Cart/Index");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                return RedirectToPage("/Cart/Index");
            }

            CurrentOrder = order;
            CheckoutUrl = checkoutUrl;

            return Page();
        }
    }
}

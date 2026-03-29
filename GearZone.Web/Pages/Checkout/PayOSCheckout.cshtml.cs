using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class PayOSCheckoutModel : PageModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string Bin { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        
        // Expiry time = now + 10 mins (passed to JS)
        public long ExpireAtUnix { get; set; }

        public IActionResult OnGet()
        {
            // Read from TempData (set by Checkout/Index when PayOS link is created)
            OrderId       = TempData["PayOS_OrderId"]?.ToString() ?? string.Empty;
            OrderCode     = TempData["PayOS_OrderCode"]?.ToString() ?? string.Empty;
            Bin           = TempData["PayOS_Bin"]?.ToString() ?? string.Empty;
            AccountNumber = TempData["PayOS_AccountNumber"]?.ToString() ?? string.Empty;
            AccountName   = TempData["PayOS_AccountName"]?.ToString() ?? string.Empty;
            Amount        = TempData["PayOS_Amount"]?.ToString() ?? string.Empty;
            Description   = TempData["PayOS_Description"]?.ToString() ?? string.Empty;
            QrCode        = TempData["PayOS_QrCode"]?.ToString() ?? string.Empty;
            CheckoutUrl   = TempData["PayOS_CheckoutUrl"]?.ToString() ?? string.Empty;

            // If there is no QR data, go back to cart
            if (string.IsNullOrEmpty(QrCode) && string.IsNullOrEmpty(OrderId))
                return RedirectToPage("/Cart/Index");

            // Set expiry 10 minutes from now
            ExpireAtUnix = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeMilliseconds();

            return Page();
        }
    }
}

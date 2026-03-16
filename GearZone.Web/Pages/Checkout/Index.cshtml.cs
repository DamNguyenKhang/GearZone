using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.User.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public IndexModel(
            ICheckoutService checkoutService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUserService userService)
        {
            _checkoutService = checkoutService;
            _orderService = orderService;
            _userManager = userManager;
            _configuration = configuration;
            _userService = userService;
        }

        public string? GoongApiKey => _configuration["GOONG_API_KEY"];

        [BindProperty(SupportsGet = true)]
        public List<Guid> SelectedCartItemIds { get; set; } = new();

        [BindProperty]
        public CheckoutRequestDto CheckoutRequest { get; set; } = new();

        public ApplicationUser CurrentUser { get; set; } = null!;
        public List<CartItem> SelectedItems { get; set; } = new();
        public List<UserAddressDto> UserAddresses { get; set; } = new();
        public decimal GrandTotal { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            CurrentUser = await _userManager.FindByIdAsync(userId);

            if (SelectedCartItemIds == null || !SelectedCartItemIds.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            // Load selected cart items to display via Service
            SelectedItems = await _checkoutService.GetCheckoutItemsAsync(userId, SelectedCartItemIds);

            if (!SelectedItems.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            GrandTotal = SelectedItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            // Load user addresses
            UserAddresses = (await _userService.GetUserAddressesAsync(userId)).ToList();
            var defaultAddress = UserAddresses.FirstOrDefault(a => a.IsDefault) ?? UserAddresses.FirstOrDefault();

            // Pre-fill shipping info
            CheckoutRequest.ShippingInfo = new ShippingInfoDto
            {
                FullName = defaultAddress?.FullName ?? CurrentUser.FullName ?? string.Empty,
                PhoneNumber = defaultAddress?.PhoneNumber ?? CurrentUser.PhoneNumber ?? string.Empty,
                EmailAddress = CurrentUser.Email ?? string.Empty,
                Address = defaultAddress?.AddressLine ?? string.Empty
            };
            CheckoutRequest.CartItemIds = SelectedCartItemIds;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            // We MUST capture the items before they potentially get cleared from the cart
            SelectedItems = await _checkoutService.GetCheckoutItemsAsync(userId, CheckoutRequest.CartItemIds);
            GrandTotal = SelectedItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            if (!ModelState.IsValid)
            {
                CurrentUser = await _userManager.FindByIdAsync(userId);
                UserAddresses = (await _userService.GetUserAddressesAsync(userId)).ToList();
                SelectedCartItemIds = CheckoutRequest.CartItemIds;
                return Page();
            }

            // Use the user's selected payment method
            var result = await _checkoutService.ProcessCheckoutAsync(userId, CheckoutRequest);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Checkout failed.");
                CurrentUser = await _userManager.FindByIdAsync(userId);
                UserAddresses = (await _userService.GetUserAddressesAsync(userId)).ToList();
                return Page();
            }

            // If PayOS: Redirect user directly to the PayOS payment page
            if (!string.IsNullOrEmpty(result.CheckoutUrl))
            {
                return Redirect(result.CheckoutUrl);
            }

            // If COD: redirect to success page
            return RedirectToPage("./Success", new { orderId = result.OrderId });
        }
    }
}

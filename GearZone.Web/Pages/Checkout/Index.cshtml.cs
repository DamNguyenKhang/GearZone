using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly ICheckoutService _checkoutService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public IndexModel(
            ICartItemRepository cartItemRepository,
            ICheckoutService checkoutService,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _cartItemRepository = cartItemRepository;
            _checkoutService = checkoutService;
            _userManager = userManager;
            _configuration = configuration;
        }

        public string? GoongApiKey => _configuration["GOONG_API_KEY"];

        [BindProperty(SupportsGet = true)]
        public List<Guid> SelectedCartItemIds { get; set; } = new();

        [BindProperty]
        public CheckoutRequestDto CheckoutRequest { get; set; } = new();

        public ApplicationUser CurrentUser { get; set; } = null!;
        public List<CartItem> SelectedItems { get; set; } = new();
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

            // Load selected cart items to display
            SelectedItems = await GetSelectedItemsWithDetails(userId, SelectedCartItemIds);

            if (!SelectedItems.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            GrandTotal = SelectedItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            // Pre-fill shipping info from user profile
            CheckoutRequest.ShippingInfo = new ShippingInfoDto
            {
                FullName = CurrentUser.FullName,
                PhoneNumber = CurrentUser.PhoneNumber,
                EmailAddress = CurrentUser.Email ?? string.Empty,
                StreetAddress = CurrentUser.Address // In a real app we might parse this, but for now just map it
            };
            CheckoutRequest.CartItemIds = SelectedCartItemIds;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            if (!ModelState.IsValid)
            {
                // Reload items on validation fail
                CurrentUser = await _userManager.FindByIdAsync(userId);
                SelectedItems = await GetSelectedItemsWithDetails(userId, CheckoutRequest.CartItemIds);
                GrandTotal = SelectedItems.Sum(ci => ci.Quantity * ci.Variant.Price);
                SelectedCartItemIds = CheckoutRequest.CartItemIds;
                return Page();
            }

            // Since Payment Integration is skipped, we default to COD
            CheckoutRequest.PaymentMethod = PaymentMethod.COD;

            var result = await _checkoutService.ProcessCheckoutAsync(userId, CheckoutRequest);
            
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Checkout failed.");
                CurrentUser = await _userManager.FindByIdAsync(userId);
                SelectedItems = await GetSelectedItemsWithDetails(userId, CheckoutRequest.CartItemIds);
                GrandTotal = SelectedItems.Sum(ci => ci.Quantity * ci.Variant.Price);
                return Page();
            }

            // Redirect to success page
            return RedirectToPage("./Success", new { orderId = result.OrderId });
        }

        private async Task<List<CartItem>> GetSelectedItemsWithDetails(string userId, List<Guid> cartItemIds)
        {
            return await _cartItemRepository.Query()
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Store)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Images)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Where(ci => cartItemIds.Contains(ci.Id) && ci.Cart.UserId == userId)
                .ToListAsync();
        }
    }
}

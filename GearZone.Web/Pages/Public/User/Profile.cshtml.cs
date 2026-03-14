using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Orders.Dtos;
using GearZone.Application.Features.Reviews.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Public.User
{
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ISellerStoreService _sellerStoreService;
        private readonly IOrderService _orderService;
        private readonly IProductReviewService _productReviewService;

        public ProfileModel(
            IAuthService authService,
            ISellerStoreService sellerStoreService,
            IOrderService orderService,
            IProductReviewService productReviewService)
        {
            _authService = authService;
            _sellerStoreService = sellerStoreService;
            _orderService = orderService;
            _productReviewService = productReviewService;
        }

        public UserDto? CurrentUser { get; set; }
        public string ActiveTab { get; set; } = "orders";
        public Store? UserStore { get; set; }
        public PagedResult<UserOrderDto> Orders { get; set; } = new();
        public UserOrderStatusSummaryDto OrderStatusSummary { get; set; } = new();
        public PagedResult<MyReviewDto> Reviews { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string OrderStatus { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int OrderPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int ReviewPage { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(string? tab = "orders")
        {
            CurrentUser = await _authService.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }
            
            UserStore = await _sellerStoreService.GetStoreByOwnerIdAsync(CurrentUser.Id);
            
            ActiveTab = tab?.ToLower() ?? "orders";

            if (ActiveTab == "orders")
            {
                OrderStatusSummary = await _orderService.GetUserOrderStatusSummaryAsync(CurrentUser.Id);
                Orders = await _orderService.GetUserOrdersAsync(CurrentUser.Id, new UserOrderQueryDto
                {
                    Status = OrderStatus,
                    SearchTerm = SearchTerm,
                    PageNumber = OrderPage,
                    PageSize = 5
                });
            }
            else if (ActiveTab == "reviews")
            {
                OrderStatusSummary = await _orderService.GetUserOrderStatusSummaryAsync(CurrentUser.Id);
                Reviews = await _productReviewService.GetMyReviewsAsync(CurrentUser.Id, ReviewPage, 6);
            }

            return Page();
        }
    }
}

using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Orders.Dtos;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Pages.Public.User.Messages;
using GearZone.Web.Pages.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;

namespace GearZone.Web.Pages.Public.User
{
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ISellerStoreService _sellerStoreService;
        private readonly IOrderService _orderService;
        private readonly IProductReviewService _productReviewService;
        private readonly BuyerInboxComposer _buyerInboxComposer;

        public ProfileModel(
            IAuthService authService,
            ISellerStoreService sellerStoreService,
            IOrderService orderService,
            IProductReviewService productReviewService,
            BuyerInboxComposer buyerInboxComposer)
        {
            _authService = authService;
            _sellerStoreService = sellerStoreService;
            _orderService = orderService;
            _productReviewService = productReviewService;
            _buyerInboxComposer = buyerInboxComposer;
        }

        public UserDto? CurrentUser { get; set; }
        public string ActiveTab { get; set; } = "orders";
        public Store? UserStore { get; set; }
        public PagedResult<UserOrderDto> Orders { get; set; } = new();
        public UserOrderStatusSummaryDto OrderStatusSummary { get; set; } = new();
        public PagedResult<MyReviewDto> Reviews { get; set; } = new();
        public ChatInboxPageViewModel MessagesInbox { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string OrderStatus { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int OrderPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int ReviewPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public Guid? ConversationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string? CounterpartScopeKey { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StoreSlug { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ProductSlug { get; set; }

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
            else if (ActiveTab == "messages")
            {
                if (!string.IsNullOrWhiteSpace(StoreSlug) && !ConversationId.HasValue)
                {
                    var ensuredConversationId = await _buyerInboxComposer.EnsureConversationAsync(CurrentUser.Id, StoreSlug);
                    if (!ensuredConversationId.HasValue)
                    {
                        TempData["ErrorMessage"] = "This shop cannot be opened in chat right now.";
                        return RedirectToPage("/Public/User/Profile", new { tab = "messages" });
                    }

                    return RedirectToPage("/Public/User/Profile", new
                    {
                        tab = "messages",
                        conversationId = ensuredConversationId,
                        productSlug = ProductSlug
                    });
                }

                MessagesInbox = await _buyerInboxComposer.BuildInboxAsync(CurrentUser.Id, new BuyerInboxBuildRequest
                {
                    BasePath = GetMessagesBasePath(),
                    Filter = Filter,
                    SearchTerm = SearchTerm,
                    CounterpartScopeKey = CounterpartScopeKey,
                    ProductSlug = ProductSlug,
                    SelectedConversationId = ConversationId,
                    IncludeThread = true,
                    IsAccountCenterSurface = true
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnGetConversationListAsync(Guid? conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var inbox = await _buyerInboxComposer.BuildInboxAsync(userId, new BuyerInboxBuildRequest
            {
                BasePath = GetMessagesBasePath(),
                Filter = Filter,
                SearchTerm = SearchTerm,
                CounterpartScopeKey = CounterpartScopeKey,
                ProductSlug = ProductSlug,
                SelectedConversationId = conversationId,
                IncludeThread = false,
                IsAccountCenterSurface = true
            });

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatConversationList.cshtml",
                ViewData = new ViewDataDictionary<ChatConversationListViewModel>(ViewData, _buyerInboxComposer.BuildConversationListViewModel(inbox))
            };
        }

        public async Task<IActionResult> OnGetThreadAsync(Guid conversationId, int loadedPageCount = 1, string? productSlug = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var thread = await _buyerInboxComposer.GetThreadAsync(
                userId,
                conversationId,
                loadedPageCount,
                productSlug ?? ProductSlug);

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatThreadPane.cshtml",
                ViewData = new ViewDataDictionary<ChatThreadPaneViewModel>(ViewData, _buyerInboxComposer.BuildThreadPaneViewModel(userId, thread, false, true))
            };
        }

        private string GetMessagesBasePath()
        {
            return Url.Page("/Public/User/Profile", new { tab = "messages" }) ?? "/Public/User/Profile?tab=messages";
        }
    }
}

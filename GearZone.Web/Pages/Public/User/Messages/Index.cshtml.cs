using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Web.Pages.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GearZone.Web.Pages.Public.User.Messages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public ChatInboxPageViewModel Inbox { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid? ConversationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CounterpartScopeKey { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StoreSlug { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ProductSlug { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            if (!string.IsNullOrWhiteSpace(StoreSlug) && !ConversationId.HasValue)
            {
                var conversationId = await _chatService.EnsureBuyerConversationAsync(userId, StoreSlug);
                if (!conversationId.HasValue)
                {
                    TempData["ErrorMessage"] = "This shop cannot be opened in chat right now.";
                    return RedirectToPage("/Public/User/Messages/Index");
                }

                return RedirectToPage(new { conversationId, productSlug = ProductSlug });
            }

            Inbox = await BuildInboxAsync(userId, ConversationId, 1);
            return Page();
        }

        public async Task<IActionResult> OnGetWidgetBootstrapAsync(Guid? conversationId, string? storeSlug, string? productSlug, string? counterpartScopeKey = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var widget = await _chatService.GetBuyerWidgetBootstrapAsync(userId, new ChatWidgetBootstrapQueryDto
            {
                ConversationId = conversationId,
                StoreSlug = storeSlug,
                ProductSlug = productSlug,
                Filter = Filter,
                SearchTerm = SearchTerm,
                CounterpartScopeKey = counterpartScopeKey ?? CounterpartScopeKey,
                LoadedPageCount = 1,
                InboxPageSize = 20,
                MessagePageSize = 30
            });

            if (widget.ActiveThread != null)
            {
                await _chatService.MarkConversationReadAsync(userId, widget.ActiveThread.ConversationId);
            }

            Response.Headers["X-Requested-Target-Unavailable"] = widget.RequestedTargetUnavailable ? "true" : "false";

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatInboxLayout.cshtml",
                ViewData = new ViewDataDictionary<ChatInboxPageViewModel>(ViewData, MapInbox(widget, userId, isWidgetSurface: true))
            };
        }

        public async Task<IActionResult> OnGetConversationListAsync(Guid? conversationId, string surface = "page")
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var isWidgetSurface = string.Equals(surface, "widget", StringComparison.OrdinalIgnoreCase);
            var inbox = await BuildInboxAsync(userId, conversationId, 1, includeThread: false, isWidgetSurface: isWidgetSurface);
            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatConversationList.cshtml",
                ViewData = new ViewDataDictionary<ChatConversationListViewModel>(ViewData, new ChatConversationListViewModel
                {
                    IsSellerView = false,
                    IsWidgetSurface = inbox.IsWidgetSurface,
                    CurrentUserId = userId,
                    BasePath = "/messages",
                    Filter = inbox.Filter,
                    SearchTerm = inbox.SearchTerm,
                    CounterpartScopeKey = inbox.CounterpartScopeKey,
                    ActiveConversationId = inbox.ActiveConversationId,
                    TotalUnreadCount = inbox.TotalUnreadCount,
                    EmptyInboxTitle = inbox.EmptyInboxTitle,
                    EmptyInboxDescription = inbox.EmptyInboxDescription,
                    CounterpartScopeOptions = inbox.CounterpartScopeOptions,
                    Conversations = inbox.Conversations
                })
            };
        }

        public async Task<IActionResult> OnGetThreadAsync(Guid conversationId, int loadedPageCount = 1, string surface = "page", string? productSlug = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var isWidgetSurface = string.Equals(surface, "widget", StringComparison.OrdinalIgnoreCase);
            var thread = await _chatService.GetBuyerThreadAsync(userId, conversationId, new ChatThreadQueryDto
            {
                LoadedPageCount = loadedPageCount,
                PageSize = 30,
                ProductSlug = productSlug
            });

            if (thread != null)
            {
                await _chatService.MarkConversationReadAsync(userId, conversationId);
            }

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatThreadPane.cshtml",
                ViewData = new ViewDataDictionary<ChatThreadPaneViewModel>(ViewData, new ChatThreadPaneViewModel
                {
                    IsSellerView = false,
                    IsWidgetSurface = isWidgetSurface,
                    CurrentUserId = userId,
                    EmptyTitle = "Chon mot cuoc tro chuyen",
                    EmptyDescription = "Chon shop o cot ben trai de xem toan bo tin nhan.",
                    Thread = thread
                })
            };
        }

        private async Task<ChatInboxPageViewModel> BuildInboxAsync(
            string userId,
            Guid? selectedConversationId,
            int loadedPageCount,
            bool includeThread = true,
            bool isWidgetSurface = false)
        {
            var query = new ChatInboxQueryDto
            {
                Filter = Filter,
                SearchTerm = SearchTerm,
                CounterpartScopeKey = CounterpartScopeKey,
                PageNumber = 1,
                PageSize = 20
            };

            var conversations = await _chatService.GetBuyerInboxAsync(userId, query);
            var counterpartScopeOptions = await _chatService.GetBuyerCounterpartScopeOptionsAsync(userId);
            var activeConversationId = selectedConversationId;
            if (!activeConversationId.HasValue && conversations.Items.Any())
            {
                activeConversationId = conversations.Items[0].ConversationId;
            }

            ChatThreadDto? thread = null;
            if (includeThread && activeConversationId.HasValue)
            {
                thread = await _chatService.GetBuyerThreadAsync(userId, activeConversationId.Value, new ChatThreadQueryDto
                {
                    LoadedPageCount = loadedPageCount,
                    PageSize = 30,
                    ProductSlug = ProductSlug
                });

                if (thread == null && conversations.Items.Any())
                {
                    activeConversationId = conversations.Items[0].ConversationId;
                    thread = await _chatService.GetBuyerThreadAsync(userId, activeConversationId.Value, new ChatThreadQueryDto
                    {
                        LoadedPageCount = 1,
                        PageSize = 30,
                        ProductSlug = ProductSlug
                    });
                }

                if (thread != null)
                {
                    await _chatService.MarkConversationReadAsync(userId, thread.ConversationId);
                }
            }

            return new ChatInboxPageViewModel
            {
                IsSellerView = false,
                IsWidgetSurface = isWidgetSurface,
                CurrentUserId = userId,
                BasePath = "/messages",
                Filter = query.Filter,
                SearchTerm = query.SearchTerm,
                CounterpartScopeKey = query.CounterpartScopeKey,
                ProductSlug = ProductSlug,
                ActiveConversationId = activeConversationId,
                TotalUnreadCount = await _chatService.GetBuyerUnreadCountAsync(userId),
                EmptyInboxTitle = "Chua co cuoc tro chuyen nao",
                EmptyInboxDescription = "Mo bat ky shop hop le nao va bam Chat de bat dau tro chuyen.",
                CounterpartScopeOptions = counterpartScopeOptions,
                Conversations = conversations,
                ActiveThread = thread
            };
        }

        private static ChatInboxPageViewModel MapInbox(
            ChatWidgetBootstrapDto widget,
            string userId,
            bool isWidgetSurface)
        {
            return new ChatInboxPageViewModel
            {
                IsSellerView = false,
                IsWidgetSurface = isWidgetSurface,
                CurrentUserId = userId,
                BasePath = "/messages",
                Filter = widget.Filter,
                SearchTerm = widget.SearchTerm,
                CounterpartScopeKey = widget.CounterpartScopeKey,
                ProductSlug = widget.ActiveThread?.ActiveProductContext?.ProductSlug,
                ActiveConversationId = widget.ActiveConversationId,
                TotalUnreadCount = widget.TotalUnreadCount,
                EmptyInboxTitle = "Chua co cuoc tro chuyen nao",
                EmptyInboxDescription = "Mo bat ky shop hop le nao va bam Chat de bat dau tro chuyen.",
                CounterpartScopeOptions = widget.CounterpartScopeOptions,
                Conversations = widget.Conversations,
                ActiveThread = widget.ActiveThread
            };
        }
    }
}

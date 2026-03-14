using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.StoreOwner
{
    [Authorize(Roles = "Store Owner")]
    public class DashboardModel : PageModel
    {
        private readonly IChatService _chatService;

        public DashboardModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        public int CustomerConversationCount { get; set; }
        public int CustomerUnreadCount { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var inbox = await _chatService.GetSellerInboxAsync(userId, new ChatInboxQueryDto
            {
                PageNumber = 1,
                PageSize = 1
            });

            CustomerConversationCount = inbox.TotalCount;
            CustomerUnreadCount = await _chatService.GetSellerUnreadCountAsync(userId);
        }
    }
}

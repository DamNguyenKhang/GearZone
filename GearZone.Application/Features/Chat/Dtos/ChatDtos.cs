using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Chat.Dtos
{
    public class ChatInboxQueryDto
    {
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ChatThreadQueryDto
    {
        public int LoadedPageCount { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public string? ProductSlug { get; set; }
    }

    public class ChatWidgetBootstrapQueryDto
    {
        public Guid? ConversationId { get; set; }
        public string? StoreSlug { get; set; }
        public string? ProductSlug { get; set; }
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public int LoadedPageCount { get; set; } = 1;
        public int InboxPageSize { get; set; } = 20;
        public int MessagePageSize { get; set; } = 30;
    }

    public class SellerChatOrderQueryDto
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SendChatMessageDto
    {
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class ChatConversationListItemDto
    {
        public Guid ConversationId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public string? StoreLogoUrl { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public string CounterpartName { get; set; } = string.Empty;
        public string? CounterpartAvatarUrl { get; set; }
        public string CounterpartSubtitle { get; set; } = string.Empty;
        public string LastMessagePreview { get; set; } = string.Empty;
        public string LastMessageSenderUserId { get; set; } = string.Empty;
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public bool HasMessages { get; set; }
    }

    public class ChatCounterpartScopeOptionDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class ChatMessageItemDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string SenderUserId { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public string? SenderAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class ChatContextOrderDto
    {
        public Guid SubOrderId { get; set; }
        public long OrderCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public int ItemCount { get; set; }
        public string ProductPreview { get; set; } = string.Empty;
    }

    public class ChatProductContextDto
    {
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string? StoreLogoUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsInStock { get; set; }
    }

    public class SellerChatOrderListItemDto
    {
        public Guid SubOrderId { get; set; }
        public long OrderCode { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string BuyerUserId { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public int ItemCount { get; set; }
        public string ProductPreview { get; set; } = string.Empty;
    }

    public class ChatThreadDto
    {
        public Guid ConversationId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public string? StoreLogoUrl { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public string CounterpartName { get; set; } = string.Empty;
        public string? CounterpartAvatarUrl { get; set; }
        public bool IsSellerView { get; set; }
        public int LoadedPageCount { get; set; }
        public int PageSize { get; set; }
        public bool HasOlderMessages { get; set; }
        public List<ChatMessageItemDto> Messages { get; set; } = new();
        public List<ChatContextOrderDto> RecentOrders { get; set; } = new();
        public ChatProductContextDto? ActiveProductContext { get; set; }
    }

    public class ChatSendMessageResultDto
    {
        public Guid ConversationId { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string StoreOwnerUserId { get; set; } = string.Empty;
        public ChatMessageItemDto Message { get; set; } = new();
    }

    public class ChatConversationUpdateDto
    {
        public bool IsSellerView { get; set; }
        public int TotalUnreadCount { get; set; }
        public ChatConversationListItemDto Conversation { get; set; } = new();
    }

    public class ChatBootstrapResultDto
    {
        public Guid ConversationId { get; set; }
        public Guid StoreId { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string StoreOwnerUserId { get; set; } = string.Empty;
    }

    public class ChatWidgetBootstrapDto
    {
        public Guid? ActiveConversationId { get; set; }
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public int TotalUnreadCount { get; set; }
        public bool RequestedTargetUnavailable { get; set; }
        public List<ChatCounterpartScopeOptionDto> CounterpartScopeOptions { get; set; } = new();
        public PagedResult<ChatConversationListItemDto> Conversations { get; set; } = new PagedResult<ChatConversationListItemDto>(new List<ChatConversationListItemDto>(), 0, 1, 20);
        public ChatThreadDto? ActiveThread { get; set; }
    }
}

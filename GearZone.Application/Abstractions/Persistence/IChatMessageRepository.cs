using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IChatMessageRepository : IRepository<ChatMessage, Guid>
    {
        Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId, int pageNumber, int pageSize);
    }
}

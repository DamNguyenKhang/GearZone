using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IConversationRepository : IRepository<Conversation, Guid>
    {
        Task<Conversation?> GetByBuyerAndStoreAsync(string buyerUserId, Guid storeId);
    }
}

using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class ConversationRepository : Repository<Conversation, Guid>, IConversationRepository
    {
        public ConversationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Conversation?> GetByBuyerAndStoreAsync(string buyerUserId, Guid storeId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.BuyerUserId == buyerUserId && c.StoreId == storeId);
        }
    }
}

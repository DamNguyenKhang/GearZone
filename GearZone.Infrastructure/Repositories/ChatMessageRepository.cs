using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class ChatMessageRepository : Repository<ChatMessage, Guid>, IChatMessageRepository
    {
        public ChatMessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(m => m.SenderUser)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SubOrder>> GetOrdersNotTransfer()
        {
            var orders = await _context.Set<SubOrder>()
            .Where(o =>
                o.Status == OrderStatus.Delivered &&
                o.PayoutStatus == PayoutStatus.Unpaid &&
                o.UpdatedAt <= DateTime.UtcNow.AddDays(-7)
            )
            .ToListAsync();
            return orders;
        }

        public async Task<List<SubOrder>> GetEligibleForPayoutAsync(
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken ct = default)
        {
            return await _context.Set<SubOrder>()
                .Include(o => o.Items)
                .Include(o => o.Store)
                .Where(o => o.Status == OrderStatus.Delivered &&
                            o.PayoutStatus == PayoutStatus.Unpaid &&
                            o.CreatedAt >= periodStart &&
                            o.CreatedAt <= periodEnd)
                .ToListAsync(ct);
        }

        public async Task BulkUpdatePayoutStatusAsync(
            List<Guid> subOrderIds,
            PayoutStatus status,
            CancellationToken ct = default)
        {
            var subOrders = await _context.Set<SubOrder>().Where(o => subOrderIds.Contains(o.Id)).ToListAsync(ct);
            foreach (var subOrder in subOrders)
            {
                subOrder.PayoutStatus = status;
            }
        }
    }
}

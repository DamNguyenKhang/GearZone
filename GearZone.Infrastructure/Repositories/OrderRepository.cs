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

        public async Task<List<Order>> GetOrdersNotTransfer()
        {
            var orders = await _dbSet
            .Where(o =>
                o.Status == OrderStatus.Delivered &&
                o.PayoutStatus == PayoutStatus.Unpaid &&
                o.UpdatedAt <= DateTime.UtcNow.AddDays(-7)
            )
            .ToListAsync();
            return orders;
        }

        public async Task<List<Order>> GetEligibleForPayoutAsync(
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken ct = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                .Include(o => o.Store)
                .Where(o => o.Status == OrderStatus.Delivered &&
                            o.PayoutStatus == PayoutStatus.Unpaid &&
                            o.CreatedAt >= periodStart &&
                            o.CreatedAt <= periodEnd)
                .ToListAsync(ct);
        }

        public async Task BulkUpdatePayoutStatusAsync(
            List<Guid> orderIds,
            PayoutStatus status,
            CancellationToken ct = default)
        {
            var orders = await _dbSet.Where(o => orderIds.Contains(o.Id)).ToListAsync(ct);
            foreach (var order in orders)
            {
                order.PayoutStatus = status;
            }
        }

        public async Task<GearZone.Application.Common.Models.PagedResult<Order>> GetAdminOrdersAsync(GearZone.Application.Features.Admin.Dtos.AdminOrderQueryDto queryDto)
        {
            var query = _dbSet
                .Include(o => o.Store)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(o => 
                    o.OrderCode.ToString().Contains(search) || 
                    o.ReceiverName.ToLower().Contains(search) ||
                    o.User.UserName.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Status) && Enum.TryParse<OrderStatus>(queryDto.Status, out var status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(queryDto.PayoutStatus) && Enum.TryParse<PayoutStatus>(queryDto.PayoutStatus, out var payoutStatus))
            {
                query = query.Where(o => o.PayoutStatus == payoutStatus);
            }

            if (queryDto.StoreId.HasValue && queryDto.StoreId.Value != Guid.Empty)
            {
                query = query.Where(o => o.StoreId == queryDto.StoreId.Value);
            }

            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= queryDto.StartDate.Value);
            }

            if (queryDto.EndDate.HasValue)
            {
                var endLocal = queryDto.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.CreatedAt <= endLocal);
            }

            if (queryDto.MinPrice.HasValue)
            {
                query = query.Where(o => o.GrandTotal >= queryDto.MinPrice.Value);
            }

            if (queryDto.MaxPrice.HasValue)
            {
                query = query.Where(o => o.GrandTotal <= queryDto.MaxPrice.Value);
            }

            // Default sort by CreatedAt Desc
            if (string.IsNullOrWhiteSpace(queryDto.SortBy))
            {
                query = query.OrderByDescending(o => o.CreatedAt);
            }
            else
            {
                bool isDesc = queryDto.SortDirection?.ToLower() == "desc";
                switch (queryDto.SortBy.ToLower())
                {
                    case "ordercode":
                        query = isDesc ? query.OrderByDescending(o => o.OrderCode) : query.OrderBy(o => o.OrderCode);
                        break;
                    case "grandtotal":
                        query = isDesc ? query.OrderByDescending(o => o.GrandTotal) : query.OrderBy(o => o.GrandTotal);
                        break;
                    case "commission":
                        query = isDesc ? query.OrderByDescending(o => o.CommissionAmount) : query.OrderBy(o => o.CommissionAmount);
                        break;
                    case "createdat":
                        query = isDesc ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt);
                        break;
                    default:
                        query = query.OrderByDescending(o => o.CreatedAt);
                        break;
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .ToListAsync();

            return new GearZone.Application.Common.Models.PagedResult<Order>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<GearZone.Application.Features.Admin.Dtos.AdminOrderStatsDto> GetAdminOrderStatsAsync()
        {
            var stats = new GearZone.Application.Features.Admin.Dtos.AdminOrderStatsDto();

            stats.TotalOrders = await _dbSet.CountAsync();
            stats.PendingOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Pending);
            stats.CompletedOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid);
            stats.CancelledOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Rejected);

            return stats;
        }
    }
}

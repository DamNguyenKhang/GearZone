using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class SubOrderRepository : Repository<SubOrder, Guid>, ISubOrderRepository
    {
        public SubOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SubOrder>> GetOrdersNotTransfer()
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

        public async Task<List<SubOrder>> GetEligibleForPayoutAsync(
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
            List<Guid> subOrderIds,
            PayoutStatus status,
            CancellationToken ct = default)
        {
            var subOrders = await _dbSet.Where(o => subOrderIds.Contains(o.Id)).ToListAsync(ct);
            foreach (var subOrder in subOrders)
            {
                subOrder.PayoutStatus = status;
            }
        }

        public async Task<PagedResult<SubOrder>> GetAdminOrdersAsync(AdminOrderQueryDto queryDto)
        {
            var query = _dbSet
                .Include(o => o.Store)
                .Include(o => o.Order)
                .ThenInclude(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(o => 
                    o.Order.OrderCode.ToString().Contains(search) || 
                    o.Order.ReceiverName.ToLower().Contains(search) ||
                    o.Order.User.UserName.ToLower().Contains(search));
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
                query = query.Where(o => o.Subtotal >= queryDto.MinPrice.Value);
            }

            if (queryDto.MaxPrice.HasValue)
            {
                query = query.Where(o => o.Subtotal <= queryDto.MaxPrice.Value);
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
                        query = isDesc ? query.OrderByDescending(o => o.Order.OrderCode) : query.OrderBy(o => o.Order.OrderCode);
                        break;
                    case "grandtotal":
                        query = isDesc ? query.OrderByDescending(o => o.Subtotal) : query.OrderBy(o => o.Subtotal);
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

            return new PagedResult<SubOrder>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<AdminOrderStatsDto> GetAdminOrderStatsAsync()
        {
            var stats = new AdminOrderStatsDto();

            stats.TotalOrders = await _dbSet.CountAsync();
            stats.PaidOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid);
            stats.UnpaidOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Pending);
            stats.TotalRevenue = await _dbSet.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Paid).SumAsync(o => o.Subtotal);

            return stats;
        }
    }
}

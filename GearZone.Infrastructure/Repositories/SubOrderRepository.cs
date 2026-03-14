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
                o.Status == OrderStatus.Completed &&
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
                .Where(o => o.Status == OrderStatus.Completed &&
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
            stats.PaidOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid);
            stats.UnpaidOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Pending);
            stats.TotalRevenue = await _dbSet.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid).SumAsync(o => o.Subtotal);

            return stats;
        }

        public async Task<decimal> GetTotalEligiblePayoutAmountAsync(CancellationToken ct = default)
        {
            return await _dbSet
                .Where(x => x.Status == OrderStatus.Completed && x.PayoutStatus == PayoutStatus.Unpaid)
                .SumAsync(x => x.NetAmount, ct);
        }

        public async Task<List<SubOrder>> GetDeliveredOrdersForAutoCompleteAsync(int days, CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-days);
            return await _dbSet
                .Where(o => o.Status == OrderStatus.Delivered &&
                            (o.UpdatedAt ?? o.CreatedAt) <= threshold)
                .ToListAsync(ct);
        }

        public async Task<List<ChartDataPoint>> GetRevenueOverviewAsync(DateTime start, DateTime end, string period, CancellationToken ct = default)
        {
            var query = _dbSet
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end && so.Status != OrderStatus.Cancelled);

            if (period == "year")
            {
                return await query
                    .GroupBy(so => new { so.CreatedAt.Year, so.CreatedAt.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new ChartDataPoint
                    {
                        Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Value = g.Sum(x => x.Subtotal),
                        SecondaryValue = g.Sum(x => x.NetAmount)
                    })
                    .ToListAsync(ct);
            }

            return await query
                .GroupBy(so => so.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString("dd MMM"),
                    Value = g.Sum(x => x.Subtotal),
                    SecondaryValue = g.Sum(x => x.NetAmount)
                })
                .ToListAsync(ct);
        }

        public async Task<List<CategoryRevenueDto>> GetCategoryBreakdownAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var data = await _context.OrderItems
                .Include(oi => oi.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Category)
                .Where(oi => oi.SubOrder.CreatedAt >= start && oi.SubOrder.CreatedAt <= end && oi.SubOrder.Status != OrderStatus.Cancelled)
                .GroupBy(oi => oi.Variant.Product.Category.Name)
                .Select(g => new CategoryRevenueDto
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync(ct);

            var total = data.Sum(x => x.Revenue);
            if (total > 0)
            {
                foreach (var item in data)
                {
                    item.Percentage = (double)(item.Revenue / total * 100);
                }
            }

            return data;
        }

        public async Task<List<OrderStatusBreakdownDto>> GetOrderStatusBreakdownAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var groups = await _dbSet
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end)
                .GroupBy(so => so.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            var total = groups.Sum(x => x.Count);
            return groups.Select(g => new OrderStatusBreakdownDto
            {
                Status = g.Status.ToString(),
                Count = g.Count,
                Percentage = total > 0 ? (double)g.Count / total * 100 : 0,
                ColorClass = GetStatusColor(g.Status)
            }).ToList();
        }

        public async Task<List<DashboardStoreDto>> GetTopStoresAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(so => so.Store)
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end && so.Status != OrderStatus.Cancelled)
                .GroupBy(so => new { so.StoreId, so.Store.StoreName, so.Store.LogoUrl, so.Store.CommissionRate, so.Store.Status })
                .Select(g => new DashboardStoreDto
                {
                    StoreId = g.Key.StoreId,
                    StoreName = g.Key.StoreName,
                    LogoUrl = g.Key.LogoUrl,
                    Revenue = g.Sum(x => x.Subtotal),
                    Orders = g.Count(),
                    Commission = g.Sum(x => x.CommissionAmount),
                    Status = g.Key.Status.ToString(),
                    Rating = 4.8 // Placeholder rating
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync(ct);
        }

        public async Task<decimal> GetGrossRevenueAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end && so.Status != OrderStatus.Cancelled)
                .SumAsync(so => (decimal?)so.Subtotal, ct) ?? 0;
        }

        public async Task<int> GetTotalOrdersCountAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _dbSet
                .CountAsync(so => so.CreatedAt >= start && so.CreatedAt <= end, ct);
        }

        private string GetStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Completed => "bg-primary",
                OrderStatus.Processing => "bg-amber-400",
                OrderStatus.Cancelled => "bg-slate-200",
                _ => "bg-slate-400"
            };
        }
    }
}

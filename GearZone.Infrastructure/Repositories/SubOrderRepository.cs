using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Orders.Dtos;
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

        public async Task<decimal> GetTotalEligiblePayoutAmountAsync(CancellationToken ct = default)
        {
            return await _dbSet
                .Where(x => x.Status == OrderStatus.Delivered && x.PayoutStatus == PayoutStatus.Unpaid)
                .SumAsync(x => x.NetAmount, ct);
        }

        public async Task<PagedResult<UserOrderDto>> GetUserOrdersAsync(string userId, UserOrderQueryDto queryDto, DateTime utcNow, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.Order.UserId == userId);

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.Order.OrderCode.ToString().Contains(search) ||
                    x.Items.Any(item => item.ProductNameSnapshot.ToLower().Contains(search)));
            }

            var reviewWindowStart = utcNow.AddDays(-7);
            var status = (queryDto.Status ?? "all").Trim().ToLowerInvariant();
            query = status switch
            {
                "processing" => query.Where(x =>
                    x.Status == OrderStatus.Pending ||
                    x.Status == OrderStatus.Approved ||
                    x.Status == OrderStatus.Paid ||
                    x.Status == OrderStatus.Processing),
                "delivered" => query.Where(x => x.Status == OrderStatus.Delivered),
                "cancelled" => query.Where(x =>
                    x.Status == OrderStatus.Cancelled ||
                    x.Status == OrderStatus.Refunded ||
                    x.Status == OrderStatus.Rejected),
                "to_review" => query.Where(x =>
                    x.Status == OrderStatus.Delivered &&
                    (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart &&
                    x.Items.Any(item => !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted))),
                _ => query
            };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(x => new UserOrderDto
                {
                    SubOrderId = x.Id,
                    OrderId = x.OrderId,
                    StoreId = x.StoreId,
                    StoreName = x.Store.StoreName,
                    StoreSlug = x.Store.Slug,
                    OrderCode = x.Order.OrderCode,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    DeliveredAt = x.DeliveredAt,
                    Subtotal = x.Subtotal,
                    HasAnyReviewableItem = x.Status == OrderStatus.Delivered
                        && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                        && x.Items.Any(item => !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted)),
                    HasAnyEditableReview = x.Status == OrderStatus.Delivered
                        && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                        && x.Items.Any(item => _context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted)),
                    Items = x.Items
                        .OrderBy(item => item.ProductNameSnapshot)
                        .Select(item => new UserOrderItemDto
                        {
                            OrderItemId = item.Id,
                            ProductId = item.Variant.ProductId,
                            ProductName = item.ProductNameSnapshot,
                            ProductSlug = item.Variant.Product.Slug,
                            ProductImageUrl = item.Variant.Product.Images
                                .Where(img => img.IsPrimary)
                                .Select(img => img.ImageUrl)
                                .FirstOrDefault()
                                ?? item.Variant.Product.Images.Select(img => img.ImageUrl).FirstOrDefault(),
                            VariantName = item.VariantNameSnapshot,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPriceSnapshot,
                            CanReview = x.Status == OrderStatus.Delivered
                                && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                                && !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted),
                            CanEditReview = x.Status == OrderStatus.Delivered
                                && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                                && _context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted),
                            ReviewId = _context.ProductReviews
                                .Where(review => review.OrderItemId == item.Id && !review.IsDeleted)
                                .Select(review => (Guid?)review.Id)
                                .FirstOrDefault(),
                            ReviewDeadline = (x.Status == OrderStatus.Delivered
                                ? (DateTime?)((x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt).AddDays(7))
                                : null)
                        })
                        .ToList()
                })
                .ToListAsync(ct);

            return new PagedResult<UserOrderDto>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<UserOrderStatusSummaryDto> GetUserOrderStatusSummaryAsync(string userId, DateTime utcNow, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.Order.UserId == userId);

            var reviewWindowStart = utcNow.AddDays(-7);

            return new UserOrderStatusSummaryDto
            {
                All = await query.CountAsync(ct),
                Processing = await query.CountAsync(x =>
                    x.Status == OrderStatus.Pending ||
                    x.Status == OrderStatus.Approved ||
                    x.Status == OrderStatus.Paid ||
                    x.Status == OrderStatus.Processing, ct),
                Delivered = await query.CountAsync(x => x.Status == OrderStatus.Delivered, ct),
                Cancelled = await query.CountAsync(x =>
                    x.Status == OrderStatus.Cancelled ||
                    x.Status == OrderStatus.Refunded ||
                    x.Status == OrderStatus.Rejected, ct),
                ToReview = await query.CountAsync(x =>
                    x.Status == OrderStatus.Delivered &&
                    (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart &&
                    x.Items.Any(item => !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted)), ct)
            };
        }

        public async Task<PagedResult<SellerChatOrderListItemDto>> GetSellerChatOrdersAsync(string ownerUserId, SellerChatOrderQueryDto queryDto, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.Store.OwnerUserId == ownerUserId);

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.Order.OrderCode.ToString().Contains(search) ||
                    (x.Order.User.FullName != null && x.Order.User.FullName.ToLower().Contains(search)) ||
                    (x.Order.User.UserName != null && x.Order.User.UserName.ToLower().Contains(search)) ||
                    (x.Order.User.Email != null && x.Order.User.Email.ToLower().Contains(search)) ||
                    x.Items.Any(item => item.ProductNameSnapshot.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(x => new SellerChatOrderListItemDto
                {
                    SubOrderId = x.Id,
                    OrderCode = x.Order.OrderCode,
                    StoreId = x.StoreId,
                    StoreName = x.Store.StoreName,
                    BuyerUserId = x.Order.UserId,
                    BuyerDisplayName = x.Order.User.FullName ?? x.Order.User.UserName ?? x.Order.User.Email ?? "Buyer",
                    BuyerAvatarUrl = x.Order.User.AvatarUrl,
                    CreatedAt = x.CreatedAt,
                    DeliveredAt = x.DeliveredAt,
                    Status = x.Status,
                    Subtotal = x.Subtotal,
                    ItemCount = x.Items.Count,
                    ProductPreview = x.Items
                        .OrderBy(item => item.ProductNameSnapshot)
                        .Select(item => item.ProductNameSnapshot)
                        .FirstOrDefault() ?? "Order items"
                })
                .ToListAsync(ct);

            return new PagedResult<SellerChatOrderListItemDto>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<SubOrder?> GetSellerChatSubOrderAsync(string ownerUserId, Guid subOrderId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(x => x.Order)
                .Include(x => x.Order.User)
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == subOrderId && x.Store.OwnerUserId == ownerUserId, ct);
        }

        public async Task<List<ChatContextOrderDto>> GetConversationOrderContextAsync(string buyerUserId, Guid storeId, int take, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Order.UserId == buyerUserId && x.StoreId == storeId)
                .OrderByDescending(x => x.DeliveredAt ?? x.CreatedAt)
                .Take(take)
                .Select(x => new ChatContextOrderDto
                {
                    SubOrderId = x.Id,
                    OrderCode = x.Order.OrderCode,
                    CreatedAt = x.CreatedAt,
                    DeliveredAt = x.DeliveredAt,
                    Status = x.Status,
                    Subtotal = x.Subtotal,
                    ItemCount = x.Items.Count,
                    ProductPreview = x.Items
                        .OrderBy(item => item.ProductNameSnapshot)
                        .Select(item => item.ProductNameSnapshot)
                        .FirstOrDefault() ?? "Order items"
                })
                .ToListAsync(ct);
        }
    }
}

using System;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using System;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<List<Order>> GetOrdersNotTransfer();

        Task<List<Order>> GetEligibleForPayoutAsync(
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken ct = default);

        Task BulkUpdatePayoutStatusAsync(
            List<Guid> orderIds,
            PayoutStatus status,
            CancellationToken ct = default);

        Task<PagedResult<Order>> GetAdminOrdersAsync(GearZone.Application.Features.Admin.Dtos.AdminOrderQueryDto queryDto);
        Task<AdminOrderStatsDto> GetAdminOrderStatsAsync();
    }
}

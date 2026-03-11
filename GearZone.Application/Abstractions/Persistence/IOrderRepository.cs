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
        Task<List<SubOrder>> GetOrdersNotTransfer();

        Task<List<SubOrder>> GetEligibleForPayoutAsync(
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken ct = default);

        Task BulkUpdatePayoutStatusAsync(
            List<Guid> subOrderIds,
            PayoutStatus status,
            CancellationToken ct = default);

        Task<PagedResult<Order>> GetAdminOrdersAsync(GearZone.Application.Features.Admin.Dtos.AdminOrderQueryDto queryDto);
        Task<AdminOrderStatsDto> GetAdminOrderStatsAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ISubOrderRepository : IRepository<SubOrder, Guid>
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

        Task<PagedResult<SubOrder>> GetAdminOrdersAsync(AdminOrderQueryDto queryDto);
        Task<AdminOrderStatsDto> GetAdminOrderStatsAsync();
        Task<decimal> GetTotalEligiblePayoutAmountAsync(CancellationToken ct = default);
    }
}

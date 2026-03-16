using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IVoucherRepository : IRepository<Voucher, Guid>
    {
        Task<PagedResult<Voucher>> GetPaginatedAdminVouchersAsync(AdminVoucherQueryDto query);
        Task<AdminVoucherSummaryDto> GetAdminVoucherSummaryAsync();
    }
}

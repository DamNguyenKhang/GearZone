using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminPayoutService
    {
        Task<PagedResult<AdminPayoutTransactionDto>> GetPayoutTransactionsAsync(PayoutTransactionQueryDto query);
    }
}

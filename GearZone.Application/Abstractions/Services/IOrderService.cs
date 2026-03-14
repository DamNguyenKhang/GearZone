using GearZone.Application.Common.Models;
using GearZone.Application.Features.Orders.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface IOrderService
    {
        Task<PagedResult<UserOrderDto>> GetUserOrdersAsync(string userId, UserOrderQueryDto query);
        Task<UserOrderStatusSummaryDto> GetUserOrderStatusSummaryAsync(string userId);
    }
}

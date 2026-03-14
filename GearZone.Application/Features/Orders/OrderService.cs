using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Orders.Dtos;

namespace GearZone.Application.Features.Orders
{
    public class OrderService : IOrderService
    {
        private readonly ISubOrderRepository _subOrderRepository;

        public OrderService(ISubOrderRepository subOrderRepository)
        {
            _subOrderRepository = subOrderRepository;
        }

        public async Task<PagedResult<UserOrderDto>> GetUserOrdersAsync(string userId, UserOrderQueryDto query)
        {
            query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            query.PageSize = query.PageSize < 1 ? 10 : query.PageSize;

            return await _subOrderRepository.GetUserOrdersAsync(userId, query, DateTime.UtcNow);
        }

        public async Task<UserOrderStatusSummaryDto> GetUserOrderStatusSummaryAsync(string userId)
        {
            return await _subOrderRepository.GetUserOrderStatusSummaryAsync(userId, DateTime.UtcNow);
        }
    }
}

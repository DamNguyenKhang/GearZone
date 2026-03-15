using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IOrderService
    {

        Task<Order> CreateOrderAsync(
            string userId,
            CheckoutRequestDto request,
            List<CartItem> cartItems,
            CancellationToken ct = default);
    }
}

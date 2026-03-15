using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<GearZone.Domain.Entities.Order> CreateOrderAsync(
            string userId,
            CheckoutRequestDto request,
            List<CartItem> cartItems,
            CancellationToken ct = default)
        {
            // Sử dụng địa chỉ từ request trực tiếp
            var shippingAddressStr = request.ShippingInfo.Address;

            // Group cart items theo Store để tạo SubOrder
            var storeGroups = cartItems.GroupBy(ci => ci.Variant.Product.StoreId).ToList();

            long orderCode = long.Parse(
                DateTime.UtcNow.ToString("yyMMddHHmmss") + new Random().Next(10, 99).ToString());

            decimal totalShippingFee = 0m; // Free shipping hiện tại
            decimal grandTotal = 0m;

            var order = new GearZone.Domain.Entities.Order
            {
                Id = Guid.NewGuid(),
                OrderCode = orderCode,
                UserId = userId,
                ShippingFee = totalShippingFee,
                ReceiverName = request.ShippingInfo.FullName,
                ReceiverPhone = request.ShippingInfo.PhoneNumber,
                ShippingAddress = shippingAddressStr,
                CreatedAt = DateTime.UtcNow,
                StatusHistories = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        NewStatus = OrderStatus.Pending,
                        ChangedAt = DateTime.UtcNow,
                        ChangedByUserId = userId
                    }
                }
            };

            foreach (var group in storeGroups)
            {
                var storeId = group.Key;
                decimal subtotal = group.Sum(ci => ci.Quantity * ci.Variant.Price);
                decimal commissionRate = 0.05m; // 5% commission mặc định
                decimal commissionAmount = subtotal * commissionRate;
                decimal netAmount = subtotal - commissionAmount;

                var subOrder = new SubOrder
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    StoreId = storeId,
                    Status = OrderStatus.Pending,
                    PayoutStatus = PayoutStatus.Unpaid,
                    Subtotal = subtotal,
                    CommissionRateSnapshot = commissionRate,
                    CommissionAmount = commissionAmount,
                    NetAmount = netAmount,
                    CreatedAt = DateTime.UtcNow,
                    Items = group.Select(ci => new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        VariantId = ci.VariantId,
                        ProductNameSnapshot = ci.Variant.Product.Name,
                        VariantNameSnapshot = ci.Variant.AttributeValues.Any()
                            ? string.Join(", ", ci.Variant.AttributeValues
                                .Select(v => v.CategoryAttributeOption.Value))
                            : string.Empty,
                        SkuSnapshot = ci.Variant.Sku,
                        UnitPriceSnapshot = ci.Variant.Price,
                        Quantity = ci.Quantity,
                        LineTotal = ci.Quantity * ci.Variant.Price
                    }).ToList()
                };

                order.SubOrders.Add(subOrder);
                grandTotal += subtotal;
            }

            order.GrandTotal = grandTotal + totalShippingFee;

            await _orderRepository.AddAsync(order, ct);

            return order;
        }
    }
}

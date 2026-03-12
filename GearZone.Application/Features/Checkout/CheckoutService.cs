using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Checkout
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public CheckoutService(
            ICartItemRepository cartItemRepository,
            IProductVariantRepository productVariantRepository,
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _cartItemRepository = cartItemRepository;
            _productVariantRepository = productVariantRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<CheckoutResponseDto> ProcessCheckoutAsync(string userId, CheckoutRequestDto request, CancellationToken ct = default)
        {
            if (request.CartItemIds == null || !request.CartItemIds.Any())
            {
                return new CheckoutResponseDto { Success = false, ErrorMessage = "No items selected for checkout." };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new CheckoutResponseDto { Success = false, ErrorMessage = "User not found." };
            }

            // Get Cart Items with eager loading
            var cartItems = await _cartItemRepository.Query()
                .Include(ci => ci.Cart)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Store)
                .Where(ci => request.CartItemIds.Contains(ci.Id) && ci.Cart.UserId == userId)
                .ToListAsync(ct);

            if (cartItems.Count != request.CartItemIds.Count)
            {
                return new CheckoutResponseDto { Success = false, ErrorMessage = "One or more invalid cart items selected." };
            }
            // Deduct Stock
            foreach (var cartItem in cartItems)
            {
                if (cartItem.Variant.StockQuantity < cartItem.Quantity)
                {
                    return new CheckoutResponseDto { Success = false, ErrorMessage = $"Insufficient stock for {cartItem.Variant.Product.Name}." };
                }
                cartItem.Variant.StockQuantity -= cartItem.Quantity;
                await _productVariantRepository.UpdateAsync(cartItem.Variant);
            }

            // Group by Store
            var storeGroups = cartItems.GroupBy(ci => ci.Variant.Product.StoreId).ToList();

            long orderCode = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmss") + new Random().Next(10, 99).ToString());
            var addressComponents = new[] { 
                request.ShippingInfo.StreetAddress, 
                request.ShippingInfo.Ward, 
                request.ShippingInfo.District, 
                request.ShippingInfo.City 
            };
            var shippingAddressStr = string.Join(", ", addressComponents
                .Where(s => !string.IsNullOrWhiteSpace(s) && s != "N/A"));

            // Calculate Grand Total
            decimal grandTotal = 0;
            decimal totalShippingFee = storeGroups.Count * 0; // Currently Free shipping per store

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
                    new OrderStatusHistory { NewStatus = OrderStatus.Pending, ChangedAt = DateTime.UtcNow, ChangedByUserId = userId }
                }
            };

            foreach (var group in storeGroups)
            {
                var storeId = group.Key;
                decimal subtotal = group.Sum(ci => ci.Quantity * ci.Variant.Price);
                decimal commissionRate = 0.05m; // 5% default commission
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
                        VariantNameSnapshot = string.Join(", ", ci.Variant.AttributeValues.Select(v => v.CategoryAttributeOption.Value)),
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

            if (request.PaymentMethod == PaymentMethod.COD)
            {
                // COD is pending processing by store
                // We keep OrderStatus.Pending for suborders
            }

            await _orderRepository.AddAsync(order);

            // Clear selected items from cart
            foreach (var ci in cartItems)
            {
                await _cartItemRepository.DeleteAsync(ci);
            }

            // Save Address
            if (request.SaveAddress)
            {
                user.Address = shippingAddressStr;
                user.FullName = request.ShippingInfo.FullName;
                user.PhoneNumber = request.ShippingInfo.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return new CheckoutResponseDto
            {
                Success = true,
                OrderId = order.Id,
                OrderCode = order.OrderCode.ToString()
            };
        }
    }
}

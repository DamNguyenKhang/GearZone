using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.Payment;
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
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentStrategyFactory _paymentStrategyFactory;
        private readonly IBackgroundJobService _backgroundJobService;

        public CheckoutService(
            ICartItemRepository cartItemRepository,
            IProductVariantRepository productVariantRepository,
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            PaymentStrategyFactory paymentStrategyFactory,
            IBackgroundJobService backgroundJobService)
        {
            _cartItemRepository = cartItemRepository;
            _productVariantRepository = productVariantRepository;
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _paymentStrategyFactory = paymentStrategyFactory;
            _backgroundJobService = backgroundJobService;
        }

        public async Task<CheckoutResponseDto> ProcessCheckoutAsync(
            string userId,
            CheckoutRequestDto request,
            CancellationToken ct = default)
        {
            // 1. Validate input
            if (request.CartItemIds == null || !request.CartItemIds.Any())
                return new CheckoutResponseDto { Success = false, ErrorMessage = "No items selected for checkout." };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new CheckoutResponseDto { Success = false, ErrorMessage = "User not found." };

            // 2. Fetch cart items
            var cartItems = await _cartItemRepository.GetCartItemsForCheckoutAsync(
                request.CartItemIds, userId, ct);

            if (cartItems.Count != request.CartItemIds.Count)
                return new CheckoutResponseDto { Success = false, ErrorMessage = "One or more invalid cart items selected." };

            // 3. Validate stock & deduct
            foreach (var cartItem in cartItems)
            {
                if (cartItem.Variant.StockQuantity < cartItem.Quantity)
                    return new CheckoutResponseDto
                    {
                        Success = false,
                        ErrorMessage = $"Insufficient stock for {cartItem.Variant.Product.Name}."
                    };

                cartItem.Variant.StockQuantity -= cartItem.Quantity;
                await _productVariantRepository.UpdateAsync(cartItem.Variant);
            }

            // 4. Create order (status depends on payment method)
            var order = await _orderService.CreateOrderAsync(userId, request, cartItems, ct);

            // 5. Process payment via Strategy Pattern
            var strategy = _paymentStrategyFactory.GetStrategy(request.PaymentMethod);
            var paymentResult = await strategy.ProcessPaymentAsync(order);

            if (!paymentResult.Success)
            {
                return new CheckoutResponseDto
                {
                    Success = false,
                    ErrorMessage = paymentResult.ErrorMessage ?? "Payment processing failed."
                };
            }

            // 6. Clear cart items
            await _cartService.ClearCartItemsAsync(request.CartItemIds, ct);

            // 7. Save address if requested
            if (request.SaveAddress)
            {
                user.FullName = request.ShippingInfo.FullName;
                user.Address = request.ShippingInfo.Address;
                user.PhoneNumber = request.ShippingInfo.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            // 8. Persist all changes
            await _unitOfWork.SaveChangesAsync(ct);

            // 9. Schedule real-time timeout job if PayOS
            if (request.PaymentMethod == PaymentMethod.PayOS)
            {
                _backgroundJobService.SchedulePaymentTimeout(order.Id, TimeSpan.FromMinutes(15));
            }

            return new CheckoutResponseDto
            {
                Success = true,
                OrderId = order.Id,
                OrderCode = order.OrderCode.ToString(),
                CheckoutUrl = paymentResult.CheckoutUrl  // null for COD, URL for PayOS
            };
        }

        public async Task<List<CartItem>> GetCheckoutItemsAsync(string userId, List<Guid> cartItemIds, CancellationToken ct = default)
        {
            return await _cartItemRepository.Query()
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Store)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Images)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Where(ci => cartItemIds.Contains(ci.Id) && ci.Cart.UserId == userId)
                .ToListAsync(ct);
        }
    }
}

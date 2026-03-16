using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
<<<<<<< HEAD
using GearZone.Application.Features.Payment;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
=======
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
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
<<<<<<< HEAD
        private readonly PaymentStrategyFactory _paymentStrategyFactory;
        private readonly IBackgroundJobService _backgroundJobService;
=======
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5

        public CheckoutService(
            ICartItemRepository cartItemRepository,
            IProductVariantRepository productVariantRepository,
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
<<<<<<< HEAD
            IUnitOfWork unitOfWork,
            PaymentStrategyFactory paymentStrategyFactory,
            IBackgroundJobService backgroundJobService)
=======
            IUnitOfWork unitOfWork)
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
        {
            _cartItemRepository = cartItemRepository;
            _productVariantRepository = productVariantRepository;
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
<<<<<<< HEAD
            _paymentStrategyFactory = paymentStrategyFactory;
            _backgroundJobService = backgroundJobService;
=======
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
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

<<<<<<< HEAD
            // 2. Fetch cart items
=======
            // 2. Lấy cart items (EF query nằm hoàn toàn trong repository)
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
            var cartItems = await _cartItemRepository.GetCartItemsForCheckoutAsync(
                request.CartItemIds, userId, ct);

            if (cartItems.Count != request.CartItemIds.Count)
                return new CheckoutResponseDto { Success = false, ErrorMessage = "One or more invalid cart items selected." };

<<<<<<< HEAD
            // 3. Validate stock & deduct
=======
            // 3. Kiểm tra và trừ tồn kho
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
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

<<<<<<< HEAD
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
=======
            // 4. Tạo order (logic nằm trong OrderService)
            var order = await _orderService.CreateOrderAsync(userId, request, cartItems, ct);

            // 5. Xóa các cart items đã checkout (logic nằm trong CartService)
            await _cartService.ClearCartItemsAsync(request.CartItemIds, ct);

            // 6. Lưu địa chỉ nếu người dùng yêu cầu
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
            if (request.SaveAddress)
            {
                user.FullName = request.ShippingInfo.FullName;
                user.Address = request.ShippingInfo.Address;
                user.PhoneNumber = request.ShippingInfo.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

<<<<<<< HEAD
            // 8. Persist all changes
            await _unitOfWork.SaveChangesAsync(ct);

            // 9. Schedule real-time timeout job if PayOS
            if (request.PaymentMethod == PaymentMethod.PayOS)
            {
                _backgroundJobService.SchedulePaymentTimeout(order.Id, TimeSpan.FromMinutes(15));
            }

=======
            // 7. Persist toàn bộ thay đổi trong một transaction
            await _unitOfWork.SaveChangesAsync(ct);

>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
            return new CheckoutResponseDto
            {
                Success = true,
                OrderId = order.Id,
<<<<<<< HEAD
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
=======
                OrderCode = order.OrderCode.ToString()
            };
        }
>>>>>>> b1f554fccf634aaff47f8754e83cb1481130efe5
    }
}

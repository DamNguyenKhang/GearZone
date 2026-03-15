using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
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

        public CheckoutService(
            ICartItemRepository cartItemRepository,
            IProductVariantRepository productVariantRepository,
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _cartItemRepository = cartItemRepository;
            _productVariantRepository = productVariantRepository;
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
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

            // 2. Lấy cart items (EF query nằm hoàn toàn trong repository)
            var cartItems = await _cartItemRepository.GetCartItemsForCheckoutAsync(
                request.CartItemIds, userId, ct);

            if (cartItems.Count != request.CartItemIds.Count)
                return new CheckoutResponseDto { Success = false, ErrorMessage = "One or more invalid cart items selected." };

            // 3. Kiểm tra và trừ tồn kho
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

            // 4. Tạo order (logic nằm trong OrderService)
            var order = await _orderService.CreateOrderAsync(userId, request, cartItems, ct);

            // 5. Xóa các cart items đã checkout (logic nằm trong CartService)
            await _cartService.ClearCartItemsAsync(request.CartItemIds, ct);

            // 6. Lưu địa chỉ nếu người dùng yêu cầu
            if (request.SaveAddress)
            {
                user.FullName = request.ShippingInfo.FullName;
                user.Address = request.ShippingInfo.Address;
                user.PhoneNumber = request.ShippingInfo.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            // 7. Persist toàn bộ thay đổi trong một transaction
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

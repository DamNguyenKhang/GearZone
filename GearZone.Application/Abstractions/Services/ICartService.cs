using GearZone.Application.Features.Cart.DTOs;

namespace GearZone.Application.Abstractions.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(string userId, Guid variantId, int quantity);
        Task UpdateCartItemQuantityAsync(Guid cartItemId, int newQuantity, string userId);
        Task RemoveCartItemAsync(Guid cartItemId, string userId);
        Task<CartDto?> GetCartAsync(string userId);
        Task<int> GetCartItemsCountAsync(string userId);
    }
}

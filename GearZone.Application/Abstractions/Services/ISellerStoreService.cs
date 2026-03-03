using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services;

public interface ISellerStoreService
{
    Task<bool> ApplyForStoreAsync(StoreRegistrationDto storeRegistrationDto);
    Task<Store?> GetStoreByOwnerIdAsync(string userId);
}

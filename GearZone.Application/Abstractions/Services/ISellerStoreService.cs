using GearZone.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services;

public interface ISellerStoreService
{
    Task<bool> ApplyForStoreAsync(Store store);
    Task<Store?> GetStoreByOwnerIdAsync(string userId);
}

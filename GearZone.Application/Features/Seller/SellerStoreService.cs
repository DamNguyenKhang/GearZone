using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Seller;

public class SellerStoreService : ISellerStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SellerStoreService(IStoreRepository storeRepository, ISystemSettingRepository systemSettingRepository, IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _systemSettingRepository = systemSettingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ApplyForStoreAsync(StoreRegistrationDto storeRegistrationDto)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync("Payment_CommissionRate");
        decimal commissionRate = 0.05m; // Default
        if (setting != null && decimal.TryParse(setting.Value, out decimal parsedRate))
        {
            commissionRate = parsedRate;
        }

        var store = new Store
        {
            Id = Guid.NewGuid(),
            OwnerUserId = storeRegistrationDto.OwnerUserId,
            StoreName = storeRegistrationDto.StoreName,
            Slug = storeRegistrationDto.StoreName.ToLower().Replace(" ", "-"),
            TaxCode = storeRegistrationDto.TaxCode,
            Phone = storeRegistrationDto.Phone,
            Email = storeRegistrationDto.Email,
            AddressLine = storeRegistrationDto.AddressLine,
            Province = storeRegistrationDto.Province,
            Status = StoreStatus.Pending,
            CommissionRate = commissionRate,
            CreatedAt = DateTime.UtcNow
        };

        await _storeRepository.AddAsync(store);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Store?> GetStoreByOwnerIdAsync(string userId)
    {
        return await _storeRepository.GetStoreByOwnerIdAsync(userId);
    }
}

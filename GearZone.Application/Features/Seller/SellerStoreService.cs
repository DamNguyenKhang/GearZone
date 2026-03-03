using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Seller;

public class SellerStoreService : ISellerStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SellerStoreService(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ApplyForStoreAsync(Store store)
    {
        await _storeRepository.AddAsync(store);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Store?> GetStoreByOwnerIdAsync(string userId)
    {
        // We'll need to use the repository to find by owner ID. 
        // Since IStoreRepository inherits from IRepository<Store, Guid>, 
        // we might need to add a specialized method to IStoreRepository if not already there,
        // or just use a generic way if available.
        // For now, I'll assume we might need to add it to IStoreRepository to avoid direct DbContext.
        return await _storeRepository.GetStoreByOwnerIdAsync(userId);
    }
}

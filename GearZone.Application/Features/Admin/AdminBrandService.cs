using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin;

public class AdminBrandService : IAdminBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminBrandService(IBrandRepository brandRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<AdminBrandDto>> GetBrandsAsync(AdminBrandQueryDto query)
    {
        var pagedBrands = await _brandRepository.GetAdminBrandsAsync(query);

        return new PagedResult<AdminBrandDto>
        {
            Items = _mapper.Map<List<AdminBrandDto>>(pagedBrands.Items),
            TotalCount = pagedBrands.TotalCount,
            PageNumber = pagedBrands.PageNumber,
            PageSize = pagedBrands.PageSize
        };
    }

    public async Task<AdminBrandDto?> GetBrandByIdAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null) return null;

        return _mapper.Map<AdminBrandDto>(brand);
    }

    public async Task<bool> ApproveBrandAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.IsApproved) return false;

        brand.IsApproved = true;
        brand.UpdatedAt = DateTime.UtcNow;

        _brandRepository.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectBrandAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || !brand.IsApproved) return false;

        brand.IsApproved = false;
        brand.UpdatedAt = DateTime.UtcNow;

        _brandRepository.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteBrandAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.IsDeleted) return false;

        brand.IsDeleted = true;
        brand.UpdatedAt = DateTime.UtcNow;

        _brandRepository.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<AdminBrandStatsDto> GetBrandStatsAsync()
    {
        return await _brandRepository.GetBrandStatsAsync();
    }
}

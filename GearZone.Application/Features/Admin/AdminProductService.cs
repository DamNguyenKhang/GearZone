using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Admin
{
    public class AdminProductService : IAdminProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminProductService(IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdminProductDto>> GetProductsAsync(AdminProductQueryDto queryDto)
        {
            var pagedProducts = await _productRepository.GetAdminProductsAsync(queryDto);

            var items = _mapper.Map<List<AdminProductDto>>(pagedProducts.Items);

            return new PagedResult<AdminProductDto>(items, pagedProducts.TotalCount, pagedProducts.PageNumber, pagedProducts.PageSize);
        }

        public async Task<AdminProductStatsDto> GetProductStatsAsync()
        {
            return await _productRepository.GetAdminProductStatsAsync();
        }

        public async Task<AdminProductDetailDto?> GetProductDetailAsync(Guid id)
        {
            var product = await _productRepository.GetAdminProductDetailAsync(id);
            if (product == null)
                return null;

            return _mapper.Map<AdminProductDetailDto>(product);
        }

        public async Task ApproveProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) throw new InvalidOperationException("Product not found");

            product.Status = ProductStatus.Active;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RejectProductAsync(Guid id, string reason)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) throw new InvalidOperationException("Product not found");

            product.Status = ProductStatus.Inactive; // or Draft or Archive? Let's use Inactive for now.
            product.UpdatedAt = DateTime.UtcNow;
            
            // Maybe add a rejection record if there's a place for it, but for now just status change.
            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

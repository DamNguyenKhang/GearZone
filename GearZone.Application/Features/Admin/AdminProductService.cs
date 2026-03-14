using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AdminProductService(IProductRepository productRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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

        public async Task<bool> BulkUpdateStatusAsync(List<Guid> productIds, ProductStatus status)
        {
            if (productIds == null || !productIds.Any())
                return false;

            var success = false;
            foreach (var id in productIds)
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product != null)
                {
                    product.Status = status;
                    await _productRepository.UpdateAsync(product);
                    success = true;
                }
            }

            if (success)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return success;
        }
    }
}
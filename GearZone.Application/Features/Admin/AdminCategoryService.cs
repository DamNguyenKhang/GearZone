using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AdminCategoryService(ICategoryRepository categoryRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<CategoryDto>> GetPaginatedCategoriesAsync(CategoryQueryDto query)
        {
            var pagedCategories = await _categoryRepository.GetPaginatedCategoriesAsync(query);
            
            var dtos = _mapper.Map<List<CategoryDto>>(pagedCategories.Items);

            return new PagedResult<CategoryDto>(dtos, pagedCategories.TotalCount, query.PageNumber, query.PageSize);
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                await _categoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(EditCategoryDto dto)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(dto.Id);
                if (category == null) return false;

                category.Name = dto.Name;
                category.Slug = dto.Slug;
                category.ParentId = dto.ParentId;
                category.IsActive = dto.IsActive;

                await _categoryRepository.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null) return false;

                category.IsDeleted = true;
                category.IsActive = false; // Optionally mark as inactive

                await _categoryRepository.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<List<CategoryDto>> GetAllCategoriesListAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }
    }
}

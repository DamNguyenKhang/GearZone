using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminCategoryService
    {
        Task<PagedResult<CategoryDto>> GetPaginatedCategoriesAsync(CategoryQueryDto query);
        Task<bool> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(EditCategoryDto dto);
        Task<bool> SoftDeleteCategoryAsync(int id);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<List<CategoryDto>> GetAllCategoriesListAsync();
    }
}

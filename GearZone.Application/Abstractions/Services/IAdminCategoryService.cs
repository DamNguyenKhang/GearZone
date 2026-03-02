using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminCategoryService
    {
        Task<PagedResult<CategoryDto>> GetPaginatedCategoriesAsync(CategoryQueryDto query);
        Task<bool> CreateCategoryAsync(GearZone.Domain.Entities.Category category);
        Task<bool> UpdateCategoryAsync(EditCategoryDto dto);
        Task<bool> SoftDeleteCategoryAsync(int id);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<System.Collections.Generic.List<CategoryDto>> GetAllCategoriesListAsync();
    }
}

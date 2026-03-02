using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<PagedResult<Category>> GetPaginatedCategoriesAsync(CategoryQueryDto query);
        Task<List<Category>> GetAllCategoriesListAsync();
    }
}

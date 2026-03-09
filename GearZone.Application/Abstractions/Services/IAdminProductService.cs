using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminProductService
    {
        Task<PagedResult<AdminProductDto>> GetProductsAsync(AdminProductQueryDto queryDto);
        Task<AdminProductStatsDto> GetProductStatsAsync();
        Task<AdminProductDetailDto?> GetProductDetailAsync(Guid id);
    }
}

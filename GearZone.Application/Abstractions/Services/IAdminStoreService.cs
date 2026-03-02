using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services;

public interface IAdminStoreService
{
    Task<PagedResult<StoreApplicationDto>> GetStoreApplicationsAsync(StoreApplicationQueryDto query);
    Task<StoreApplicationDto?> GetStoreApplicationByIdAsync(Guid storeId);
    Task<bool> ApproveStoreAsync(Guid storeId);
    Task<bool> RejectStoreAsync(Guid storeId, string reason);
    Task<bool> RequestInfoAsync(Guid storeId, string note);
    Task<StoreApplicationStatsDto> GetStoreApplicationStatsAsync();
}

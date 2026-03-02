using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IStoreRepository : IRepository<Store, Guid>
    {
        Task<PagedResult<Store>> GetStoreApplicationsAsync(StoreApplicationQueryDto query);
        Task<Store?> GetStoreApplicationByIdAsync(Guid storeId);
        Task<StoreApplicationStatsDto> GetStoreApplicationStatsAsync();
    }
}

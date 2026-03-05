using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class StoreRepository : Repository<Store, Guid>, IStoreRepository
    {
        public StoreRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Store>> GetStoreApplicationsAsync(StoreApplicationQueryDto query)
        {
            var dbQuery = _dbSet
                .Include(s => s.OwnerUser)
                .AsQueryable();

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                dbQuery = dbQuery.Where(s => 
                    s.StoreName.ToLower().Contains(search) || 
                    s.Slug.ToLower().Contains(search) || 
                    s.TaxCode.ToLower().Contains(search) ||
                    s.Phone.ToLower().Contains(search) ||
                    (s.OwnerUser != null && s.OwnerUser.Email != null && s.OwnerUser.Email.ToLower().Contains(search)) ||
                    (s.OwnerUser != null && s.OwnerUser.FullName != null && s.OwnerUser.FullName.ToLower().Contains(search)));
            }
            
            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.CreatedAt >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var endOfDay = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                dbQuery = dbQuery.Where(s => s.CreatedAt <= endOfDay);
            }

            var totalCount = await dbQuery.CountAsync();

            var stores = await dbQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Store>
            {
                Items = stores,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<Store?> GetStoreApplicationByIdAsync(Guid storeId)
        {
            return await _dbSet
                .Include(s => s.OwnerUser)
                .FirstOrDefaultAsync(s => s.Id == storeId);
        }

        public async Task<Store?> GetStoreByOwnerIdAsync(string userId)
        {
            return await _dbSet
                .Include(s => s.OwnerUser)
                .FirstOrDefaultAsync(s => s.OwnerUserId == userId);
        }

        public async Task<StoreApplicationStatsDto> GetStoreApplicationStatsAsync()
        {
            var query = _dbSet.AsQueryable();

            return new StoreApplicationStatsDto
            {
                TotalCount = await query.CountAsync(),
                PendingCount = await query.CountAsync(s => s.Status == Domain.Enums.StoreStatus.Pending),
                ApprovedCount = await query.CountAsync(s => s.Status == Domain.Enums.StoreStatus.Approved),
                RejectedCount = await query.CountAsync(s => s.Status == Domain.Enums.StoreStatus.Rejected)
            };
        }

        public async Task<Store?> GetBySlugAsync(string slug)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Slug == slug 
                    && s.Status == Domain.Enums.StoreStatus.Approved);
        }
    }
}

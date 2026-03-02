using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category, int>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Category>> GetPaginatedCategoriesAsync(CategoryQueryDto query)
        {
            var dbQuery = _dbSet
                .Include(c => c.Parent)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                dbQuery = dbQuery.Where(c => 
                    c.Name.ToLower().Contains(search) || 
                    c.Slug.ToLower().Contains(search));
            }

            if (query.IsActive.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.IsActive == query.IsActive.Value);
            }

            if (query.ParentId.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.ParentId == query.ParentId.Value);
            }

            var totalCount = await dbQuery.CountAsync();

            var categories = await dbQuery
                .OrderByDescending(c => c.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Category>
            {
                Items = categories,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<List<Category>> GetAllCategoriesListAsync()
        {
            return await _dbSet.OrderBy(c => c.Name).ToListAsync();
        }
    }
}

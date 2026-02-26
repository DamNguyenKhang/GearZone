using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserDto>> GetUsersAsync(UserQueryDto query)
        {
            var queryable =
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                select new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    Role = r.Name,
                    CreatedAt = u.CreatedAt,
                    IsDeleted = u.IsDeleted
                };

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.Trim().ToLower();
                queryable = queryable.Where(u =>
                    u.FullName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm));
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(u => u.IsActive == query.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                queryable = queryable.Where(u => u.Role == query.Role);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<UserDto>(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize);
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            return await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId into u_ur
                from ur in u_ur.DefaultIfEmpty()
                join r in _context.Roles on ur.RoleId equals r.Id into ur_r
                from r in ur_r.DefaultIfEmpty()
                where u.Id == userId
                select new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    Role = r != null ? r.Name : null,
                    CreatedAt = u.CreatedAt,
                    AvatarUrl = u.AvatarUrl,
                    IsDeleted = u.IsDeleted
                }).FirstOrDefaultAsync();
        }
    }
}

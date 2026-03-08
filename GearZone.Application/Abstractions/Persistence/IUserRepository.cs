using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IUserRepository
    {
        Task<PagedResult<UserDto>> GetUsersAsync(UserQueryDto query);
        Task<UserDto?> GetUserByIdAsync(string userId);
    }
}

using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class StoreUserRepository : Repository<StoreUser, Guid>, IStoreUserRepository
    {
        public StoreUserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

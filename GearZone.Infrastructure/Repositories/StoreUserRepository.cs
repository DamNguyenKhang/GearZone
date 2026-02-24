using GearZone.Domain.Abstractions.Persistence;
using GearZone.Domain.Entities;
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

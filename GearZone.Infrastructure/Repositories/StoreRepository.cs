using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class StoreRepository : Repository<Store, Guid>, IStoreRepository
    {
        public StoreRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

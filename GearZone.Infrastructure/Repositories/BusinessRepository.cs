using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Repositories
{
    public class BusinessRepository : Repository<Business, Guid>, IBusinessRepository
    {
        public BusinessRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

using GearZone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Abstractions.Persistence
{
    public interface IBusinessRepository : IRepository<Business, Guid>
    {
    }
}

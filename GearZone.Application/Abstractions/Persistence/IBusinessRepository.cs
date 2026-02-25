using GearZone.Application.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IBusinessRepository : IRepository<Business, Guid>
    {
    }
}

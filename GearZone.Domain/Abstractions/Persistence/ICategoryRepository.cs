using GearZone.Domain.Entities;
using System;

namespace GearZone.Domain.Abstractions.Persistence
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
    }
}

using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;

namespace GearZone.Infrastructure.Repositories
{
    public class BrandRepository : Repository<Brand, int>, IBrandRepository
    {
        public BrandRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

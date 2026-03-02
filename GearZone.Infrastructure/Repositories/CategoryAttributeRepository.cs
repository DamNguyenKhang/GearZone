using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;

namespace GearZone.Infrastructure.Repositories
{
    public class CategoryAttributeRepository : Repository<CategoryAttribute, int>, ICategoryAttributeRepository
    {
        public CategoryAttributeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

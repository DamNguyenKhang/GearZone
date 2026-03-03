using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review, Guid>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

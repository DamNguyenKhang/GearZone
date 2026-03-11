using GearZone.Application.Abstractions.Persistence;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IPayoutBatchRepository _payoutBatchRepository;
        public IPayoutBatchRepository PayoutBatchRepository => _payoutBatchRepository ??= new PayoutBatchRepository(_dbContext);

        private IPayoutTransactionRepository _payoutTransactionRepository;
        public IPayoutTransactionRepository PayoutTransactionRepository => _payoutTransactionRepository ??= new PayoutTransactionRepository(_dbContext);

        private IPayoutItemRepository _payoutItemRepository;
        public IPayoutItemRepository PayoutItemRepository => _payoutItemRepository ??= new PayoutItemRepository(_dbContext);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _dbContext.SaveChangesAsync(ct);
    }
}

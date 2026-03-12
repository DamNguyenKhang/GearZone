using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin
{
    public class AdminPayoutService : IAdminPayoutService
    {
        private readonly IPayoutBatchRepository _batchRepo;
        private readonly IPayoutTransactionRepository _txRepo;
        private readonly IMapper _mapper;

        public AdminPayoutService(
            IPayoutBatchRepository batchRepo,
            IPayoutTransactionRepository txRepo,
            IMapper mapper)
        {
            _batchRepo = batchRepo;
            _txRepo = txRepo;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdminPayoutTransactionDto>> GetPayoutTransactionsAsync(PayoutTransactionQueryDto query)
        {
            // Returns all transactions across batches (uses existing repo GetByStoreId is per-store)
            // For now, delegate to batch repo but filtered per query
            // Since the repository doesn't have a general paged query, we use the batch level
            throw new NotImplementedException("Use AdminPayoutBatch Detail for transactions.");
        }

        public async Task<PagedResult<AdminPayoutBatchDto>> GetPayoutBatchesAsync(AdminPayoutBatchQueryDto query)
        {
            var pagedBatches = await _batchRepo.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.Status);

            var items = _mapper.Map<List<AdminPayoutBatchDto>>(pagedBatches.Items);
            return new PagedResult<AdminPayoutBatchDto>(items, pagedBatches.TotalCount, pagedBatches.PageNumber, pagedBatches.PageSize);
        }

        public async Task<AdminPayoutBatchDto?> GetPayoutBatchDetailAsync(Guid id)
        {
            var batch = await _batchRepo.GetByIdWithTransactionsAsync(id);
            if (batch == null) return null;

            var dto = _mapper.Map<AdminPayoutBatchDto>(batch);
            dto.Transactions = _mapper.Map<List<AdminPayoutTransactionDto>>(batch.Transactions);
            return dto;
        }
    }
}

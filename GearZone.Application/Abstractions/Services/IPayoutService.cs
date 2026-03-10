using GearZone.Application.Common;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Services;

public interface IPayoutService
{
    /// <summary>
    /// Generates a new Payout Batch aggregating orders up to the specified end date.
    /// Returns the generated PayoutBatch ID or an error result.
    /// </summary>
    Task<Result<Guid>> GenerateWeeklyBatchAsync(DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves and processes a specific Payout Transaction within a Batch.
    /// Initiates the transfer via PayOS.
    /// </summary>
    Task<Result<bool>> ProcessPayoutTransactionAsync(string transactionCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves all eligible transactions in a batch.
    /// </summary>
    Task<Result<bool>> ProcessPayoutBatchAsync(string batchCode, CancellationToken cancellationToken = default);
}

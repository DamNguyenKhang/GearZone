using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/payouts")]
[ApiController]
public class PayoutsController : BaseApiController
{
    private readonly IAdminPayoutService _payoutService;

    public PayoutsController(IAdminPayoutService payoutService)
    {
        _payoutService = payoutService;
    }

    // GET /api/admin/payouts?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PayoutTransactionQueryDto query)
    {
        var transactions = await _payoutService.GetPayoutTransactionsAsync(query);
        var summary = await _payoutService.GetPayoutTransactionSummaryAsync(query);
        return OkResponse(new { transactions, summary });
    }

    // GET /api/admin/payouts/batches?[query]
    [HttpGet("batches")]
    public async Task<IActionResult> Batches([FromQuery] AdminPayoutBatchQueryDto query)
    {
        var batches = await _payoutService.GetPayoutBatchesAsync(query);
        return OkResponse(batches);
    }

    // GET /api/admin/payouts/batches/{id}
    [HttpGet("batches/{id:guid}")]
    public async Task<IActionResult> BatchDetail(Guid id)
    {
        var batch = await _payoutService.GetPayoutBatchDetailAsync(id);
        if (batch == null) return FailResponse("Batch not found.", 404);
        return OkResponse(batch);
    }

    // GET /api/admin/payouts/transactions/{id}
    [HttpGet("transactions/{id:guid}")]
    public async Task<IActionResult> TransactionDetail(Guid id)
    {
        var detail = await _payoutService.GetPayoutTransactionDetailAsync(id);
        if (detail == null) return FailResponse("Transaction not found.", 404);
        return OkResponse(detail);
    }

    // GET /api/admin/payouts/seller-summary
    [HttpGet("seller-summary")]
    public async Task<IActionResult> SellerSummary()
    {
        var start = DateTime.UtcNow.AddMonths(-1);
        var end = DateTime.UtcNow;
        var summary = await _payoutService.GetSellerPayableSummaryAsync(start, end);
        return OkResponse(summary);
    }
}

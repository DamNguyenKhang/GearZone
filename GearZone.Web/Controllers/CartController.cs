using GearZone.Application.Abstractions.Services;
using GearZone.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearZone.Web.Controllers;

[Route("api/cart")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET /api/cart
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cart = await _cartService.GetCartAsync(UserId);
        return Ok(ApiResponse<object>.Ok(cart));
    }

    // POST /api/cart/add
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
    {
        try
        {
            var cartItemId = await _cartService.AddToCartAsync(UserId, request.VariantId, request.Quantity, request.IsBuyNow);
            var cartCount = await _cartService.GetCartItemsCountAsync(UserId);
            return Ok(ApiResponse<object>.Ok(new { cartItemId, cartCount }, "Added to cart."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    // PUT /api/cart/update-quantity
    [HttpPut("update-quantity")]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
    {
        try
        {
            await _cartService.UpdateCartItemQuantityAsync(request.CartItemId, request.Quantity, UserId);
            return Ok(ApiResponse.Ok("Quantity updated."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    // DELETE /api/cart/remove/{cartItemId}
    [HttpDelete("remove/{cartItemId:guid}")]
    public async Task<IActionResult> Remove(Guid cartItemId)
    {
        try
        {
            await _cartService.RemoveCartItemAsync(cartItemId, UserId);
            return Ok(ApiResponse.Ok("Item removed."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}

public class AddToCartRequest
{
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
    public bool IsBuyNow { get; set; }
}

public class UpdateQuantityRequest
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}

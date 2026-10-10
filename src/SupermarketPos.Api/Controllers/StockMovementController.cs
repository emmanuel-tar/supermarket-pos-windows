using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketPos.Api.Data;
using SupermarketPos.Api.Models;

namespace SupermarketPos.Api.Controllers;

[ApiController]
[Route("api/stock-movements")]
[Authorize]
public class StockMovementController : ControllerBase
{
    private readonly AppDbContext _db;

    public StockMovementController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetStockMovements([FromQuery] int? productId = null, [FromQuery] string? movementType = null, [FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var query = _db.StockMovements
            .Include(sm => sm.Product)
            .Where(sm => sm.MovementDate >= startDate)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(sm => sm.ProductId == productId);

        if (!string.IsNullOrEmpty(movementType))
            query = query.Where(sm => sm.MovementType == movementType);

        var movements = await query
            .OrderByDescending(sm => sm.MovementDate)
            .Select(sm => new
            {
                sm.Id,
                sm.Product.Name,
                sm.Product.Sku,
                sm.MovementType,
                sm.Quantity,
                sm.UnitCost,
                sm.MovementAmount,
                sm.ReferenceNo,
                sm.Reason,
                sm.MovementDate
            })
            .ToListAsync();

        return Ok(movements);
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> CreateStockMovement([FromBody] CreateStockMovementRequest request)
    {
        var product = await _db.Products.FindAsync(request.ProductId);
        if (product is null)
            return BadRequest(new { error = "Product not found" });

        var inventory = await _db.InventoryRecords
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId);

        if (inventory is null)
            return BadRequest(new { error = "Inventory record not found" });

        // Validate adjustment based on movement type
        if (request.MovementType == "Damage" || request.MovementType == "Expiry")
        {
            if (inventory.QuantityOnHand < request.Quantity)
                return BadRequest(new { error = "Cannot remove more quantity than available" });
            inventory.QuantityOnHand -= request.Quantity;
        }
        else if (request.MovementType == "Adjustment")
        {
            inventory.QuantityOnHand += request.Quantity;
        }
        else if (request.MovementType == "Transfer")
        {
            if (inventory.QuantityOnHand < request.Quantity)
                return BadRequest(new { error = "Insufficient quantity for transfer" });
            inventory.QuantityOnHand -= request.Quantity;
        }

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            MovementType = request.MovementType,
            Quantity = request.Quantity,
            UnitCost = request.UnitCost ?? product.CostPrice,
            MovementAmount = (request.UnitCost ?? product.CostPrice) * request.Quantity,
            ReferenceNo = request.ReferenceNo,
            Reason = request.Reason,
            UserId = 1, // Placeholder
            Notes = request.Notes
        };

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStockMovements), new { productId = movement.ProductId }, new { movement.Id });
    }
}

public class CreateStockMovementRequest
{
    public int ProductId { get; set; }
    public string MovementType { get; set; } = string.Empty; // Adjustment, Damage, Expiry, Transfer
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

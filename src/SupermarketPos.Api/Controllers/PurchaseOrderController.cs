using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketPos.Api.Data;
using SupermarketPos.Api.Models;
using System.Security.Claims;

namespace SupermarketPos.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrderController : ControllerBase
{
    private readonly AppDbContext _db;

    public PurchaseOrderController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetPurchaseOrders([FromQuery] string? status = null)
    {
        var query = _db.PurchaseOrders.Include(po => po.Supplier).AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(po => po.Status == status);

        var orders = await query
            .OrderByDescending(po => po.OrderDate)
            .Select(po => new
            {
                po.Id,
                po.PoNumber,
                po.Supplier.Name,
                po.Status,
                po.TotalAmount,
                po.OrderDate,
                po.ExpectedDeliveryDate,
                ItemCount = po.Items.Count
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetPurchaseOrder(int id)
    {
        var order = await _db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .ThenInclude(poi => poi.Product)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (order is null)
            return NotFound(new { error = "Purchase order not found" });

        return Ok(new
        {
            order.Id,
            order.PoNumber,
            SupplierName = order.Supplier.Name,
            order.Status,
            order.SubTotal,
            order.TaxAmount,
            order.TotalAmount,
            order.OrderDate,
            order.ExpectedDeliveryDate,
            order.ReceivedDate,
            order.Notes,
            Items = order.Items.Select(i => new
            {
                i.Id,
                i.Product.Name,
                i.Product.Sku,
                i.QuantityOrdered,
                i.QuantityReceived,
                i.UnitCost,
                i.LineTotal
            })
        });
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request)
    {
        var supplier = await _db.Suppliers.FindAsync(request.SupplierId);
        if (supplier is null)
            return BadRequest(new { error = "Supplier not found" });

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest(new { error = "Purchase order must have at least one item" });

        decimal subTotal = 0m;
        var userId = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";

        var po = new PurchaseOrder
        {
            PoNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            SupplierId = request.SupplierId,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes,
            Status = "Draft",
            CreatedByUserId = 1 // Placeholder - get from token in production
        };

        foreach (var item in request.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product is null)
                return BadRequest(new { error = $"Product {item.ProductId} not found" });

            var lineTotal = item.UnitCost * item.QuantityOrdered;
            subTotal += lineTotal;

            po.Items.Add(new PurchaseOrderItem
            {
                ProductId = item.ProductId,
                QuantityOrdered = item.QuantityOrdered,
                UnitCost = item.UnitCost,
                LineTotal = lineTotal,
                Notes = item.Notes
            });
        }

        var tax = subTotal * 0.08m;
        po.SubTotal = subTotal;
        po.TaxAmount = tax;
        po.TotalAmount = subTotal + tax;

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = po.Id }, new { po.Id, po.PoNumber });
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> ApprovePurchaseOrder(int id)
    {
        var order = await _db.PurchaseOrders.FindAsync(id);
        if (order is null)
            return NotFound(new { error = "Purchase order not found" });

        if (order.Status != "Draft")
            return BadRequest(new { error = $"Cannot approve order with status {order.Status}" });

        order.Status = "Approved";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase order approved", status = order.Status });
    }

    [HttpPost("{id}/receive")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> ReceivePurchaseOrder(int id, [FromBody] ReceivePurchaseOrderRequest request)
    {
        var order = await _db.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (order is null)
            return NotFound(new { error = "Purchase order not found" });

        if (order.Status != "Approved")
            return BadRequest(new { error = $"Cannot receive order with status {order.Status}" });

        if (request.ReceivedItems == null || request.ReceivedItems.Count == 0)
            return BadRequest(new { error = "Must specify received items" });

        foreach (var received in request.ReceivedItems)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == received.ItemId);
            if (item is null)
                return BadRequest(new { error = $"Item {received.ItemId} not found in PO" });

            item.QuantityReceived = received.QuantityReceived;

            // Update inventory
            var inventory = await _db.InventoryRecords
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

            if (inventory is null)
            {
                inventory = new InventoryRecord { ProductId = item.ProductId, QuantityOnHand = 0 };
                _db.InventoryRecords.Add(inventory);
            }

            inventory.QuantityOnHand += received.QuantityReceived;

            // Log stock movement
            _db.StockMovements.Add(new StockMovement
            {
                ProductId = item.ProductId,
                MovementType = "Purchase",
                Quantity = received.QuantityReceived,
                UnitCost = item.UnitCost,
                MovementAmount = item.UnitCost * received.QuantityReceived,
                ReferenceNo = order.PoNumber,
                UserId = 1,
                Reason = "Goods received from purchase order"
            });
        }

        order.Status = "Received";
        order.ReceivedDate = DateTime.UtcNow;
        order.ReceivedByUserId = 1;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase order received successfully", status = order.Status });
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> CancelPurchaseOrder(int id)
    {
        var order = await _db.PurchaseOrders.FindAsync(id);
        if (order is null)
            return NotFound(new { error = "Purchase order not found" });

        if (order.Status == "Received" || order.Status == "Cancelled")
            return BadRequest(new { error = $"Cannot cancel order with status {order.Status}" });

        order.Status = "Cancelled";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase order cancelled", status = order.Status });
    }
}

public class CreatePurchaseOrderRequest
{
    public int SupplierId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseOrderItemRequest> Items { get; set; } = new();
}

public class PurchaseOrderItemRequest
{
    public int ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ReceivePurchaseOrderRequest
{
    public List<ReceivedItemRequest> ReceivedItems { get; set; } = new();
}

public class ReceivedItemRequest
{
    public int ItemId { get; set; }
    public int QuantityReceived { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketPos.Api.Data;
using SupermarketPos.Api.Models;

namespace SupermarketPos.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly AppDbContext _db;

    public SupplierController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetSuppliers([FromQuery] bool activeOnly = true)
    {
        var suppliers = await _db.Suppliers
            .Where(s => !activeOnly || s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.ContactPerson,
                s.Phone,
                s.Email,
                s.Address,
                s.City,
                s.State,
                s.CreditLimit,
                s.CurrentBalance,
                CreditAvailable = s.CreditLimit - s.CurrentBalance,
                s.PaymentTerms,
                s.IsActive
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetSupplier(int id)
    {
        var supplier = await _db.Suppliers
            .Include(s => s.PurchaseOrders)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier is null)
            return NotFound(new { error = "Supplier not found" });

        return Ok(new
        {
            supplier.Id,
            supplier.Name,
            supplier.ContactPerson,
            supplier.Phone,
            supplier.Email,
            supplier.Address,
            supplier.City,
            supplier.State,
            supplier.ZipCode,
            supplier.CreditLimit,
            supplier.CurrentBalance,
            CreditAvailable = supplier.CreditLimit - supplier.CurrentBalance,
            supplier.PaymentTerms,
            supplier.IsActive,
            PurchaseOrderCount = supplier.PurchaseOrders.Count
        });
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Name and Email are required" });

        var supplier = new Supplier
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            CreditLimit = request.CreditLimit,
            PaymentTerms = request.PaymentTerms,
            IsActive = true
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, new { supplier.Id, supplier.Name });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound(new { error = "Supplier not found" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            supplier.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Email))
            supplier.Email = request.Email;
        if (!string.IsNullOrWhiteSpace(request.ContactPerson))
            supplier.ContactPerson = request.ContactPerson;
        if (!string.IsNullOrWhiteSpace(request.Phone))
            supplier.Phone = request.Phone;
        if (!string.IsNullOrWhiteSpace(request.Address))
            supplier.Address = request.Address;
        if (!string.IsNullOrWhiteSpace(request.City))
            supplier.City = request.City;
        if (!string.IsNullOrWhiteSpace(request.State))
            supplier.State = request.State;
        if (!string.IsNullOrWhiteSpace(request.ZipCode))
            supplier.ZipCode = request.ZipCode;
        if (request.CreditLimit.HasValue)
            supplier.CreditLimit = request.CreditLimit.Value;
        if (!string.IsNullOrWhiteSpace(request.PaymentTerms))
            supplier.PaymentTerms = request.PaymentTerms;
        if (request.IsActive.HasValue)
            supplier.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Supplier updated successfully" });
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> DeactivateSupplier(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier is null)
            return NotFound(new { error = "Supplier not found" });

        supplier.IsActive = false;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Supplier deactivated" });
    }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; } = 50000m;
    public string PaymentTerms { get; set; } = "Net 30";
}

public class UpdateSupplierRequest
{
    public string? Name { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? PaymentTerms { get; set; }
    public bool? IsActive { get; set; }
}

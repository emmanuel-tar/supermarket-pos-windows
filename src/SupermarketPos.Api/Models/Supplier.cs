namespace SupermarketPos.Api.Models;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PurchaseOrder> PurchaseOrders { get; set; } = new();
}

public class PurchaseOrder
{
    public int Id { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public int? StoreId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Approved, Submitted, Received, Cancelled
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public int? ReceivedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Supplier Supplier { get; set; } = null!;
    public List<PurchaseOrderItem> Items { get; set; } = new();
}

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public int? QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
    public string Notes { get; set; } = string.Empty;

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? StoreId { get; set; }
    public string MovementType { get; set; } = string.Empty; // Purchase, Sale, Return, Adjustment, Transfer, Damage, Expiry
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal MovementAmount { get; set; }
    public string ReferenceNo { get; set; } = string.Empty; // PO Number, Sale Number, etc.
    public string Reason { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}

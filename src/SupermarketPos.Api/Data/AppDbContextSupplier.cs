using Microsoft.EntityFrameworkCore;
using SupermarketPos.Api.Models;

namespace SupermarketPos.Api.Data;

public partial class AppDbContext : DbContext
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    private void ConfigureSupplierModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>().HasKey(s => s.Id);
        modelBuilder.Entity<Supplier>().Property(s => s.Name).IsRequired();
        modelBuilder.Entity<Supplier>().Property(s => s.Email).IsRequired();
        modelBuilder.Entity<Supplier>().Property(s => s.CreditLimit).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Supplier>().Property(s => s.CurrentBalance).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<PurchaseOrder>().HasKey(po => po.Id);
        modelBuilder.Entity<PurchaseOrder>().Property(po => po.PoNumber).IsRequired();
        modelBuilder.Entity<PurchaseOrder>().Property(po => po.SubTotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PurchaseOrder>().Property(po => po.TaxAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PurchaseOrder>().Property(po => po.TotalAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId);

        modelBuilder.Entity<PurchaseOrderItem>().HasKey(poi => poi.Id);
        modelBuilder.Entity<PurchaseOrderItem>().Property(poi => poi.UnitCost).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PurchaseOrderItem>().Property(poi => poi.LineTotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId);
        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.Product)
            .WithMany()
            .HasForeignKey(poi => poi.ProductId);

        modelBuilder.Entity<StockMovement>().HasKey(sm => sm.Id);
        modelBuilder.Entity<StockMovement>().Property(sm => sm.UnitCost).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<StockMovement>().Property(sm => sm.MovementAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<StockMovement>()
            .HasOne(sm => sm.Product)
            .WithMany()
            .HasForeignKey(sm => sm.ProductId);

        // Seed suppliers
        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "Fresh Foods Ltd", ContactPerson = "John Smith", Phone = "+1-800-123-4567", Email = "contact@freshfoods.com", Address = "123 Market St", City = "New York", State = "NY", ZipCode = "10001", CreditLimit = 50000m, CurrentBalance = 12500m, PaymentTerms = "Net 30", IsActive = true },
            new Supplier { Id = 2, Name = "Global Imports Inc", ContactPerson = "Maria Garcia", Phone = "+1-800-567-8901", Email = "sales@globalimports.com", Address = "456 Trade Blvd", City = "Los Angeles", State = "CA", ZipCode = "90001", CreditLimit = 75000m, CurrentBalance = 0m, PaymentTerms = "Net 45", IsActive = true },
            new Supplier { Id = 3, Name = "Regional Bakery Co", ContactPerson = "David Lee", Phone = "+1-800-234-5678", Email = "orders@regionalbakery.com", Address = "789 Flour Lane", City = "Chicago", State = "IL", ZipCode = "60601", CreditLimit = 25000m, CurrentBalance = 5000m, PaymentTerms = "Net 15", IsActive = true }
        );
    }
}

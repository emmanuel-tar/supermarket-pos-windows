using Microsoft.EntityFrameworkCore;
using SupermarketPos.Api.Models;

namespace SupermarketPos.Api.Data;

public partial class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product and Inventory models
        modelBuilder.Entity<Product>().HasKey(p => p.Id);
        modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired();
        modelBuilder.Entity<Product>().Property(p => p.Sku).IsRequired();
        modelBuilder.Entity<Product>().Property(p => p.SellingPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Product>().Property(p => p.CostPrice).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<InventoryRecord>().HasKey(i => i.Id);
        modelBuilder.Entity<InventoryRecord>()
            .HasOne(i => i.Product)
            .WithOne(p => p.Inventory)
            .HasForeignKey<InventoryRecord>(i => i.ProductId);

        // User and Authorization models
        modelBuilder.Entity<UserAccount>().HasKey(u => u.Id);
        modelBuilder.Entity<UserAccount>().HasIndex(u => u.Username).IsUnique();

        // Sales models
        modelBuilder.Entity<Sale>().HasKey(s => s.Id);
        modelBuilder.Entity<SaleItem>().HasKey(s => s.Id);
        modelBuilder.Entity<SaleItem>()
            .HasOne(s => s.Sale)
            .WithMany(s => s.Items)
            .HasForeignKey(s => s.SaleId);

        // Supplier models
        ConfigureSupplierModels(modelBuilder);

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Milk 1L", Sku = "SKU-001", Category = "Dairy", CostPrice = 40m, SellingPrice = 52m, ReorderLevel = 10, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Bread", Sku = "SKU-002", Category = "Bakery", CostPrice = 25m, SellingPrice = 38m, ReorderLevel = 8, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "Rice 5kg", Sku = "SKU-003", Category = "Groceries", CostPrice = 300m, SellingPrice = 420m, ReorderLevel = 15, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "Laundry Detergent", Sku = "SKU-004", Category = "Household", CostPrice = 180m, SellingPrice = 260m, ReorderLevel = 9, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 5, Name = "Sugar 1kg", Sku = "SKU-005", Category = "Groceries", CostPrice = 70m, SellingPrice = 95m, ReorderLevel = 12, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 6, Name = "Soap", Sku = "SKU-006", Category = "Personal Care", CostPrice = 45m, SellingPrice = 68m, ReorderLevel = 20, IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        // Seed Inventory
        modelBuilder.Entity<InventoryRecord>().HasData(
            new InventoryRecord { Id = 1, ProductId = 1, QuantityOnHand = 28 },
            new InventoryRecord { Id = 2, ProductId = 2, QuantityOnHand = 41 },
            new InventoryRecord { Id = 3, ProductId = 3, QuantityOnHand = 16 },
            new InventoryRecord { Id = 4, ProductId = 4, QuantityOnHand = 9 },
            new InventoryRecord { Id = 5, ProductId = 5, QuantityOnHand = 35 },
            new InventoryRecord { Id = 6, ProductId = 6, QuantityOnHand = 24 }
        );

        // Seed Users
        modelBuilder.Entity<UserAccount>().HasData(
            new UserAccount { Id = 1, Username = "cashier", PasswordHash = "cashier123", FullName = "Cashier User", Role = "Cashier", IsActive = true },
            new UserAccount { Id = 2, Username = "manager", PasswordHash = "manager123", FullName = "Store Manager", Role = "Manager", IsActive = true },
            new UserAccount { Id = 3, Username = "admin", PasswordHash = "admin123", FullName = "System Admin", Role = "Admin", IsActive = true }
        );
    }
}

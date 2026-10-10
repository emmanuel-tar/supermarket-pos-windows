# Supermarket POS MVP - Phase 3

A Windows-friendly supermarket point-of-sale MVP for both FOH (front of house) and BOH (back of house) operations.

## Overview
This repository contains a working MVP for a supermarket retail POS system with:

- FOH cashier workflow
- BOH inventory dashboard
- product catalog and stock tracking
- sales checkout simulation
- daily operational summary
- role-based navigation for cashier and manager views
- supplier management
- purchase order workflow
- stock movement tracking

## Tech Stack
- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite database
- JWT authentication
- Static HTML + JavaScript frontend

## MVP Features

### FOH (Front of House)
- search products
- add products to cart
- quantity adjustment
- payment method selection
- sale total calculation
- stock reduction on checkout
- receipt summary output

### BOH (Back of House)
- inventory summary view
- low stock alert list
- dashboard overview values
- sales summary by category
- operational reporting metrics

### Supplier Management (NEW in Phase 3)
- supplier list and details
- supplier credit management
- supplier contact information
- purchase order creation
- purchase order approval workflow
- goods receiving
- stock movement tracking

### Purchase Order Workflow
1. Create PO from supplier
2. Add line items with products and quantities
3. Approve PO (Manager/Admin only)
4. Receive goods and update inventory
5. Cancel if needed
6. Track stock movements for audit

## Project Structure

```text
src/
  SupermarketPos.Api/
    Models/
      Product.cs
      Supplier.cs
      Sale.cs
      UserAccount.cs
    Controllers/
      SupplierController.cs
      PurchaseOrderController.cs
      StockMovementController.cs
    Data/
      AppDbContext.cs
      AppDbContextSupplier.cs
      AppDbContextFinal.cs
    Program.cs
    appsettings.json
    wwwroot/
      index.html
      styles.css
      app.js
```

## Run Locally

Prerequisites:
- .NET 8 SDK installed

Commands:

```bash
dotnet restore
cd src/SupermarketPos.Api
dotnet run
```

Open the browser at:

```text
http://localhost:5000
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login

### Products & Inventory
- `GET /api/products` - List active products
- `GET /api/inventory` - Inventory status
- `GET /api/dashboard` - Dashboard metrics
- `GET /api/reports/summary` - Sales summary

### Sales
- `POST /api/sales/checkout` - Process sale

### Suppliers (NEW)
- `GET /api/suppliers` - List suppliers
- `GET /api/suppliers/{id}` - Supplier details
- `POST /api/suppliers` - Create supplier (Manager/Admin)
- `PUT /api/suppliers/{id}` - Update supplier (Manager/Admin)
- `POST /api/suppliers/{id}/deactivate` - Deactivate supplier (Manager/Admin)

### Purchase Orders (NEW)
- `GET /api/purchase-orders` - List POs
- `GET /api/purchase-orders/{id}` - PO details
- `POST /api/purchase-orders` - Create PO (Manager/Admin)
- `POST /api/purchase-orders/{id}/approve` - Approve PO (Manager/Admin)
- `POST /api/purchase-orders/{id}/receive` - Receive goods (Manager/Admin)
- `POST /api/purchase-orders/{id}/cancel` - Cancel PO (Manager/Admin)

### Stock Movements (NEW)
- `GET /api/stock-movements` - List movements
- `POST /api/stock-movements` - Create adjustment (Manager/Admin)

## Default Demo Data

### Users
- Username: `cashier` / Password: `cashier123` (Cashier role)
- Username: `manager` / Password: `manager123` (Manager role)
- Username: `admin` / Password: `admin123` (Admin role)

### Suppliers
- Fresh Foods Ltd (dairy, produce)
- Global Imports Inc (general goods)
- Regional Bakery Co (bakery items)

### Products
- Milk 1L (Dairy)
- Bread (Bakery)
- Rice 5kg (Groceries)
- Laundry Detergent (Household)
- Sugar 1kg (Groceries)
- Soap (Personal Care)

## Roadmap for Production

### Phase 4: Advanced Features
- Windows desktop UI for cashier terminals using WPF
- Receipt printer integration
- Barcode scanner support
- Cash drawer management
- Card terminal integration
- Customer loyalty program
- Discounts and promotions engine

### Phase 5: Reporting & Analytics
- Advanced sales reports
- Inventory analytics
- Supplier performance reports
- Cash reconciliation
- GST/VAT compliance reports
- Export to PDF/CSV

### Phase 6: Enterprise Features
- SQL Server migration
- Multi-store support
- Role-based approval workflows
- Audit logging
- API rate limiting
- Cloud deployment

## License
MIT

# Supermarket POS MVP

A Windows-friendly supermarket point-of-sale MVP for both FOH (front of house) and BOH (back of house) operations.

## Overview
This repository contains a working MVP for a supermarket retail POS system with:

- FOH cashier workflow
- BOH inventory dashboard
- product catalog and stock tracking
- sales checkout simulation
- daily operational summary
- role-based navigation for cashier and manager views

## Tech Stack
- C# / .NET 8
- ASP.NET Core minimal API
- Static HTML + JavaScript frontend
- In-memory data for rapid MVP testing

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

## Project Structure

```text
src/
  SupermarketPos.Api/
    Program.cs
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

or if the port differs, follow the console output.

## Default Demo Data
The app ships with demo products and inventory values, including:
- milk
- bread
- rice
- detergent
- sugar
- soap

## Notes
This is a working MVP designed for rapid prototyping and demo purposes. It is intentionally simple and uses in-memory data instead of a full database and hardware integrations.

## Roadmap for Production
- SQL Server persistence
- Windows desktop UI for cashier terminals
- receipt printer integration
- barcode scanner support
- cash drawer management
- card terminal integration
- supplier and purchase order flow
- role-based authentication
- reporting export (CSV/PDF)

## License
MIT
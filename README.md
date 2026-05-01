# Retail POS + ERP Sync System

## Overview

Retail POS + ERP Sync System is a prototype application that demonstrates how a retail outlet can operate locally, create sales while offline, update local inventory, and later synchronize sales data with a central ERP system.


## Objectives Covered

- POS sale entry
- Local data storage to simulate offline support
- Basic inventory management
- Sync mechanism from local POS to central ERP
- Data consistency using idempotency
- Retry handling for failed sync records
- Status tracking using Pending, Synced, and Failed

---

## Tech Stack Used

### Backend

- ASP.NET Core Web API (.NET 8)
- Dapper
- SQL Server
- N-Tier / Layered Architecture
- Repository Pattern
- CQS-style Command / Query separation
- FluentValidation
- Dependency Injection

### Frontend

- Blazor Server
- Bootstrap / BlazorBootstrap
- HttpClient-based API integration

### Database

- SQL Server
- Local POS tables
- Central ERP simulation table

---

## Architecture

```text
Blazor UI
   |
   | HTTP API calls
   v
ASP.NET Core Web API
   |
   v
Application Layer
   |
   | Business logic, validation, sync orchestration
   v
Repository Interfaces
   |
   v
Infrastructure Layer
   |
   | Dapper commands and queries
   v
SQL Server Database
```

See `ARCHITECTURE.md` for the Mermaid architecture diagram.

---

## Project Structure

```text
RetailErp.Pos
|
├── RetailErp.Pos.API
│   ├── Controllers
│   ├── Filters
│   ├── Middlewares
│   └── Program.cs
│
├── RetailErp.Pos.Application // Busine
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   ├── Validators
│   └── Common
│
├── RetailErp.Pos.Domain
│   ├── Entities
│   └── Enums
│
├── RetailErp.Pos.Infrastructure
│   └── Data
│       ├── Command
│       ├── Query
│       └── IDbConnectionFactory.cs
│
└── RetailErp.Pos.UI
    ├── Components
    ├── Pages
    └── Services
```

---

## Main Features

### Product Management

- Create product
- View product list
- Maintain stock quantity
- Store barcode and price

### POS Sale Entry

- Select product
- Enter quantity
- Enter unit price
- Enter/simulate barcode
- Generate unique Sale ID
- Generate sale timestamp
- Store sale locally
- Reduce local stock

### Sales Backoffice

- View local sales
- View sync status
- View retry count
- View last sync attempt time

### Sync Sales

- Sync Pending and Failed sales
- Insert successfully synced sales into `CentralSales`
- Prevent duplicate central insert using `SaleId`
- Mark local sale as `Synced`
- Mark failed sale as `Failed`
- Increment `RetryCount` on failure

### Central ERP Sales View

- View successfully synced sales from `CentralSales`
- Confirm that local POS data reached the central ERP simulation

---

## Database Design

### Products

ProductId
Name
Barcode
StockQuantity
Price
CreatedAt


### Sales

Local POS sale table.

SaleId
OutletId
SaleDate
TotalAmount
SyncStatus
RetryCount
LastSyncAttemptAt
CreatedAt


### SaleItems

Sale details table.

SaleItemId
SaleId
ProductId
Barcode
Quantity
UnitPrice
LineTotal


### CentralSales

Central ERP simulation table.

SaleId
OutletId
SaleDate
TotalAmount
ReceivedAt


---

## Sync Flow

1. User creates a sale from the POS UI.
2. Sale is stored locally in Sales and SaleItems.
3. Product stock is reduced locally.
4. Sale is created with SyncStatus = Pending.
5. User clicks Sync Sales.
6. System fetches local sales where SyncStatus is Pending or Failed.
7. For each sale:
   - Check whether SaleId already exists in CentralSales.
   - If not exists, insert into CentralSales.
   - If exists, treat as duplicate and skip insert.
8. If sync succeeds:
   - Mark local sale as Synced.
   - Update LastSyncAttemptAt.
   -Insert synced data into Central Sales
9. If sync fails:
   - Mark local sale as Failed.
   - Increment RetryCount.
   - Update LastSyncAttemptAt.
10. Failed records are picked again during the next sync.
```

---

## Idempotency

Idempotency ensures that the same sale is not inserted into the central ERP more than once.

This project handles idempotency by:

- Using `SaleId` as the unique identifier
- Checking `CentralSales` before insert
- Keeping `SaleId` as the primary key in `CentralSales`

Example:

First sync:
SaleId A inserted into CentralSales

Second sync with same SaleId:
System detects SaleId A already exists
Insert is skipped
Duplicate is counted
Local sale is treated as already synced


---

## Retry Handling

Retry is handled using:

SyncStatus = Failed
RetryCount = RetryCount + 1
LastSyncAttemptAt = current UTC time


The next sync attempt fetches:

SyncStatus IN ('Pending', 'Failed')


So failed sales are retried automatically when the user clicks sync again.

---

## How to Run the Project

### 1. Clone Repository

```bash
git clone <https://github.com/aburayhan98/retail-pos-erp.git>
```

### 2. Open Solution

Open the solution file in Visual Studio:

RetailErp.Pos.sln


### 3. Configure Database Connection

Update `RetailErp.Pos.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=RetailPosDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

If using SQL Server Express:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=RetailPosDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Create Database

Run the SQL scripts for:

- Products
- Sales
- SaleItems
- CentralSales

### 5. Set Multiple Startup Projects

In Visual Studio:

```text
Right-click Solution
→ Properties
→ Startup Project
→ Multiple startup projects
```

Set:


RetailErp.Pos.API  → Start
RetailErp.Pos.UI   → Start

### 6. Run the Project

Start the solution.

Expected applications:


API Swagger: https://localhost:7187:<api-port>/swagger
Blazor UI:  https://localhost:7187:<ui-port>

https://localhost:7211/
https://localhost:7187/

## API Endpoints

### Products

```http
POST /api/products
GET  /api/products
GET  /api/products/{productId}
```

### Sales

```http
POST /api/sales
GET  /api/sales
GET  /api/sales/{saleId}
```

### Sync

```http
POST /sync-sales
```

### Central Sales

```http
GET /api/central-sales
```

---

## UI Pages

```text
/products       → Product create and list
/sales          → POS sale entry
/sales-list     → Sales backoffice
/sync           → Sync sales
/central-sales  → Central ERP sales view
```

---

## Design Decisions

### N-Tier Architecture

N-Tier architecture was selected because it provides clear separation of concerns while keeping the system simple enough for a prototype.

### CQS-style Repository Separation

  Commands and queries were separated for better readability and maintainability.

  SQLite was selected to simulate offline local POS storage and make the project easy to run without external dependencies.
  Dapper was selected for explicit SQL control and lightweight repository implementation.
  SaleId is generated client-side at the POS and used as an idempotency key during central ERP sync.
  Inventory is reduced locally during sale creation to support offline operation.
  Sync is pull-from-local/push-to-central: local POS sends only Pending/Failed sales and updates status after response.
  Duplicate central inserts are prevented by a unique SaleId constraint and explicit existence check.


### Local-first Sale Creation

Sales are saved locally first to simulate offline POS behavior.

### Central ERP Simulation

`CentralSales` is used as a simulated central ERP table.

### Sync Status Only in Local Sales

Sync status belongs to local `Sales` because the local POS needs to know whether each sale is `Pending`, `Synced`, or `Failed`.

`CentralSales` stores only successfully received ERP data.

---

## Limitations

- No real offline device storage such as SQLite in browser/device
- No real background queue or worker service
- Sync is manually triggered by button
- No authentication or authorization
- No advanced conflict resolution
- No distributed transaction between separate physical databases
- NO implementation of Retry with exponential backoff like Polly or scheduled retry
- UI is prototype-level
- No automated deployment pipeline

---


---

## Summary

This project demonstrates a simple POS-to-ERP synchronization prototype with:

- Local sale creation
- Local inventory update
- Offline-style local persistence
- Central ERP synchronization
- Idempotency
- Retry handling
- Sync status tracking
- Blazor UI for product, sale, sync, and central ERP views

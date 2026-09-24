# Lab Consumable Expiry Tracker

## Documentation Report Batch 20

## Documentation Technical Report Batch 20

## Project Overview
**Lab Consumable Expiry Tracker** is a comprehensive inventory management system designed for laboratories to prevent the use of expired or ineligible consumable materials. The system manages inventory at the **Lot** level, prioritizes usage using a deterministic **FEFO** (*First-Expired-First-Out*) algorithm, provides real-time low-stock signals, and records every consumption and disposal transaction atomically with full audit traceability.

## Key Features (MVP)
- **Inventory Management:** Master Item and Lot registration with strict tracking of expiry dates, storage locations, and administrative statuses.
- **FEFO Allocation Engine:** Real-time eligibility evaluation and deterministic lot allocation for laboratory Jobs.
- **Atomic Consumption:** Multi-lot stock deduction in a single, fail-safe transaction to prevent partial commits, silent overwrites, or negative stock.
- **Disposal & Status Control:** Administrative actions to block, quarantine, or release lots, alongside formal disposal logging with mandatory reasons.
- **Dashboard & Alerts:** Real-time visibility into usable stock, low-stock warnings, and expiring/blocked lots.
- **Immutable Audit Trail:** Complete traceability of all consumption and disposal events, preserving historical data even if lots expire or users are deactivated.
- **Role-Based Access Control (RBAC):** Strict segregation of duties enforced on the server.

## Roles
The system enforces a strict Segregation of Duties via two canonical roles:
1. **`Scientist`**: Creates Jobs, requests FEFO allocation previews, and confirms material consumption in the laboratory.
2. **`WarehouseAdmin`**: Manages master Items/Lots, records disposals, handles administrative status changes (Block/Quarantine/Release), and manages user accounts.

## Tech Stack
- **Backend:** ASP.NET Core Web API (.NET 8), Entity Framework Core, ASP.NET Core Identity, JWT Bearer.
- **Frontend:** React (Single Page Application).
- **Database:** PostgreSQL.

## Architecture & Design Principles
This project is built using **Clean Architecture** (Service Layer Pattern) and follows a **Specs-Driven Development** workflow:
- **Domain Purity:** Core business rules (like the FEFO algorithm and inventory invariants) are completely isolated from the database, EF Core, and UI.
- **Atomicity & Concurrency:** Stock mutations (Consumption/Disposal) are guaranteed *all-or-nothing* using a scoped Unit of Work and Optimistic Concurrency Control (`RowVersion`).
- **Traceability:** Every state mutation requires explicit actor identification from the authenticated context and server-side timestamps.
- **Specs-First:** Implementation is strictly driven by the `specs/` directory, mapping directly to the SRS requirements (FR/BR/NFR) and open decisions (D-xx).

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher) and npm/yarn
- [PostgreSQL](https://www.postgresql.org/download/) (v14 or higher)

### 1. Database Setup
1. Start your PostgreSQL server.
2. Create a new database (e.g., `lab_tracker`).
3. Update the connection string in `LabConsumableExpiryTracker/appsettings.json` and `appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=lab_tracker;Username=your_user;Password=your_password"
   }
### 2. Backend Setup
```bash
# Navigate to the backend root directory
cd LabConsumableExpiryTracker.Infrastructure

# Restore NuGet dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the backend API
dotnet run
```
*Note: The application will automatically execute DbInitializer and UserSeeder on startup to provision canonical roles (Scientis, WarehouseAdmin) and default admin accounts.*
### 3. FrontEnd Setup
```bash
# Navigate to the frontend root directory
cd LabConsumableExpiryTracker.Web

# Install Node dependencies
npm install

# (Optional) Configure environment variables for the API base URL
cp .env.example .env 

# Start the Vite/React development server
npm run dev
```

## Services

All application-side business logic is encapsulated in `LabConsumableExpiryTracker.Application/Services/`. Each service implements an interface in `Services/Interfaces/`.

### Service List

| Service | Interface | Responsibility |
|---------|-----------|----------------|
| `AuthService` | `IAuthService` | Login, refresh token, revoke token |
| `ItemService` | `IItemService` | CRUD for master Items; get all / paged |
| `LotService` | `ILotService` | Lot CRUD, FEFO allocation preview, consumption confirmation, summaries |
| `LotSelectionService` | `ILotSelectionService` | Deterministic FEFO lot allocation engine |
| `JobService` | `IJobService` | Job lifecycle: create, start, cancel, complete, update |
| `ConsumptionHistoryService` | `IConsumptionHistoryService` | User consumption history queries |
| `DisposalService` | `IDisposalService` | Create disposal records for lots |
| `AuditService` | `IAuditService` | Immutable audit trail: events, consumptions, disposals |
| `DashboardAlertService` | `IDashboardAlertService` | Low-stock / expiring / expired / blocked alerts |
| `DashboardSummaryService` | `IDashboardSummaryService` | Dashboard summary counts |
| `ExportService` | `IExportService` | CSV export for stock, consumptions, disposals |
| `UserService` | `IUserService` | User profile CRUD, activate/deactivate |

### Service Patterns
- All methods return `ServiceResult<T>` wrapping success/failure, data, and messages.
- `CancellationToken ct` is passed explicitly through every service call.
- Domain rules (FEFO, stock invariants, expiry checks) live inside the services — not in controllers or EF Core.

## Controllers

All API endpoints are defined in `LabConsumableExpiryTracker.API/Controllers/`. Each controller is an ASP.NET Core `ControllerBase` with `[ApiController]` and `[Route]` attributes. Role-based access is enforced via `[Authorize(Roles = ...)]`.

### Controller List

| Controller | Route | Roles | Key Endpoints |
|------------|-------|-------|---------------|
| `AuthController` | `api/auth` | Public | `POST login`, `POST refresh`, `POST revoke` |
| `UserController` | `api/users` | `WarehouseAdmin` (for mutations), all (for `me`) | `GET me`, `PATCH me`, `GET`, `POST`, `PUT {id}`, `PUT {id}/activate`, `PUT {id}/deactivate` |
| `ItemController` | `api/item` | All (read), `WarehouseAdmin` (write) | `GET`, `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`, `GET item/{itemId}/summary`, `GET paged` |
| `LotController` | `api/lot` | `WarehouseAdmin` | `GET`, `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`, `POST {id}/disposals`, `GET summary`, `GET paged` |
| `JobController` | `api/jobs` | `Scientist` | `GET`, `GET {id}`, `POST`, `PUT {id}`, `POST {id}/start`, `POST {id}/cancel`, `POST {id}/complete`, `POST {jobId}/allocations/preview`, `POST {jobId}/consumptions` |
| `ConsumptionHistoryController` | `api/consumptions-history` | `Scientist` | `GET me`, `GET {id}` |
| `DisposalController` | `api/disposals` | All (read) | `GET`, `GET {id}` |
| `AuditReadController` | `api` | All | `GET audit-events`, `GET consumptions`, `GET consumptions/{id}` |
| `DashboardSummaryController` | `api/dashboard` | `Scientist`, `WarehouseAdmin` | `GET summary` |
| `DashboardAlertsController` | `api/dashboard/alerts` | `Scientist`, `WarehouseAdmin` | `GET` (paginated alerts) |
| `ExportController` | `api/exports` | `WarehouseAdmin` | `GET stock`, `GET consumptions`, `GET disposals` |

### Controller Patterns
- Success returns `Ok(response)` where `response` is a `ServiceResult<T>`.
- Failure returns `BadRequest`, `NotFound`, `Unauthorized`, or `Conflict` (409) as appropriate.
- `DashboardAlertsController` returns `PagedResult<DashboardAlertDto>` — each alert carries `ItemName`, `ItemCode`, `LotNumber`, `RemainingQuantity`, `BaseUnit`, `ExpiryDate` for display on the dashboard.
- Export endpoints stream CSV files (`FileContentResult`) with timestamped filenames.

## Project Structure
```text
├── LabConsumableExpiryTracker/                     # Root (.NET 8)
│   ├── LabConsumableExpiryTracker.API/             # Presentation Layer          (Controllers)
│   ├── LabConsumableExpiryTracker.Application/     # Services, Interfaces, DTOs, Validators
│   ├── LabConsumableExpiryTracker.Domain/          # Core Entities, Enums, Value Objects
│   ├── LabConsumableExpiryTracker.Infrastructure/  # EF Core, Identity, JWT, Repositories
│   ├── LabConsumableExpiryTracker.Web/             # React SPA (Vite)
│   ├── LabConsumableExpiryTracker.Tests/           # Unit & Integration Tests
```



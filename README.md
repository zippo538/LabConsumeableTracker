# Lab Consumable Expiry Tracker

## 📌 Project Overview
**Lab Consumable Expiry Tracker** is a comprehensive inventory management system designed for laboratories to prevent the use of expired or ineligible consumable materials. The system manages inventory at the **Lot** level, prioritizes usage using a deterministic **FEFO** (*First-Expired-First-Out*) algorithm, provides real-time low-stock signals, and records every consumption and disposal transaction atomically with full audit traceability.

## ✨ Key Features (MVP)
- **Inventory Management:** Master Item and Lot registration with strict tracking of expiry dates, storage locations, and administrative statuses.
- **FEFO Allocation Engine:** Real-time eligibility evaluation and deterministic lot allocation for laboratory Jobs.
- **Atomic Consumption:** Multi-lot stock deduction in a single, fail-safe transaction to prevent partial commits, silent overwrites, or negative stock.
- **Disposal & Status Control:** Administrative actions to block, quarantine, or release lots, alongside formal disposal logging with mandatory reasons.
- **Dashboard & Alerts:** Real-time visibility into usable stock, low-stock warnings, and expiring/blocked lots.
- **Immutable Audit Trail:** Complete traceability of all consumption and disposal events, preserving historical data even if lots expire or users are deactivated.
- **Role-Based Access Control (RBAC):** Strict segregation of duties enforced on the server.

## 👥 Canonical Roles
The system enforces a strict Segregation of Duties via two canonical roles:
1. **`Scientist`**: Creates Jobs, requests FEFO allocation previews, and confirms material consumption in the laboratory.
2. **`WarehouseAdmin`**: Manages master Items/Lots, records disposals, handles administrative status changes (Block/Quarantine/Release), and manages user accounts.

## 🛠️ Tech Stack
- **Backend:** ASP.NET Core Web API (.NET 8), Entity Framework Core, ASP.NET Core Identity, JWT Bearer.
- **Frontend:** React (Single Page Application).
- **Database:** PostgreSQL.

## 🏗️ Architecture & Design Principles
This project is built using **Clean Architecture** (Service Layer Pattern) and follows a **Specs-Driven Development** workflow:
- **Domain Purity:** Core business rules (like the FEFO algorithm and inventory invariants) are completely isolated from the database, EF Core, and UI.
- **Atomicity & Concurrency:** Stock mutations (Consumption/Disposal) are guaranteed *all-or-nothing* using a scoped Unit of Work and Optimistic Concurrency Control (`RowVersion`).
- **Traceability:** Every state mutation requires explicit actor identification from the authenticated context and server-side timestamps.
- **Specs-First:** Implementation is strictly driven by the `specs/` directory, mapping directly to the SRS requirements (FR/BR/NFR) and open decisions (D-xx).

## 🚀 Getting Started

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
cd LabConsumableExpiryTracker

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
cd LabConsumableExpiryTracker.Frontend

# Install Node dependencies
npm install

# (Optional) Configure environment variables for the API base URL
cp .env.example .env 

# Start the Vite/React development server
npm run dev
```

## 📂 Project Structure
```text
├── LabConsumableExpiryTracker/                     # Backend Root (.NET 8)
│   ├── LabConsumableExpiryTracker.API/             # Presentation Layer(Controllers, Middleware)
│   ├── LabConsumableExpiryTracker.Application/     # Use Cases, DTOs, Validators, Services
│   ├── LabConsumableExpiryTracker.Domain/          # Core Entities, Enums, Interfaces, Invariants
│   └── LabConsumableExpiryTracker.Infrastructure/  # EF Core, Identity, JWT, Repositories
│
├── LabConsumableExpiryTracker.Frontend/            # React SPA (Vite)
│   └── src/
│       ├── app/                                    # Router, shell, layouts
│       ├── features/                               # Feature-based modules (auth, jobs, lots, etc.)
│       └── services/                               # Single HTTP adapter to the API
│
├── LabConsumableExpiryTracker.Tests/               # Unit & Integration Tests
│
└── specs/                                          # Single Source of Truth (Specs-Driven Dev)
    ├── decisions/                                  # Open decisions (D-05, D-08, D-11, etc.)
    ├── F-01-item-lot/                              # Feature specs, plans, tasks, acceptance
    ├── ................
    ├── F-014-react-frontend/
    ├── architecture.md                             # Clean Architecture structure, dependency flow, and naming conventions
    ├── Readme.md                                   # Information about the specs directory
```
## 📜 Specs-Driven Development
The ``specs/`` directory is the single source of truth between the SRS and the code.
1. Spec-first: spec.md is written and reviewed BEFORE code.
2. Decision gating: Tasks depending on open decisions (e.g., ``D-05``, ``D-08``) are marked ``[BLOCKED]``.
3. Layer-aware tasks: Every task in ``tasks.md`` explicitly targets a layer ``([Domain], [Application], [Infrastructure], [API], [Tests])``.
4. Traceability: Every spec maps directly to SRS requirements (``FR-xx``, ``BR-xx``, ``NFR-xx``, ``AC-xx``).



## 🤝 Contributing & CI/CD
This repository uses GitLab CI/CD.
- All merge requests must pass the pipeline (``dotnet build``, ``dotnet test``, ``npm run build``).
- Do not bypass ``TreatWarningsAsErrors``.
- Do not introduce scope creep or alter canonical domain contracts (Item-Lot, FEFO, Roles) without updating the ``specs/decisions/`` and getting explicit Product Owner approval.
- Refer to ``agent.md`` and ``SKILL.md`` for strict AI Agent and developer implementation rules before modifying core domain logic.

## 📄 Documentation References
| Document | Purpose  
|------------------|------------------|
| srs.md | Software Requirements Specification (Baseline for MVP) |
| architecture.md | Clean Architecture structure, dependency flow, and naming conventions |
| agent.md | AI Agent instructions, domain contracts, and reconciliation rules |
| SKILL.md | Implementation rules, FEFO logic, transaction boundaries, and verification |
| specs/README.md | Specs-Driven Development workflow and task conventions |

### 💡 Adjustment Notes:
1. **Folder Structure:** Aligned with `architecture.md` and `agent.md` (separating `API`, `Application`, `Domain`, and `Infrastructure`).
2. **Canonical Roles:** Explicitly defined as `Scientist` and `WarehouseAdmin`, in strict accordance with `SKILL.md` and the `SRS`.
3. **Testing & CI:** Added the `dotnet build -warnaserror` command and emphasized that integration tests must use a **real PostgreSQL instance** (not in-memory)—per `SKILL.md` guidelines—to verify atomicity.
4. **Specs-Driven:** Added a dedicated section referencing the `specs/` folder and the `tasks.md` conventions outlined in `specs/README.md`.
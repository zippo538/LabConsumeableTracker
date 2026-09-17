# Arsitektur Proyek — Service Layer Pattern

## 1. Struktur Solution

LabConsumableExpiryTracker/
├── LabConsumableExpiryTracker.API/ # Presentation Layer (HTTP)
│ ├── Controllers/
│ ├── Middleware/
│ ├── Filters/
│ ├── Program.cs
│ └── appsettings.json
│
├── LabConsumableExpiryTracker.Application/ # Application Layer (Use Cases)
│ ├── DTOs/
│ │ ├── Items/
│ │ ├── Lots/
│ │ ├── Jobs/
│ │ ├── Consumptions/
│ │ ├── Disposals/
│ │ ├── Audit/
│ │ ├── Stock/
│ │ ├── Users/
│ │ └── Auth/
│ ├── Interfaces/ # Kontrak service (untuk DI)
│ │ ├── IItemService.cs
│ │ ├── ILotService.cs
│ │ ├── IJobService.cs
│ │ ├── ILotSelectionService.cs
│ │ ├── IConsumptionService.cs
│ │ ├── IDisposalService.cs
│ │ ├── IAuditService.cs
│ │ ├── IStockService.cs
│ │ ├── IIdentityService.cs
│ │ └── IUserService.cs
│ ├── Services/ # Business logic orchestration
│ │ ├── ItemService.cs
│ │ ├── LotService.cs
│ │ ├── JobService.cs
│ │ ├── LotSelectionService.cs # FEFO algorithm (domain-pure)
│ │ ├── ConsumptionService.cs
│ │ ├── DisposalService.cs
│ │ ├── AuditService.cs
│ │ ├── StockService.cs
│ │ ├── IdentityService.cs
│ │ └── UserService.cs
│ ├── Validators/ # FluentValidation
│ │ ├── Items/
│ │ ├── Lots/
│ │ ├── Jobs/
│ │ ├── Consumptions/
│ │ ├── Disposals/
│ │ ├── Auth/
│ │ └── Users/
│ ├── Mappings/ # AutoMapper profiles
│ │ └── MappingProfile.cs
│ ├── Common/ # Shared application concern
│ │ ├── Interfaces/
│ │ │ ├── ICurrentUserService.cs
│ │ │ ├── IDateTime.cs
│ │ │ └── IExpiryPolicy.cs
│ │ ├── Behaviours/ # MediatR pipeline (jika dipakai)
│ │ ├── Exceptions/
│ │ │ ├── ValidationException.cs
│ │ │ ├── ForbiddenAccessException.cs
│ │ │ └── NotFoundException.cs
│ │ ├── Models/
│ │ │ ├── Result.cs
│ │ │ └── PagedResponse.cs
│ │ └── Mappings/
│ └── DependencyInjection.cs
│
├── LabConsumableExpiryTracker.Domain/ # Domain Layer (Core)
│ ├── Entities/
│ │ ├── Item.cs
│ │ ├── Lot.cs
│ │ ├── Job.cs
│ │ ├── Consumption.cs
│ │ ├── Disposal.cs
│ │ └── User.cs # ApplicationUser
│ ├── Enums/
│ │ ├── UserRole.cs
│ │ ├── LotAdministrativeStatus.cs
│ │ ├── ExpiryCondition.cs
│ │ ├── UnitOfMeasure.cs
│ │ └── JobStatus.cs
│ ├── ValueObjects/ # (opsional, untuk future)
│ ├── Events/ # Domain events (jika dipakai)
│ ├── Exceptions/
│ │ └── DomainException.cs
│ ├── Interfaces/ # Kontrak repository (DI balik)
│ │ ├── IItemRepository.cs
│ │ ├── ILotRepository.cs
│ │ ├── IJobRepository.cs
│ │ ├── IConsumptionRepository.cs
│ │ ├── IDisposalRepository.cs
│ │ ├── IUserRepository.cs
│ │ └── IUnitOfWork.cs
│ └── Constants/
│
├── LabConsumableExpiryTracker.Infrastructure/ # Infrastructure Layer
│ ├── Persistence/
│ │ ├── AppDbContext.cs
│ │ ├── Configurations/ # EF Core fluent config
│ │ │ ├── ItemConfiguration.cs
│ │ │ ├── LotConfiguration.cs
│ │ │ ├── JobConfiguration.cs
│ │ │ ├── ConsumptionConfiguration.cs
│ │ │ ├── DisposalConfiguration.cs
│ │ │ └── UserConfiguration.cs
│ │ ├── Repositories/
│ │ │ ├── ItemRepository.cs
│ │ │ ├── LotRepository.cs
│ │ │ ├── JobRepository.cs
│ │ │ ├── ConsumptionRepository.cs
│ │ │ ├── DisposalRepository.cs
│ │ │ ├── UserRepository.cs
│ │ │ └── UnitOfWork.cs
│ │ ├── Migrations/
│ │ ├── DbInitializer.cs
│ │ └── UserSeeder.cs
│ ├── Identity/
│ │ ├── ApplicationUser.cs
│ │ ├── IdentityService.cs
│ │ └── JwtTokenGenerator.cs
│ ├── Services/
│ │ ├── DateTimeService.cs # IDateTime impl
│ │ └── ExpiryPolicy.cs # IExpiryPolicy impl (D-05)
│ └── DependencyInjection.cs
│
├── LabConsumableExpiryTracker.Tests/ # Test Layer
│ ├── Unit/
│ │ ├── Domain/ # Entity invariant test
│ │ ├── Services/ # Application service test
│ │ └── Validators/ # FluentValidation test
│ ├── Integration/
│ │ ├── Repositories/ # EF Core + PostgreSQL
│ │ ├── Services/ # Service + DB nyata
│ │ └── Api/ # WebApplicationFactory
│ ├── Fixtures/
│ │ └── DatabaseFixture.cs
│ └── Helpers/
│
└── LabConsumableExpiryTracker.Web/ # Frontend (React)
├── src/
│ ├── app/ # Router, shell
│ ├── features/
│ │ ├── auth/
│ │ ├── dashboard/
│ │ ├── items/
│ │ ├── lots/
│ │ ├── jobs/
│ │ ├── consumption/
│ │ ├── disposal/
│ │ ├── audit/
│ │ └── users/
│ ├── components/
│ ├── layouts/
│ ├── services/ # HTTP adapter tunggal (UI-13)
│ ├── hooks/
│ ├── utils/
│ ├── styles/
│ └── tests/
├── package.json
└── vite.config.ts

## 2. Dependency Flow

┌─────────────────────────────────────────────────────────────────┐
│ API (Controllers) │
│ - HTTP binding, authorization, ServiceResult → HTTP response │
└──────────────────────────────┬──────────────────────────────────┘
│ depends on
▼
┌─────────────────────────────────────────────────────────────────┐
│ Application (Services) │
│ - Orkestrasi use case, validasi, mapping DTO, boundary │
│ - Bergantung pada Domain via interface (DI) │
└──────────────────────────────┬──────────────────────────────────┘
│ depends on (interface)
▼
┌─────────────────────────────────────────────────────────────────┐
│ Domain (Core) │
│ - Entity, enum, invariant, interface repository │
│ - TIDAK bergantung pada layer lain │
└──────────────────────────────▲──────────────────────────────────┘
│ implements
│
┌──────────────────────────────┴──────────────────────────────────┐
│ Infrastructure │
│ - EF Core, Identity, JWT, external services │
│ - Implementasi interface dari Domain │
└─────────────────────────────────────────────────────────────────┘

## 3. Konvensi Penamaan

| Layer                 | Konvensi                              | Contoh                               |
| --------------------- | ------------------------------------- | ------------------------------------ |
| Domain Entity         | PascalCase, tanpa suffix              | `Item`, `Lot`, `Job`                 |
| Domain Interface      | Prefiks `I` + nama konsep             | `ILotRepository`, `IUnitOfWork`      |
| Application Service   | Suffix `Service`                      | `ItemService`, `LotSelectionService` |
| Application Interface | Prefiks `I` + nama service            | `IItemService`                       |
| DTO                   | Suffix `Dto` / `Request` / `Response` | `CreateItemRequest`, `ItemResponse`  |
| Validator             | Suffix `Validator`                    | `CreateItemValidator`                |
| Controller            | Suffix `Controller`                   | `ItemsController`                    |
| Repository            | Suffix `Repository`                   | `LotRepository`                      |
| Configuration         | Suffix `Configuration`                | `LotConfiguration`                   |
| Test                  | Suffix `Tests`                        | `LotSelectionServiceTests`           |

## 4. Tanggung Jawab per Layer (SRS §9.1)

### Domain

- Invariant entity (BR-01 s.d. BR-16)
- Validasi internal entity
- Enum dan value object
- Interface repository (kontrak, bukan implementasi)
- **TIDAK** boleh bergantung EF Core, Identity, atau framework eksternal

### Application

- Orkestrasi use case (satu service = satu bounded context fitur)
- Validasi lintas resource (mis. "Lot milik Item yang benar")
- Boundary transaksi (IUnitOfWork)
- Mapping entity ↔ DTO
- Business rules yang melibatkan banyak entity
- **TIDAK** boleh bergantung EF Core langsung (lewat interface repository)

### Infrastructure

- Implementasi repository (EF Core + PostgreSQL)
- Konfigurasi entity (fluent API)
- Migration dan seeder
- Identity, JWT, external service
- Implementasi `IDateTime`, `IExpiryPolicy`

### API

- HTTP binding (routing, model binding)
- Authorization (role-based policy)
- Pemetaan `Result<T>` → HTTP status
- Exception handling global
- **TIDAK** boleh berisi business logic

## 5. Alur Request (Contoh: Consume)

[API] POST /api/jobs/{jobId}/consumptions
↓ ConsumeRequest dto
[API] ConsumeValidator → 400 jika invalid
↓
[API] ConsumptionService.ConsumeAsync(jobId, dto, currentUser)
↓
[Application] ConsumptionService:
a. Validasi Job exists & InProgress
b. Validasi Item exists
c. Load Lot via ILotRepository
d. Cek eligibility via ExpiryPolicy
e. Cek RowVersion
f. Deduksi RemainingQuantity
g. Create Consumption entity
h. IUnitOfWork.SaveChangesAsync()
↓
[Infrastructure] AppDbContext → PostgreSQL
↓
[Application] Map entity → ConsumeResponse dto
↓
[API] Return 201 ConsumeResponse

## 6. Prinsip Kunci

1. **Dependency Inversion**: Domain mendefinisikan interface, Infrastructure mengimplementasi.
2. **Domain Purity**: `LotSelectionService` (FEFO) dapat diuji tanpa DB/UI (NFR-03).
3. **Single Responsibility**: Controller tidak melakukan validasi bisnis; Service tidak melakukan HTTP binding.
4. **Explicit Dependencies**: Semua dependensi di-inject via constructor.
5. **No Leaky Abstraction**: LINQ EF tidak bocor ke Application/Domain.

## 7. Penempatan Task per Layer

Setiap task di `tasks.md` harus secara eksplisit menyebutkan layer target:

```markdown
- [ ] T-XX-YY: [Layer] Deskripsi (referensi FR/BR)
```

Contoh:

- [ ] T-06-01: [Domain] Entity `Consumption` (JobId, LotId, Quantity, ...)
- [ ] T-06-02: [Infrastructure] EF Core config Consumption + FK
- [ ] T-06-03: [Application] `ConsumeCommand` handler dengan validasi lengkap
- [ ] T-06-11: [API] Controller `POST /api/jobs/{jobId}/consumptions`
- [ ] T-06-13: [Tests] Unit test quantity valid

## 8. Referensi SRS

- NFR-01: Stack (ASP.NET Core, EF Core, Identity, JWT, React, PostgreSQL)
- NFR-02: Clean Architecture, Repository Pattern, DI
- NFR-03: Domain murni, LotSelectionService tanpa DB/UI
- NFR-04: AppDbContext, repository, UoW scoped
- NFR-06: AutoMapper, FluentValidation
- §9.1: Tanggung jawab implementasi
- §9.2: Penempatan berkas referensi

---

## 📄 Update: `specs/README.md`

```markdown
# Specs Driven Development — Lab Consumable Expiry Tracker

## 1. Tujuan

Repositori `specs/` merupakan sumber kebenaran tunggal antara SRS dan kode.
Setiap fitur MVP (F-01 s.d. F-14) memiliki folder sendiri yang berisi:

| Berkas          | Isi                                                                                                             |
| --------------- | --------------------------------------------------------------------------------------------------------------- |
| `spec.md`       | Kontrak perilaku: aturan domain, endpoint, DTO, error, invariant.                                               |
| `plan.md`       | Strategi implementasi: lapisan arsitektur, dependensi, urutan, risiko.                                          |
| `tasks.md`      | Checklist task granular **dengan label layer** `[Domain]`/`[Application]`/`[Infrastructure]`/`[API]`/`[Tests]`. |
| `acceptance.md` | Kriteria penerimaan terverifikasi (Given/When/Then).                                                            |

## 2. Arsitektur

Proyek menggunakan **Service Layer Pattern** dalam struktur Clean Architecture.
Lihat [`architecture.md`](./architecture.md) untuk detail struktur solution,
dependency flow, dan konvensi penamaan.

Ringkasan layer:

- **Domain** — Entity, enum, interface repository, invariant
- **Application** — Service (business logic), DTO, validator, mapping
- **Infrastructure** — EF Core, Identity, JWT, implementasi repository
- **API** — Controller, middleware, authorization
- **Tests** — Unit (Domain/Application), Integration (PostgreSQL, API)
- **Web** — React SPA

## 3. Cara Kerja

1. **Spec-first** — `spec.md` ditulis dan direview SEBELUM kode.
2. **Decision gating** — Folder `decisions/` menyimpan keputusan terbuka.
   Task yang bergantung pada keputusan terbuka ditandai `[BLOCKED]`.
3. **Layer-aware tasks** — Setiap task di `tasks.md` mencantumkan layer
   target: `[Domain]`, `[Application]`, `[Infrastructure]`, `[API]`, `[Tests]`.
4. **Acceptance-driven** — `acceptance.md` berisi skenario uji yang harus
   hijau sebelum fitur dinyatakan Done.
5. **Traceability** — Setiap spec/plan/task/acceptance mencantumkan ID SRS.

## 4. Konvensi

- Bahasa: Indonesia (konsisten dengan SRS).
- Prioritas: `Must` / `Should` / `Nice`.
- Status task: `[ ]` / `[~]` / `[x]` / `[BLOCKED]`.
- Role kanonis: `scientis` (huruf kecil) dan `WarehouseAdmin`.
- Semua timestamp UTC; tampilan lokal menyebut zona waktu.
- Setiap task merujuk FR/BR/NFR/UI di SRS.

## 5. Struktur Folder
```

specs/
├── README.md
├── architecture.md ← Anda di sini (arsitektur solution)
├── decisions/
├── F-01-item-lot/
├── F-02-expiry-eligibility/
├── F-03-stock-dashboard/
├── F-04-job-lifecycle/
├── F-05-fefo-allocation/
├── F-06-consumption/
├── F-07-disposal-status/
├── F-08-audit-history/
├── F-11-user-management/
├── F-12-registration/
├── F-13-authentication/
└── F-14-react-frontend/

## 6. Referensi

| Dokumen      | Versi | Lokasi                                       |
| ------------ | ----- | -------------------------------------------- |
| PRD          | v1.4  | `PRD_Lab_Consumable_Expiry_Tracker_v1_4.pdf` |
| SRS          | v1.0  | `srs4.md`                                    |
| Architecture | —     | `specs/architecture.md`                      |

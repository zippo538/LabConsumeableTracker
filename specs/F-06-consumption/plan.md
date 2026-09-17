# F-06 — Plan Implementasi

## Strategi

Consumption adalah command paling kritis. Atomicity dibuktikan di PostgreSQL
nyata (NFR-04). Implementasi mengikuti dependency flow Clean Architecture.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen untuk F-06                                                                                                     |
| ------------------ | ------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | `Entities/Consumption.cs`, `Interfaces/IConsumptionRepository.cs`                                                       |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `Persistence/Configurations/ConsumptionConfiguration.cs`, `Repositories/ConsumptionRepository.cs`                       |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/ConsumptionService.cs`, `Interfaces/IConsumptionService.cs`, `DTOs/Consumptions/`, `Validators/Consumptions/` |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/ConsumptionsController.cs` (atau nested di `JobsController`)                                               |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Unit/Services/ConsumptionServiceTests.cs`, `Integration/Services/ConsumptionIntegrationTests.cs`                       |

## Urutan Implementasi

1. **[Domain]** Entity `Consumption` (JobId, LotId, Quantity, ConsumedAt, ConsumedBy)
2. **[Infrastructure]** `ConsumptionConfiguration` + FK ke Job & Lot
3. **[Infrastructure]** Migration `CreateConsumptions`
4. **[Infrastructure]** `ConsumptionRepository` implementasi
5. **[Application]** DTO: `ConsumeRequest`, `ConsumeResponse`, `ConsumptionDto`, `UpdatedLotDto`
6. **[Application]** `ConsumeValidator` (FluentValidation)
7. **[Application]** Interface `IConsumptionService`
8. **[Application]** `ConsumptionService.ConsumeAsync`:
   - Validasi Job exists & InProgress (D-07)
   - Validasi Item exists
   - Load Lot via `ILotRepository`
   - Cek eligibility via `IExpiryPolicy` (D-05)
   - Cek RowVersion per Lot
   - Deduksi `RemainingQuantity`
   - Create `Consumption` entity
   - `IUnitOfWork.SaveChangesAsync()` → atomik
   - Handle `DbUpdateConcurrencyException` → 409
9. **[Application]** AutoMapper profile untuk Consumption
10. **[API]** Controller endpoint `POST /api/jobs/{jobId}/consumptions`
11. **[API]** Authorization policy: S only
12. **[Tests/Unit]** Unit test quantity valid, deduksi atomik (mock UoW)
13. **[Tests/Unit]** Unit test quantity invalid → tolak
14. **[Tests/Integration]** Integration test multi-Lot rollback pada satu kegagalan (AC-X-02)
15. **[Tests/Integration]** Integration test Lot expired antara preview dan commit (AC-F06-03)
16. **[Tests/Integration]** Integration test dua writer pada Lot sama (NFR-12)
17. **[Web]** Halaman consume (F-14)

## Atomicity Boundary

┌─────────────────────────────────────────────────────────┐
│ ConsumptionService.ConsumeAsync │
│ │
│ 1. Validasi Job + Item │
│ 2. Load Lot (dengan RowVersion) │
│ 3. Cek eligibility + expiry │
│ 4. Cek RowVersion │
│ 5. Deduksi RemainingQuantity │
│ 6. Create Consumption │
│ 7. await _unitOfWork.SaveChangesAsync() ← ATOMIC │
│ │
│ Jika gagal di langkah manapun → rollback total │
└─────────────────────────────────────────────────────────┘

## Risiko

| Risiko                               | Mitigasi                                                                           |
| ------------------------------------ | ---------------------------------------------------------------------------------- |
| Race condition dua consume bersamaan | RowVersion + PostgreSQL isolation level                                            |
| Timeout setelah commit               | FR-06-08: client tidak retry otomatis                                              |
| D-09 belum final                     | Default: satu Item, tolak LotId ganda                                              |
| EF Core leak ke Application          | Review PR: tidak ada `using Microsoft.EntityFrameworkCore` di `ConsumptionService` |

## Referensi

BR-11 s.d. BR-14, FR-06-01 s.d. FR-06-08, §5.3 Boundary transaksi,
NFR-04, `specs/architecture.md`.

# F-05 — Plan Implementasi

## Strategi

FEFO adalah domain service murni (`LotSelectionService`, NFR-03) yang
menerima kandidat Lot dan `now`, lalu mengembalikan alokasi. Tidak ada
side effect. Service ini dapat diuji tanpa DB/UI.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-05                                                                                                       |
| ------------------ | ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | `Services/LotSelectionService.cs`, `Models/LotAllocation.cs` (value object)                                         |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `LotRepository.GetCandidatesAsync(itemId)`                                                                          |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/AllocationService.cs`, `Interfaces/IAllocationService.cs`, `DTOs/Allocations/`, `Validators/Allocations/` |
| **API**            | `LabConsumableExpiryTracker.API`            | Nested di `JobsController` — `POST /api/jobs/{jobId}/allocations/preview`                                           |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Unit/Domain/LotSelectionServiceTests.cs`                                                                           |

## Urutan Implementasi

1. **[Domain]** Value object `LotAllocation` (LotId, Quantity)
2. **[Domain]** `LotSelectionService.Allocate(candidates, requestedQty, now, expiryPolicy)` (BR-09, NFR-03)
3. **[Domain]** Sorting: ExpiryDate asc → ReceivedAt asc → Id asc (FR-05-03)
4. **[Domain]** Alokasi multi-Lot sampai quantity terpenuhi (FR-05-04)
5. **[Domain]** Penolakan jika stock kurang (FR-05-05)
6. **[Infrastructure]** `LotRepository.GetCandidatesAsync(itemId)`
7. **[Application]** DTO: `PreviewFefoRequest`, `PreviewFefoResponse`, `AllocationItemResponse`
8. **[Application]** `PreviewFefoValidator` (FluentValidation)
9. **[Application]** Interface `IAllocationService`
10. **[Application]** `AllocationService.PreviewAsync` — load candidates, call `LotSelectionService`
11. **[Application]** AutoMapper profile
12. **[API]** Endpoint `POST /api/jobs/{jobId}/allocations/preview` (§7.5)
13. **[API]** Authorization policy: S only (§3.1)
14. **[Tests/Unit]** Unit test single Lot sufficient (AC-F05-01)
15. **[Tests/Unit]** Unit test multi Lot, expiry ascending (AC-F05-01)
16. **[Tests/Unit]** Unit test tie expiry, ReceivedAt ascending (AC-F05-02)
17. **[Tests/Unit]** Unit test insufficient stock → reject (AC-F05-03)
18. **[Tests/Integration]** Integration test preview dengan PostgreSQL (NFR-12)
19. **[Web]** Halaman preview (F-14)

## Risiko

| Risiko                          | Mitigasi                                                 |
| ------------------------------- | -------------------------------------------------------- |
| Tie-breaker tidak deterministik | Urutan ExpiryDate → ReceivedAt → Id (BR-09)              |
| Preview dianggap reservasi      | Response `IsReservation: false` + UI explanation (UI-07) |
| D-09 belum final                | Default: satu Item per preview                           |

## Referensi

FR-05-01 s.d. FR-05-07, BR-09/10, §5.2 Algoritme FEFO, NFR-03, `specs/architecture.md`.

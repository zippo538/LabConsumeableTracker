# F-07 — Plan Implementasi

## Strategi

Disposal parsial (Active, quantity < remaining) dapat diimplementasikan
sebelum D-08 final. Transisi status (Block/Quarantine/Release) menunggu D-08.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-07                                                                                               |
| ------------------ | ------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | `Entities/Disposal.cs`, `Interfaces/IDisposalRepository.cs`, invariant disposal                             |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `Persistence/Configurations/DisposalConfiguration.cs`, `Repositories/DisposalRepository.cs`, migration      |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/DisposalService.cs`, `Interfaces/IDisposalService.cs`, `DTOs/Disposals/`, `Validators/Disposals/` |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/LotsController.cs` (nested disposal/status endpoints)                                          |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Unit/Application/DisposalServiceTests.cs`, `Integration/Services/DisposalIntegrationTests.cs`              |

## Urutan Implementasi

1. **[Domain]** Entity `Disposal` (LotId, Quantity, Reason, DisposedAt, DisposedBy) (§6.1)
2. **[Domain]** Interface `IDisposalRepository`
3. **[Infrastructure]** `DisposalConfiguration` EF Core + FK ke Lot
4. **[Infrastructure]** Migration `CreateDisposals`
5. **[Infrastructure]** `DisposalRepository` implementasi
6. **[Application]** DTO: `DisposeRequest`, `DisposeResponse`, `DisposalResponse`, `LotSnapshotResponse`
7. **[Application]** `DisposeValidator` (FluentValidation)
8. **[Application]** Interface `IDisposalService`
9. **[Application]** `DisposalService.DisposeAsync` — validasi + deduksi + create Disposal atomik (FR-07-01 s.d. FR-07-03)
10. **[Application]** Validasi Reason tidak kosong (FR-07-01)
11. **[Application]** Validasi quantity ≤ remaining, Lot bukan Disposed (FR-07-02)
12. **[Application]** Server-set DisposedBy dan DisposedAt (FR-07-03, BR-11)
13. **[Application]** Handle `DbUpdateConcurrencyException` → 409 (BR-13)
14. **[Application]** AutoMapper profile
15. **[API]** Endpoint `POST /api/lots/{lotId}/disposals` (§7.7)
16. **[API]** Authorization policy: W only (§3.1)
17. **[Tests/Unit]** Unit test disposal sukses (AC-F07-01)
18. **[Tests/Integration]** Integration test disposal + concurrency (AC-X-01)
19. **[Web]** Halaman disposal (F-14)

## Risiko

| Risiko                      | Mitigasi                                                    |
| --------------------------- | ----------------------------------------------------------- |
| D-08 belum final            | Implementasi disposal parsial Active dulu                   |
| Reason wajib/tidak per aksi | Default: wajib untuk disposal, opsional untuk status (D-08) |
| Full disposal → Disposed    | Tunggu D-08                                                 |

## [BLOCKED] — Menunggu D-08

- Transisi Block/Quarantine/Release
- Full disposal → Disposed
- Reason per aksi status
- Audit status

## Referensi

FR-07-01 s.d. FR-07-08, BR-16, NFR-04, `specs/architecture.md`.

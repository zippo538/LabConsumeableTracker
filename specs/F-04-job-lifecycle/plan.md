# F-04 — Plan Implementasi

## Strategi

Job lifecycle adalah state machine di domain. Setiap transisi adalah method
pada entity `Job` yang melempar `DomainException` jika state tidak sesuai.
Concurrency ditangani di level persistence (GAP-SRS-06).

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-04                                                                                |
| ------------------ | ------------------------------------------- | -------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | `Entities/Job.cs`, `Enums/JobStatus.cs`, `Interfaces/IJobRepository.cs`, invariant transisi  |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `Persistence/Configurations/JobConfiguration.cs`, `Repositories/JobRepository.cs`, migration |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/JobService.cs`, `Interfaces/IJobService.cs`, `DTOs/Jobs/`, `Validators/Jobs/`      |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/JobsController.cs`                                                              |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Unit/Domain/JobTests.cs`, `Integration/Services/JobServiceTests.cs`                         |

## Urutan Implementasi

1. **[Domain]** Entity `Job` + enum `JobStatus` (Draft, InProgress, Completed, Cancelled) (§6.3)
2. **[Domain]** State machine: `Create()` → Draft (FR-04-02)
3. **[Domain]** Transisi `Start()`: Draft → InProgress, set StartedAt (FR-04-03)
4. **[Domain]** Transisi `Complete()`: InProgress → Completed, set CompletedAt (FR-04-04)
5. **[Domain]** Validasi transisi ilegal → throw `DomainException` (FR-04-05)
6. **[Domain]** Interface `IJobRepository`
7. **[Infrastructure]** `JobConfiguration` EF Core
8. **[Infrastructure]** Migration `CreateJobs`
9. **[Infrastructure]** `JobRepository` implementasi
10. **[Application]** DTO: `CreateJobRequest`, `JobResponse`, `JobDetailResponse`
11. **[Application]** `CreateJobValidator` (FluentValidation)
12. **[Application]** Interface `IJobService`
13. **[Application]** `JobService.CreateAsync`, `StartAsync`, `CompleteAsync`
14. **[Application]** `JobService.ListAsync`, `GetDetailAsync`
15. **[Application]** AutoMapper profile
16. **[API]** `JobsController` — semua endpoint (§7.4)
17. **[API]** Authorization policy: S only (§3.1)
18. **[Tests/Unit]** Unit test state machine (AC-F04-01, AC-F04-02)
19. **[Tests/Integration]** Integration test lifecycle + concurrency (AC-API-02)
20. **[Web]** Halaman Job (F-14)

## Risiko

| Risiko                     | Mitigasi                                                        |
| -------------------------- | --------------------------------------------------------------- |
| D-07 belum final           | Implementasi Draft→InProgress→Completed dulu; Cancel belakangan |
| GAP-SRS-06: race condition | Gunakan optimistic concurrency atau `SELECT FOR UPDATE`         |
| Ownership Job belum final  | Default: semua scientis dapat membaca semua Job (FR-04-01)      |

## Referensi

FR-04-01 s.d. FR-04-06, §5.4 Lifecycle, NFR-04, `specs/architecture.md`.

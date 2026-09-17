# F-04 — Task List

## Domain

- [ ] T-04-01: [Domain] Entity `Job` + enum `JobStatus` (Draft, InProgress, Completed, Cancelled) (§6.3)
- [ ] T-04-02: [Domain] State machine: Create → Draft (FR-04-02)
- [ ] T-04-03: [Domain] Transisi Start: Draft → InProgress, set StartedAt (FR-04-03)
- [ ] T-04-04: [Domain] Transisi Complete: InProgress → Completed, set CompletedAt (FR-04-04)
- [ ] T-04-05: [Domain] Validasi transisi ilegal → throw DomainException (FR-04-05)
- [ ] T-04-06: [Domain] Interface `IJobRepository`

## Infrastructure

- [ ] T-04-07: [Infrastructure] `JobConfiguration` EF Core
- [ ] T-04-08: [Infrastructure] Migration `CreateJobs`
- [ ] T-04-09: [Infrastructure] `JobRepository` implementasi

## Application

- [ ] T-04-10: [Application] DTO `CreateJobRequest`, `JobResponse`, `JobDetailResponse`
- [ ] T-04-11: [Application] `CreateJobValidator` (FluentValidation)
- [ ] T-04-12: [Application] Interface `IJobService`
- [ ] T-04-13: [Application] `JobService.CreateAsync` (FR-04-02)
- [ ] T-04-14: [Application] `JobService.StartAsync` (FR-04-03)
- [ ] T-04-15: [Application] `JobService.CompleteAsync` (FR-04-04)
- [ ] T-04-16: [Application] `JobService.ListAsync` + `GetDetailAsync` (FR-04-01)
- [ ] T-04-17: [Application] AutoMapper profile Job

## API

- [ ] T-04-18: [API] `JobsController` — GET list, GET detail, POST create, POST start, POST complete (§7.4)
- [ ] T-04-19: [API] Authorization policy: S only (§3.1)

## Tests

- [ ] T-04-20: [Tests/Unit] Unit test state machine (AC-F04-01, AC-F04-02)
- [ ] T-04-21: [Tests/Unit] Unit test transisi ilegal (FR-04-05)
- [ ] T-04-22: [Tests/Integration] Integration test lifecycle (AC-API-02)

## [BLOCKED]

- [BLOCKED] T-04-23: [Domain] Implementasi Cancel setelah D-07 final
- [BLOCKED] T-04-24: [Infrastructure] Finalisasi concurrency mechanism setelah GAP-SRS-06

# F-05 — Task List

## Domain

- [ ] T-05-01: [Domain] Value object `LotAllocation` (LotId, Quantity)
- [ ] T-05-02: [Domain] `LotSelectionService.Allocate` (BR-09, NFR-03)
- [ ] T-05-03: [Domain] Sorting: ExpiryDate → ReceivedAt → Id (FR-05-03)
- [ ] T-05-04: [Domain] Alokasi multi-Lot (FR-05-04)
- [ ] T-05-05: [Domain] Penolakan stock kurang (FR-05-05)

## Infrastructure

- [ ] T-05-06: [Infrastructure] `LotRepository.GetCandidatesAsync(itemId)`

## Application

- [ ] T-05-07: [Application] DTO `PreviewFefoRequest`, `PreviewFefoResponse`, `AllocationItemResponse`
- [ ] T-05-08: [Application] `PreviewFefoValidator` (FluentValidation)
- [ ] T-05-09: [Application] Interface `IAllocationService`
- [ ] T-05-10: [Application] `AllocationService.PreviewAsync` (FR-05-01, FR-05-02)
- [ ] T-05-11: [Application] AutoMapper profile

## API

- [ ] T-05-12: [API] Endpoint `POST /api/jobs/{jobId}/allocations/preview` (§7.5)
- [ ] T-05-13: [API] Authorization policy: S only (§3.1)

## Tests

- [ ] T-05-14: [Tests/Unit] Single Lot sufficient (AC-F05-01)
- [ ] T-05-15: [Tests/Unit] Multi Lot, expiry ascending (AC-F05-01)
- [ ] T-05-16: [Tests/Unit] Tie expiry, ReceivedAt ascending (AC-F05-02)
- [ ] T-05-17: [Tests/Unit] Insufficient stock → reject (AC-F05-03)
- [ ] T-05-18: [Tests/Unit] Tie Id sebagai tie-breaker terakhir (BR-09)
- [ ] T-05-19: [Tests/Integration] Preview dengan PostgreSQL (NFR-12)

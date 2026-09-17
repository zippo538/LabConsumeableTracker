# F-06 — Task List

## Domain

- [ ] T-06-01: [Domain] Entity `Consumption` (JobId, LotId, Quantity, ConsumedAt, ConsumedBy) (§6.1)

## Infrastructure

- [ ] T-06-02: [Infrastructure] `ConsumptionConfiguration` + FK ke Job & Lot (NFR-05)
- [ ] T-06-03: [Infrastructure] Migration `CreateConsumptions`

## Application

- [ ] T-06-04: [Application] DTO `ConsumeRequest`, `ConsumeResponse`, `AllocationItemRequest`
- [ ] T-06-05: [Application] `ConsumeValidator` — validasi struktur request
- [ ] T-06-06: [Application] Interface `IConsumptionService`
- [ ] T-06-07: [Application] `ConsumptionService.ConsumeAsync` — orkestrasi lengkap (FR-06-01, FR-06-02)
- [ ] T-06-08: [Application] Validasi quantity positif ≤ remaining (FR-06-03, BR-04)
- [ ] T-06-09: [Application] Validasi total = RequestedQuantity (FR-06-03)
- [ ] T-06-10: [Application] Validasi RowVersion per Lot (BR-13)
- [ ] T-06-11: [Application] Deduksi + Consumption atomik via `IUnitOfWork` (BR-12, FR-06-04)
- [ ] T-06-12: [Application] Server-set ConsumedBy dan ConsumedAt (FR-06-05, BR-11)
- [ ] T-06-13: [Application] Handle `DbUpdateConcurrencyException` → 409 (BR-13, AC-X-01)
- [ ] T-06-14: [Application] Validasi Lot expired saat commit (FR-06-02, AC-F06-03)
- [ ] T-06-15: [Application] AutoMapper profile Consumption

## API

- [ ] T-06-16: [API] Controller `POST /api/jobs/{jobId}/consumptions` (§7.6)
- [ ] T-06-17: [API] Authorization policy: S only (§3.1)

## Tests

- [ ] T-06-18: [Tests/Unit] Unit test quantity valid, deduksi atomik (AC-F06-01)
- [ ] T-06-19: [Tests/Unit] Unit test quantity invalid → tolak (AC-F06-02)
- [ ] T-06-20: [Tests/Integration] Integration test multi-Lot rollback (AC-X-02)
- [ ] T-06-21: [Tests/Integration] Integration test Lot expired antara preview-commit (AC-F06-03)
- [ ] T-06-22: [Tests/Integration] Integration test dua writer pada Lot sama (NFR-12)

## [BLOCKED]

- [BLOCKED] T-06-23: [Application] Finalisasi idempotency/retry setelah D-09

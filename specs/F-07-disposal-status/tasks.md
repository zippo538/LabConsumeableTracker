# F-07 — Task List

## Domain

- [ ] T-07-01: [Domain] Entity `Disposal` (LotId, Quantity, Reason, DisposedAt, DisposedBy) (§6.1)
- [ ] T-07-02: [Domain] Interface `IDisposalRepository`

## Infrastructure

- [ ] T-07-03: [Infrastructure] `DisposalConfiguration` EF Core + FK (NFR-05)
- [ ] T-07-04: [Infrastructure] Migration `CreateDisposals`
- [ ] T-07-05: [Infrastructure] `DisposalRepository` implementasi

## Application

- [ ] T-07-06: [Application] DTO `DisposeRequest`, `DisposeResponse`, `DisposalResponse`, `LotSnapshotResponse`
- [ ] T-07-07: [Application] `DisposeValidator` (FluentValidation)
- [ ] T-07-08: [Application] Interface `IDisposalService`
- [ ] T-07-09: [Application] `DisposalService.DisposeAsync` (FR-07-01, FR-07-02, FR-07-03)
- [ ] T-07-10: [Application] Validasi Reason tidak kosong (FR-07-01)
- [ ] T-07-11: [Application] Validasi quantity ≤ remaining, Lot bukan Disposed (FR-07-02)
- [ ] T-07-12: [Application] Deduksi + Disposal atomik dengan actor/waktu server (FR-07-03, BR-11)
- [ ] T-07-13: [Application] Handle `DbUpdateConcurrencyException` → 409 (BR-13, AC-X-01)
- [ ] T-07-14: [Application] AutoMapper profile

## API

- [ ] T-07-15: [API] Endpoint `POST /api/lots/{lotId}/disposals` (§7.7)
- [ ] T-07-16: [API] Authorization policy: W only (§3.1)

## Tests

- [ ] T-07-17: [Tests/Unit] Unit test disposal sukses (AC-F07-01)
- [ ] T-07-18: [Tests/Unit] Unit test quantity invalid → tolak
- [ ] T-07-19: [Tests/Integration] Integration test disposal + concurrency (AC-X-01)

## [BLOCKED] — Menunggu D-08

- [BLOCKED] T-07-20: [Domain] Implementasi Block/Quarantine/Release (FR-07-04)
- [BLOCKED] T-07-21: [Application] Validasi Lot blocked/quarantined tidak eligible (AC-F07-02)
- [BLOCKED] T-07-22: [Domain] Full disposal → Disposed
- [BLOCKED] T-07-23: [Application] Reason per aksi status
- [BLOCKED] T-07-24: [API] Endpoint block/quarantine/release
- [BLOCKED] T-07-25: [Infrastructure] Audit status (jika diwajibkan D-08)

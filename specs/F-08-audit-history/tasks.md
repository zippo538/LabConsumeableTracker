# F-08 — Task List

## Application

- [ ] T-08-01: [Application] DTO `ConsumptionResponse`, `DisposalResponse`, `AuditEventResponse`
- [ ] T-08-02: [Application] DTO `AuditFilterRequest`
- [ ] T-08-03: [Application] Interface `IAuditService`
- [ ] T-08-04: [Application] `AuditService.GetConsumptionsAsync` (FR-08-01, FR-08-03)
- [ ] T-08-05: [Application] `AuditService.GetDisposalsAsync` (FR-08-01, FR-08-03)
- [ ] T-08-06: [Application] `AuditService.GetAuditEventsAsync` — gabungan (FR-08-01)
- [ ] T-08-07: [Application] Validasi filter: Disposal + JobId → 400 (FR-08-04)
- [ ] T-08-08: [Application] Detail Consumption + Disposal (FR-08-02)
- [ ] T-08-09: [Application] Pastikan histori tetap terbaca saat actor nonaktif (FR-08-06)
- [ ] T-08-10: [Application] AutoMapper profile

## API

- [ ] T-08-11: [API] `AuditController` — GET `/api/audit-events`, `/api/consumptions`, `/api/disposals` (§7.8)
- [ ] T-08-12: [API] Authorization policy: S/W (§3.1)

## Infrastructure

- [ ] T-08-13: [Infrastructure] Index database pada `ConsumedAt`, `DisposedAt`, `ItemId`, `LotId` (NFR-15)

## Tests

- [ ] T-08-14: [Tests/Integration] Integration test filter + pagination (AC-F08-01, AC-API-05)
- [ ] T-08-15: [Tests/Integration] Integration test histori tetap terbaca saat actor nonaktif (FR-08-06)

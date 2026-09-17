# F-02 — Task List

## Domain

- [ ] T-02-01: [Domain] `ExpiryEvaluator` dengan signature `ExpiryCondition Evaluate(Lot, DateTime now, int expiringSoonDays)` (FR-02-02, NFR-03)
- [ ] T-02-02: [Domain] `EligibilityChecker.IsEligible(Lot, DateTime now)` (BR-06, FR-02-04)
- [ ] T-02-03: [Domain] Interface `IExpiryPolicy` di `Domain/Interfaces/` (D-05)

## Infrastructure

- [ ] T-02-04: [Infrastructure] `ExpiryPolicy` implementasi `IExpiryPolicy` (D-05)
- [ ] T-02-05: [Infrastructure] `DateTimeService` wrapper `TimeProvider` (FR-02-05)
- [ ] T-02-06: [Infrastructure] Registrasi `TimeProvider.System` dan `IExpiryPolicy` di DI

## Tests

- [ ] T-02-07: [Tests/Unit] Lot sebelum cutoff, di luar warning → Valid (AC-F02-01)
- [ ] T-02-08: [Tests/Unit] Lot dalam warning window, Active, remaining > 0 → ExpiringSoon, eligible (AC-F02-02)
- [ ] T-02-09: [Tests/Unit] Lot pada/setelah cutoff → Expired, tidak eligible (AC-F02-03)
- [ ] T-02-10: [Tests/Unit] ExpiringSoon + Active + remaining > 0 = eligible (FR-02-04)
- [ ] T-02-11: [Tests/Unit] Expired + Active + remaining > 0 = tidak eligible (FR-02-04)
- [ ] T-02-12: [Tests/Unit] ManuallyBlocked + remaining > 0 + Valid = tidak eligible (BR-06)
- [ ] T-02-13: [Tests/Unit] Quarantined + remaining > 0 + Valid = tidak eligible (BR-06)

## Application (integrasi)

- [ ] T-02-14: [Application] Integrasi `ExpiryEvaluator` ke `LotResponse` (FR-03-01)
- [ ] T-02-15: [Application] Integrasi `EligibilityChecker` ke `LotSelectionService` (F-05)
- [ ] T-02-16: [Application] Integrasi `EligibilityChecker` ke `ConsumptionService` (F-06)

## [BLOCKED]

- [BLOCKED] T-02-17: [Infrastructure] Finalisasi cutoff dan warning boundary setelah D-05 (FR-02-06)

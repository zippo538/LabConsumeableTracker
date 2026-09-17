# F-01 — Task List

## Domain (`LabConsumableExpiryTracker.Domain`)

- [ ] T-01-01: [Domain] Entity `Item` dengan atribut kanonis (BR-01, FR-01-01)
- [ ] T-01-02: [Domain] Entity `Lot` dengan atribut kanonis (BR-02, FR-01-04)
- [ ] T-01-03: [Domain] Enum `UnitOfMeasure` (Milliliter, Gram, Unit, Vial) (§6.3)
- [ ] T-01-04: [Domain] Enum `LotAdministrativeStatus` (§6.3)
- [ ] T-01-05: [Domain] Invariant: RemainingQuantity = InitialQuantity saat create (BR-03, FR-01-05)
- [ ] T-01-06: [Domain] Interface `IItemRepository` dan `ILotRepository`
- [ ] T-01-07: [Domain] Interface `IUnitOfWork`

## Infrastructure (`LabConsumableExpiryTracker.Infrastructure`)

- [ ] T-01-08: [Infrastructure] `ItemConfiguration` (precision/scale, uniqueness) (NFR-05)
- [ ] T-01-09: [Infrastructure] `LotConfiguration` + FK ke Item (NFR-05)
- [ ] T-01-10: [Infrastructure] RowVersion concurrency token untuk Lot (NFR-05, D-03)
- [ ] T-01-11: [Infrastructure] Migration `CreateItemsAndLots` (NFR-04)
- [ ] T-01-12: [Infrastructure] `ItemRepository` implementasi
- [ ] T-01-13: [Infrastructure] `LotRepository` implementasi
- [ ] T-01-14: [Infrastructure] `UnitOfWork` dengan `SaveChangesAsync`

## Application (`LabConsumableExpiryTracker.Application`)

- [ ] T-01-15: [Application] DTO `CreateItemRequest`, `ItemResponse`, `UpdateItemRequest`
- [ ] T-01-16: [Application] DTO `RegisterLotRequest`, `LotResponse`, `UpdateLotAdminRequest`
- [ ] T-01-17: [Application] `CreateItemValidator` + `RegisterLotValidator` (FluentValidation)
- [ ] T-01-18: [Application] Interface `IItemService` dan `ILotService`
- [ ] T-01-19: [Application] `ItemService.CreateAsync` — orkestrasi create Item (FR-01-01)
- [ ] T-01-20: [Application] `LotService.RegisterAsync` — server-set RemainingQuantity (FR-01-04, FR-01-05)
- [ ] T-01-21: [Application] `ItemService.ListAsync` — search, filter, pagination (FR-01-03)
- [ ] T-01-22: [Application] `LotService.ListAsync` — filter ItemId, pagination (FR-01-03)
- [ ] T-01-23: [Application] `ItemService.UpdateAsync` — validasi field yang diizinkan (FR-01-02)
- [ ] T-01-24: [Application] `ItemService.DeactivateAsync` (FR-01-02, GAP-SRS-02)
- [ ] T-01-25: [Application] `LotService.UpdateAdminAsync` + RowVersion check (FR-01-07)
- [ ] T-01-26: [Application] AutoMapper `MappingProfile` untuk Item/Lot (NFR-06)

## API (`LabConsumableExpiryTracker.API`)

- [ ] T-01-27: [API] `ItemsController` — POST, GET list, GET detail (GAP-SRS-01)
- [ ] T-01-28: [API] `ItemsController` — PATCH update, PATCH deactivate (GAP-SRS-01)
- [ ] T-01-29: [API] `LotsController` — POST registrasi, GET list, GET detail (GAP-SRS-01)
- [ ] T-01-30: [API] `LotsController` — PATCH update administratif (GAP-SRS-01)
- [ ] T-01-31: [API] Authorization policy: W untuk mutasi, S/W untuk baca (§3.1)
- [ ] T-01-32: [API] Exception filter → pemetaan `Result<T>` ke HTTP status (NFR-09)

## Tests (`LabConsumableExpiryTracker.Tests`)

- [ ] T-01-33: [Tests/Unit] Unit test invariant domain Item/Lot (NFR-12)
- [ ] T-01-34: [Tests/Unit] Unit test validator Item/Lot
- [ ] T-01-35: [Tests/Unit] Unit test ItemService/LotService (mock repository)
- [ ] T-01-36: [Tests/Integration] Integration test create Item + registrasi Lot di PostgreSQL
- [ ] T-01-37: [Tests/Integration] Integration test concurrency RowVersion (AC-X-01)

## [BLOCKED] — Menunggu D-04/D-05

- [BLOCKED] T-01-38: [Domain] Finalisasi tipe ID, precision/scale, uniqueness (D-04)
- [BLOCKED] T-01-39: [Domain] Finalisasi representasi ExpiryDate (D-05)
- [BLOCKED] T-01-40: [Application] Validasi registrasi Lot expired (D-04)

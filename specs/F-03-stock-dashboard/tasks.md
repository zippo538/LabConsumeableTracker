# F-03 — Task List

## Application

- [ ] T-03-01: [Application] DTO `LotStockResponse`, `ItemStockResponse`, `StockItemResponse`
- [ ] T-03-02: [Application] DTO `DashboardSummaryResponse`
- [ ] T-03-03: [Application] Interface `IStockService`
- [ ] T-03-04: [Application] `StockService.GetLotStockAsync` — detail Lot + ExpiryCondition + IsEligible (FR-03-01)
- [ ] T-03-05: [Application] `StockService.GetItemStockAsync` — UsableStock dari Lot eligible (FR-03-02, BR-07)
- [ ] T-03-06: [Application] `StockService.GetStockItemsAsync` — list + search + filter IsLowStock + pagination (FR-03-06)
- [ ] T-03-07: [Application] `StockService.GetDashboardSummaryAsync` — count low stock, expiring, expired, blocked (FR-03-04, FR-03-05)
- [ ] T-03-08: [Application] Hitung IsLowStock per Item (FR-03-03)
- [ ] T-03-09: [Application] Pastikan quantity beda unit tidak dijumlahkan (FR-03-07)
- [ ] T-03-10: [Application] AutoMapper profile untuk stock DTO

## API

- [ ] T-03-11: [API] `StockController` — GET `/api/stock/items`, `/api/items/{id}/stock`, `/api/lots/{id}/stock` (§7.3)
- [ ] T-03-12: [API] `DashboardController` — GET `/api/dashboard/summary` (§7.3)
- [ ] T-03-13: [API] Authorization policy: S/W (§3.1)

## Infrastructure

- [ ] T-03-14: [Infrastructure] Index database pada `ItemId`, `AdministrativeStatus`, `ExpiryDate` (NFR-15)

## Tests

- [ ] T-03-15: [Tests/Integration] Integration test UsableStock multi-Lot (AC-F03-01)
- [ ] T-03-16: [Tests/Integration] Integration test dashboard summary counts (AC-API-01)
- [ ] T-03-17: [Tests/Integration] Integration test quantity beda unit tidak dijumlahkan (FR-03-07)

## [BLOCKED]

- [BLOCKED] T-03-18: [Application] Finalisasi operator low stock setelah D-06
- [BLOCKED] T-03-19: [Application] Finalisasi CountCriteria setelah GAP-SRS-05

# F-03 — Plan Implementasi

## Strategi

Stock adalah read model dihitung on-the-fly dari Lot eligible. Tidak ada
tabel stock terpisah. Dashboard summary adalah agregasi. Semua evaluasi
menggunakan `ExpiryEvaluator` (F-02) dan `EligibilityChecker`.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-03                                                                               |
| ------------------ | ------------------------------------------- | ------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | Tidak ada entity baru; menggunakan `Item` dan `Lot` existing                                |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | Query EF Core untuk agregasi UsableStock, count dashboard                                   |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/StockService.cs`, `Interfaces/IStockService.cs`, `DTOs/Stock/`, `DTOs/Dashboard/` |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/StockController.cs`, `Controllers/DashboardController.cs`                      |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Integration/Services/StockServiceTests.cs`                                                 |

## Urutan Implementasi

1. **[Application]** DTO: `LotStockResponse`, `ItemStockResponse`, `StockItemResponse`, `DashboardSummaryResponse`
2. **[Application]** Interface `IStockService`
3. **[Application]** `StockService.GetLotStockAsync` — detail Lot + ExpiryCondition + IsEligible (FR-03-01)
4. **[Application]** `StockService.GetItemStockAsync` — UsableStock dari Lot eligible (FR-03-02, BR-07)
5. **[Application]** `StockService.GetStockItemsAsync` — list + search + filter IsLowStock + pagination (FR-03-06)
6. **[Application]** `StockService.GetDashboardSummaryAsync` — count low stock, expiring, expired, blocked (FR-03-04, FR-03-05)
7. **[Application]** Hitung IsLowStock per Item (FR-03-03) — menunggu D-06
8. **[Application]** Pastikan quantity beda unit tidak dijumlahkan (FR-03-07)
9. **[API]** `StockController` — GET endpoints (§7.3)
10. **[API]** `DashboardController` — GET summary (§7.3)
11. **[API]** Authorization policy: S/W
12. **[Tests/Integration]** Integration test UsableStock multi-Lot (AC-F03-01)
13. **[Tests/Integration]** Integration test dashboard summary counts (AC-API-01)
14. **[Web]** Halaman dashboard (F-14)

## Risiko

| Risiko                                 | Mitigasi                                                         |
| -------------------------------------- | ---------------------------------------------------------------- |
| D-06 belum final                       | Default `<`; swap satu baris jika berubah                        |
| CountCriteria belum final (GAP-SRS-05) | Ekspos CountCriteria di response; default: RemainingQuantity > 0 |
| Performa agregasi                      | Index pada `ItemId`, `AdministrativeStatus`, `ExpiryDate`        |

## Referensi

FR-03-01 s.d. FR-03-07, BR-07, NFR-04, `specs/architecture.md`.

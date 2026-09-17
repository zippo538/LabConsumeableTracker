# F-03 — Stock, Low Stock, dan Dashboard Dasar

## Prioritas: Must | SRS §4.3 | Trace: FR-03-01 s.d. FR-03-07

## 1. Ringkasan

Server menyediakan data stock per Lot dan per Item, menghitung UsableStock
dan IsLowStock, serta menyajikan ringkasan dashboard (low stock count,
expiring/expired/blocked Lot count).

## 2. Aturan Domain

| ID       | Aturan                                                                   |
| -------- | ------------------------------------------------------------------------ |
| BR-07    | UsableStock(Item) = jumlah RemainingQuantity Lot eligible milik Item.    |
| FR-03-02 | Item tanpa Lot eligible → UsableStock = 0.                               |
| FR-03-03 | IsLowStock per Item; operator `<` atau `<=` menunggu D-06.               |
| FR-03-05 | Blocked mencakup Quarantined + ManuallyBlocked; tidak dijumlahkan ganda. |
| FR-03-07 | Quantity beda unit tidak dijumlahkan sebagai satu angka.                 |

## 3. Endpoint (SRS §7.3)

| Method | Route                     | Akses | Query                              | Response                                                                                                                                                       |
| ------ | ------------------------- | ----- | ---------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| GET    | /api/stock/items          | S/W   | Search, IsLowStock, Page, PageSize | ItemId, Code, Name, BaseUnit, UsableStock, MinimumStock, IsLowStock, EvaluatedAt                                                                               |
| GET    | /api/items/{itemId}/stock | S/W   | —                                  | Detail stock Item + ExpiringSoonDays, EvaluatedAt                                                                                                              |
| GET    | /api/lots/{lotId}/stock   | S/W   | —                                  | LotId, ItemId, LotNumber, BaseUnit, InitialQuantity, RemainingQuantity, ExpiryDate, AdministrativeStatus, ExpiryCondition, IsEligible, RowVersion, EvaluatedAt |
| GET    | /api/dashboard/summary    | S/W   | ItemId?                            | LowStockItemCount, ExpiringSoonLotCount, ExpiredLotCount, BlockedLotCount, EvaluatedAt                                                                         |

## 4. Read Model

`UsableStock`, `IsLowStock`, `ExpiryCondition`, `IsEligible`, `EvaluatedAt`
adalah hasil evaluasi server, bukan field request (SRS §6.1).

## 5. Dependensi

- D-05: Expiry evaluation (F-02).
- D-06: Low stock operator.
- CountCriteria dashboard (GAP-SRS-05, PROPOSED).

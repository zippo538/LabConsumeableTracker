# F-08 — Riwayat Consumption dan Disposal (Audit)

## Prioritas: Must | SRS §4.8 | Trace: FR-08-01 s.d. FR-08-07

## 1. Ringkasan

Kedua role dapat membaca seluruh riwayat Consumption dan Disposal melalui
endpoint terpadu dan terpisah. Filter, pagination, dan detail tersedia.
Endpoint bersifat baca saja.

## 2. Aturan

| ID       | Aturan                                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------------------------- |
| FR-08-02 | Consumption: Lot, Item, Job, quantity/unit, waktu, actor. Disposal: Lot, Item, quantity/unit, Reason, waktu, actor. |
| FR-08-04 | EventType=Disposal + JobId → 400. JobId tanpa EventType → Consumption saja.                                         |
| FR-08-05 | Endpoint baca; tidak ada edit/hapus histori.                                                                        |
| FR-08-06 | Histori tetap dapat ditelusuri meski Lot expired atau actor dinonaktifkan.                                          |
| BR-15    | Histori Consumption sah sebelum expiry tetap sah setelah Lot expired.                                               |

## 3. Endpoint (SRS §7.8)

| Method | Route                  | Akses | Filter                                                    |
| ------ | ---------------------- | ----- | --------------------------------------------------------- |
| GET    | /api/audit-events      | S/W   | EventType, ItemId, LotId, JobId, From, To, Page, PageSize |
| GET    | /api/consumptions      | S/W   | JobId, ItemId, LotId, From, To, Page, PageSize            |
| GET    | /api/consumptions/{id} | S/W   | —                                                         |
| GET    | /api/disposals         | S/W   | ItemId, LotId, From, To, Page, PageSize                   |
| GET    | /api/disposals/{id}    | S/W   | —                                                         |

## 4. Error

| HTTP | Code               | Kondisi                                    |
| ---- | ------------------ | ------------------------------------------ |
| 400  | VALIDATION_ERROR   | EventType=Disposal + JobId, filter invalid |
| 404  | RESOURCE_NOT_FOUND | Detail tidak ada                           |

## 5. Dependensi

- D-04: Ordering ID.
- D-08: Perluasan event status Lot.
- D-10: Retention audit.

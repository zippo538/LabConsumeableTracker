# F-07 — Disposal dan Status Administratif Lot

## Prioritas: Must | SRS §4.7 | Trace: FR-07-01 s.d. FR-07-08

## 1. Ringkasan

WarehouseAdmin mencatat disposal (pembuangan) quantity Lot dan mengelola
status administratif (Block, Quarantine, Release). Disposal tidak memiliki
JobId. Perubahan status tidak mengurangi quantity.

## 2. Aturan Domain

| ID       | Aturan                                                                   |
| -------- | ------------------------------------------------------------------------ |
| BR-16    | Disposal tidak memiliki JobId; perubahan administratif ≠ Disposal palsu. |
| FR-07-02 | Quantity disposal ≤ remaining; Lot Disposed ditolak.                     |
| FR-07-03 | Deduksi + Disposal atomik; actor/waktu server.                           |
| FR-07-05 | Block/Quarantine/Release tidak mengurangi quantity.                      |
| FR-07-06 | Lot blocked/quarantined dikeluarkan dari FEFO dan Consumption.           |
| FR-07-07 | Release tidak memperpanjang expiry.                                      |

## 3. Endpoint (SRS §7.7)

| Method | Route                        | Akses | Body                                 |
| ------ | ---------------------------- | ----- | ------------------------------------ |
| POST   | /api/lots/{lotId}/disposals  | W     | Quantity, Reason, ExpectedRowVersion |
| POST   | /api/lots/{lotId}/block      | W     | Reason?, ExpectedRowVersion          |
| POST   | /api/lots/{lotId}/quarantine | W     | Reason?, ExpectedRowVersion          |
| POST   | /api/lots/{lotId}/release    | W     | Reason?, ExpectedRowVersion          |

## 4. Disposal Response

```json
{
  "Disposal": {
    "Id": "...",
    "LotId": "...",
    "Quantity": 2,
    "Reason": "...",
    "DisposedAt": "...",
    "DisposedBy": "..."
  },
  "Lot": {
    "RemainingQuantity": 8,
    "AdministrativeStatus": "Active",
    "RowVersion": "<v2>"
  }
}
```

## 5. Error

| HTTP | Code                 | Kondisi                          |
| ---- | -------------------- | -------------------------------- |
| 400  | VALIDATION_ERROR     | Quantity invalid, Reason kosong  |
| 404  | RESOURCE_NOT_FOUND   | Lot tidak ada                    |
| 409  | INSUFFICIENT_STOCK   | Remaining kurang                 |
| 409  | CONCURRENCY_CONFLICT | RowVersion stale                 |
| 409  | INVALID_LOT_STATE    | Status tidak mengizinkan operasi |

## 6. Dependensi

D-08 (BLOCKING): Matriks transisi, disposal per status, full disposal, reason.

---

## `specs/F-07-disposal-status/plan.md`

```markdown
# F-07 — Plan Implementasi

## Strategi

Disposal parsial (Active, quantity < remaining) dapat diimplementasikan
sebelum D-08 final. Transisi status (Block/Quarantine/Release) menunggu D-08.

## Urutan

1. Entity `Disposal` + EF config.
2. Command `DisposeCommand` (parsial, Active only).
3. Controller disposal.
4. Transisi status setelah D-08.
5. Full disposal → Disposed setelah D-08.
6. Frontend disposal/status pages.

## Risiko

| Risiko                      | Mitigasi                                                    |
| --------------------------- | ----------------------------------------------------------- |
| D-08 belum final            | Implementasi disposal parsial Active dulu                   |
| Reason wajib/tidak per aksi | Default: wajib untuk disposal, opsional untuk status (D-08) |
```

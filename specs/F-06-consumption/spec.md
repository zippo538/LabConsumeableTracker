# F-06 — Consumption Atomik

## Prioritas: Must | SRS §4.6 | Trace: FR-06-01 s.d. FR-06-08, BR-11 s.d. BR-14

## 1. Ringkasan

Scientis mengonfirmasi konsumsi bahan untuk Job berdasarkan preview FEFO.
Server memvalidasi ulang seluruh kondisi, lalu mendeduksi stok dan membuat
record Consumption dalam satu transaksi atomik.

## 2. Aturan Domain

| ID       | Aturan                                                                 |
| -------- | ---------------------------------------------------------------------- |
| BR-04    | Quantity positif dan ≤ remaining aktual.                               |
| BR-11    | Actor dan timestamp dari context terverifikasi dan waktu server.       |
| BR-12    | Seluruh deduksi dan audit satu command commit/rollback bersama.        |
| BR-13    | ExpectedRowVersion diperiksa atomik; mismatch → conflict.              |
| BR-14    | Server tidak mengganti Lot secara diam-diam pada konflik.              |
| FR-06-05 | ConsumedBy dan ConsumedAt dari server; payload tidak boleh memalsukan. |
| FR-06-06 | Konflik satu Lot → batalkan seluruh command.                           |
| FR-06-08 | Timeout/network failure ≠ bukti gagal commit; tidak retry otomatis.    |

## 3. Endpoint (SRS §7.6)

`POST /api/jobs/{jobId}/consumptions` — akses S

**Request:**

```json
{
  "ItemId": "<item-id>",
  "RequestedQuantity": 7,
  "Allocations": [
    { "LotId": "<lot-1>", "Quantity": 4, "ExpectedRowVersion": "<v1>" },
    { "LotId": "<lot-2>", "Quantity": 3, "ExpectedRowVersion": "<v2>" }
  ]
}
```

**Response 201:**

```json
{
  "JobId": "<job-id>",
  "ItemId": "<item-id>",
  "BaseUnit": "Milliliter",
  "TotalConsumedQuantity": 7,
  "Consumptions": [
    {
      "Id": "<c1>",
      "JobId": "<job>",
      "LotId": "<lot-1>",
      "Quantity": 4,
      "ConsumedAt": "...",
      "ConsumedBy": "..."
    }
  ],
  "UpdatedLots": [
    { "LotId": "<lot-1>", "RemainingQuantity": 0, "RowVersion": "<v2>" }
  ]
}
```

## 4. Boundary Transaksi (SRS §5.3)

Satu command = validasi Job + baca Lot + cek versi/eligibility + deduksi +
buat Consumption → IUnitOfWork.SaveChangesAsync(). All-or-nothing.

## 5. Validasi Server (FR-06-02)

1.  Identitas + izin scientis.
2.  Job exists, status InProgress (D-07).
3.  Item exists.
4.  Setiap Lot: exists, milik Item, eligible, remaining ≥ quantity, RowVersion match.
5.  Total quantity = RequestedQuantity.
6.  Tidak ada LotId ganda (D-09).

## 6. Error

| HTTP | Code                 | Kondisi                                                |
| ---- | -------------------- | ------------------------------------------------------ |
| 400  | VALIDATION_ERROR     | Quantity nonpositif, total tidak cocok, Lot salah Item |
| 404  | RESOURCE_NOT_FOUND   | Job/Item/Lot tidak ada                                 |
| 409  | INSUFFICIENT_STOCK   | Remaining kurang                                       |
| 409  | LOT_EXPIRED          | Lot expired saat commit                                |
| 409  | LOT_NOT_ELIGIBLE     | Lot tidak eligible                                     |
| 409  | CONCURRENCY_CONFLICT | RowVersion stale                                       |
| 409  | INVALID_JOB_STATE    | Job bukan InProgress                                   |

## 7. Dependensi

- D-07: Consume hanya InProgress.
- D-09: Satu Item per command, LotId ganda, idempotency.

---

## `specs/F-06-consumption/plan.md`

```markdown
# F-06 — Plan Implementasi

## Strategi

Consumption adalah command paling kritis. Atomicity dibuktikan di PostgreSQL
nyata (NFR-04). Tidak ada partial success.

## Urutan

1. Command `ConsumeCommand` + handler.
2. Validasi lengkap di handler (bukan di controller).
3. Transaksi: load Lot with RowVersion → deduksi → create Consumption → SaveChanges.
4. Concurrency: EF Core RowVersion check → DbUpdateConcurrencyException → 409.
5. Controller + DTO.
6. Integration test: multi-Lot success, multi-Lot partial fail → rollback.
7. Frontend consume page (F-14).

## Risiko

| Risiko                               | Mitigasi                              |
| ------------------------------------ | ------------------------------------- |
| Race condition dua consume bersamaan | RowVersion + PostgreSQL isolation     |
| Timeout setelah commit               | FR-06-08: client tidak retry otomatis |
| D-09 belum final                     | Default: satu Item, tolak LotId ganda |
```

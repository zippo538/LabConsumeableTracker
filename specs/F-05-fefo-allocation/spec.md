# F-05 — Preview FEFO (First-Expired-First-Out)

## Prioritas: Must | SRS §4.5 | Trace: FR-05-01 s.d. FR-05-07

## 1. Ringkasan

Scientis meminta preview alokasi FEFO untuk Job dan Item tertentu. Server
mengembalikan rekomendasi Lot berdasarkan expiry terdekat tanpa mengurangi
stok atau membuat reservasi.

## 2. Aturan Domain

| ID       | Aturan                                                                     |
| -------- | -------------------------------------------------------------------------- |
| BR-09    | Alokasi FEFO deterministik: ExpiryDate asc, ReceivedAt asc, Id asc.        |
| BR-10    | Rekomendasi bukan reservasi; waktu preview ≠ waktu validasi commit.        |
| FR-05-05 | Stock eligible kurang → seluruh permintaan ditolak; tidak ada partial.     |
| FR-05-07 | Preview tidak mengurangi stok, tidak membuat Consumption, tidak reservasi. |

## 3. Algoritme (SRS §5.2)

Validasi akses scientis, Job, Item, quantity > 0, policy Job.
candidates = ILotRepository.GetCandidatesAsync(itemId)
now = TimeProvider.GetUtcNow()
eligible = candidates.Where(l => IsEligible(l, now))
sorted = eligible.OrderBy(l => l.ExpiryDate)
.ThenBy(l => l.ReceivedAt)
.ThenBy(l => l.Id)
remaining_need = requestedQuantity
allocations = []
foreach lot in sorted:
alloc = min(lot.RemainingQuantity, remaining_need)
allocations.Add(lot, alloc)
remaining_need -= alloc
if remaining_need == 0: break
if remaining_need > 0: reject (409 INSUFFICIENT_STOCK)
return allocations + metadata

## 4. Endpoint (SRS §7.5)

`POST /api/jobs/{jobId}/allocations/preview` — akses S

**Request:**

```json
{
  "ItemId": "<item-id>",
  "RequestedQuantity": 7
}
```

**Response 200:**

```json
{
  "JobId": "<job-id>",
  "ItemId": "<item-id>",
  "BaseUnit": "Milliliter",
  "RequestedQuantity": 7,
  "AllocatedQuantity": 7,
  "PreviewedAt": "2026-09-11T08:00:00Z",
  "IsReservation": false,
  "Allocations": [
    {
      "LotId": "<lot-1>",
      "LotNumber": "LOT-01",
      "ExpiryDate": "<D-05>",
      "RemainingQuantity": 4,
      "Quantity": 4,
      "RowVersion": "<v1>"
    }
  ]
}
```

**5. Error**

| HTTP | Code |Kondisi|
|-----------|---------|
| 404 | RESOURCE_NOT_FOUND |Job/Item tidak ada|
| 409 | INSUFFICIENT_STOCK |Stock eligible kurang|
| 409 | INVALID_JOB_STATE |Job tidak memenuhi policy|

**6 Dependensi**
D-04: Ordering ID tie-breaker.
D-05: Expiry evaluation.
D-07: Policy Job untuk preview.
D-09: Scope multi-Item, kepatuhan preview.

---

## `specs/F-05-fefo-allocation/plan.md`

```markdown
# F-05 — Plan Implementasi

## Strategi

FEFO adalah domain service murni (`LotSelectionService`, NFR-03) yang
menerima kandidat Lot dan `now`, lalu mengembalikan alokasi. Tidak ada
side effect.

## Urutan

1. `LotSelectionService.Allocate(candidates, requestedQty, now)`.
2. Unit test: single Lot, multi Lot, tie-breaker, insufficient.
3. Application: `PreviewFefoCommand` — load candidates, call service.
4. Controller + DTO.
5. Frontend preview page (F-14).

## Risiko

| Risiko                          | Mitigasi                                         |
| ------------------------------- | ------------------------------------------------ |
| Tie-breaker tidak deterministik | Urutan ExpiryDate → ReceivedAt → Id (BR-09)      |
| Preview dianggap reservasi      | Response `IsReservation: false` + UI explanation |
```

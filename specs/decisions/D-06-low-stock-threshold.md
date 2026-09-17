# D-06 — Low Stock Threshold Operator

## Status: OPEN / PROPOSED

## Konteks (SRS §4.3, §12.1)

F-03 memerlukan operator pembanding untuk menentukan `IsLowStock`:

- Opsi A: `UsableStock < MinimumStock` (strict less-than).
- Opsi B: `UsableStock <= MinimumStock` (less-than-or-equal).

Nilai `MinimumStock` dikonfigurasi per Item (FR-01-01). `UsableStock` adalah
jumlah `RemainingQuantity` seluruh Lot eligible milik Item (BR-07).

## Dampak

| Skenario        | Opsi A (`<`)    | Opsi B (`<=`) |
| --------------- | --------------- | ------------- |
| Usable=5, Min=5 | Tidak low stock | Low stock     |
| Usable=4, Min=5 | Low stock       | Low stock     |
| Usable=0, Min=0 | Tidak low stock | Low stock     |

## Usulan (PROPOSED)

Gunakan `<` (strict). Jika `MinimumStock = 0`, Item tidak pernah low stock
kecuali kebijakan berubah.

## Gate

- [ ] Product Owner memutuskan operator final.
- [ ] AC-F03-01 diperbarui dengan operator yang disepakati.

## Referensi SRS

FR-03-03, BR-07, AC-F03-01, AC-API-01.

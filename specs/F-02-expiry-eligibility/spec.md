# F-02 — Expiry Engine dan Eligibility

## Prioritas: Must | SRS §4.2 | Trace: FR-02-01 s.d. FR-02-06

## 1. Ringkasan

Server mengevaluasi kondisi expiry setiap Lot pada saat read, alokasi FEFO,
dan konsumsi. ExpiryCondition adalah hasil kalkulasi, bukan field persisten.
Eligibility menggabungkan status administratif, remaining, dan expiry.

## 2. Aturan Domain

| ID       | Aturan                                                                                                                  |
| -------- | ----------------------------------------------------------------------------------------------------------------------- |
| BR-06    | Eligible = Active AND RemainingQuantity > 0 AND belum expired pada waktu keputusan.                                     |
| BR-08    | Expired ≠ AdministrativeStatus. ExpiringSoon tidak berarti tidak eligible.                                              |
| FR-02-02 | Sebelum cutoff & di luar warning → Valid; dalam warning & sebelum cutoff → ExpiringSoon; pada/setelah cutoff → Expired. |
| FR-02-03 | ExpiryCondition = hasil perhitungan, terpisah dari status administratif persisten.                                      |
| FR-02-04 | ExpiringSoon tetap eligible jika Active & remaining > 0. Expired selalu tidak eligible.                                 |
| FR-02-05 | Waktu dari TimeProvider; domain menerima `now`, tidak baca jam sistem langsung.                                         |

## 3. Enum ExpiryCondition (§6.3)

`Valid` | `ExpiringSoon` | `Expired`

## 4. Algoritme Evaluasi

Input: Lot lot, DateTime now, int expiringSoonDays, cutoff (D-05)
Hitung cutoffDate = lot.ExpiryDate + cutoff_rule (D-05)
If now >= cutoffDate → Expired
If now >= cutoffDate - expiringSoonDays → ExpiringSoon
Else → Valid

## 5. Eligibility Check

IsEligible(Lot lot, DateTime now):
return lot.AdministrativeStatus == Active
&& lot.RemainingQuantity > 0
&& EvaluateExpiry(lot, now) != Expired

## 6. Kapan Evaluasi Dipanggil

- Read detail Lot (FR-03-01): sertakan ExpiryCondition, IsEligible, EvaluatedAt.
- Preview FEFO (FR-05-02): filter kandidat eligible.
- Consumption (FR-06-02): validasi ulang eligibility aktual.
- Dashboard (FR-03-04): hitung Lot expiring soon dan expired.

## 7. Dependensi Keputusan

- **D-05** (BLOCKING): Cutoff, timezone, warning window boundary.
- Tanpa D-05, algoritme tidak dapat diimplementasikan secara final.

## 8. Error

Tidak ada error HTTP langsung; evaluasi menghasilkan status yang memengaruhi
endpoint lain (409 LOT_EXPIRED, 409 LOT_NOT_ELIGIBLE).

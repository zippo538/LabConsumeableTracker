# D-05 — Expiry Cutoff, Timezone, dan Warning Window

## Status: OPEN / PROPOSED

## Konteks (SRS §4.2, §5.2, §12.1)

Expiry engine (F-02) memerlukan tiga parameter yang belum final:

1. **Cutoff expiry** — Kapan Lot dianggap Expired?
   - Opsi A: `ExpiryDate` adalah `DateOnly`; cutoff = akhir hari (23:59:59 UTC)
     pada tanggal tersebut.
   - Opsi B: `ExpiryDate` disimpan sebagai `DateTime` UTC instant; cutoff =
     instant tersebut.
   - Opsi C: Cutoff berbasis tanggal lokal laboratorium (mis. WIB = UTC+7).

2. **Timezone untuk tanggal** — Apakah `ExpiryDate` pada Lot adalah tanggal
   kalender lokal lab atau tanggal UTC?

3. **Warning window** — Berapa hari sebelum cutoff Lot berstatus ExpiringSoon?
   - Usulan PRD: inklusif 24 jam (atau N hari sesuai `ExpiringSoonDays` per Item).
   - Apakah boundary warning window inklusif atau eksklusif?

## Dampak

| Area               | Jika salah                                          |
| ------------------ | --------------------------------------------------- |
| F-02 Expiry engine | Lot valid terblokir atau Lot expired lolos ke FEFO. |
| F-05 Preview FEFO  | Kandidat salah, alokasi tidak akurat.               |
| F-06 Consumption   | Commit berhasil untuk Lot yang seharusnya expired.  |
| AC-F02-01 s.d. 03  | Acceptance tidak dapat diverifikasi.                |
| Unit test          | Boundary test tidak deterministik.                  |

## Usulan (PROPOSED — belum final)

- `ExpiryDate` disimpan sebagai `DateOnly` di database.
- Cutoff = akhir hari UTC pada `ExpiryDate` (yaitu `ExpiryDate + 1 day` 00:00:00 UTC).
- Warning window = `ExpiringSoonDays` hari sebelum cutoff, inklusif.
- Evaluasi menggunakan `TimeProvider.GetUtcNow()` pada application boundary.

## Gate

- [ ] Product Owner / perwakilan lab menyetujui cutoff dan timezone.
- [ ] Engineering mengonfirmasi representasi `DateOnly` di EF Core + PostgreSQL.
- [ ] Boundary test ditulis dan hijau berdasarkan keputusan final.

## Referensi SRS

FR-02-01 s.d. FR-02-06, BR-06, BR-08, BR-09, AC-F02-01 s.d. AC-F02-03,
GAP-SRS-05 (CountCriteria terkait).

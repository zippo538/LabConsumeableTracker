# D-08 — Lot Status Transition, Disposal Policy, dan Audit Status

## Status: OPEN

## Konteks (SRS §4.7, §5.4, §12.1)

F-07 (Disposal, blocking, quarantine, release) memerlukan matriks transisi
yang belum final.

## Matriks Transisi Usulan (PROPOSED)

Active ──Block──────→ ManuallyBlocked
Active ──Quarantine──→ Quarantined
ManuallyBlocked ──Release──→ Active
Quarantined ──Release──→ Active
[status yang diizinkan] ──Full Disposal──→ Disposed
Disposed ──(tidak ada transisi keluar)──→ Disposed

## Pertanyaan Terbuka

| #   | Pertanyaan                                                                           | Dampak              |
| --- | ------------------------------------------------------------------------------------ | ------------------- |
| 1   | Bolehkah disposal pada Lot expired?                                                  | FR-07-02, AC-F07-01 |
| 2   | Bolehkah disposal pada Lot blocked/quarantined?                                      | FR-07-02, FR-07-06  |
| 3   | Apakah full disposal (remaining → 0) otomatis mengubah status ke Disposed?           | FR-07-02, lifecycle |
| 4   | Apakah setiap perubahan status (Block/Quarantine/Release) memerlukan Reason?         | FR-07-04, DTO       |
| 5   | Apakah perubahan status memerlukan audit record terpisah?                            | FR-08, NFR-10       |
| 6   | Apakah Release memperpanjang expiry? (SRS sudah bilang tidak, tapi perlu konfirmasi) | FR-07-07            |

## Dampak pada Task

- Task F-07 yang melibatkan transisi status ditandai `[BLOCKED]` sampai
  matriks final.
- Task disposal (FR-07-01 s.d. FR-07-03) dapat dilanjutkan untuk kasus
  Active → disposal parsial karena tidak bergantung matriks.

## Gate

- [ ] Product Owner / perwakilan lab menyetujui matriks transisi.
- [ ] Kebijakan disposal per status ditetapkan.
- [ ] Kebutuhan audit status diputuskan.

## Referensi SRS

FR-07-01 s.d. FR-07-08, BR-16, AC-F07-01/02, AC-API-04, GAP-SRS-02.

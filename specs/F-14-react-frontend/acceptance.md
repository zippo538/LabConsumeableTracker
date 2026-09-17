# F-14 — Acceptance Criteria

### AC-FE-01 s.d. AC-FE-18

Seluruh acceptance criteria frontend dari SRS §10.3 berlaku. Ringkasan:

| ID       | Skenario                        | Expected                            |
| -------- | ------------------------------- | ----------------------------------- |
| AC-FE-01 | Route terlindungi tanpa login   | Redirect Login                      |
| AC-FE-02 | Role berbeda, menu/route        | UI mengikuti izin                   |
| AC-FE-03 | Register invalid/duplikat/valid | Error/sukses ke Login               |
| AC-FE-04 | Login/logout flow               | Navigasi tepat, cache bersih        |
| AC-FE-05 | Dashboard loading/gagal/kosong  | State dibedakan                     |
| AC-FE-06 | Unit berbeda                    | Usable stock benar                  |
| AC-FE-07 | Admin/scientis Item/Lot         | Hanya admin mutasi                  |
| AC-FE-08 | Detail Lot                      | Status/expiry/eligibility jelas     |
| AC-FE-09 | Job actions                     | Hanya transisi sah                  |
| AC-FE-10 | Preview multi-Lot               | Urutan, quantity, "bukan reservasi" |
| AC-FE-11 | Submit consume                  | No double submit; bukti server      |
| AC-FE-12 | Conflict consume                | 409, reload/reconfirm               |
| AC-FE-13 | Disposal                        | Validasi + bukti server             |
| AC-FE-14 | Audit filter                    | Consumption→Job, Disposal→Reason    |
| AC-FE-15 | User management                 | Field/izin sesuai F-11              |
| AC-FE-16 | Error handling                  | State UI sesuai §8.3                |
| AC-FE-17 | Viewport + keyboard             | Tidak terpotong, fokus terlihat     |
| AC-FE-18 | Race search                     | Hasil terbaru dipertahankan         |

### Gate Tambahan

- [ ] Semua operasi inti terintegrasi API nyata (FR-14-02).
- [ ] Mock/fixture saja tidak memenuhi acceptance.
- [ ] Responsivitas dan aksesibilitas diuji pada 3 viewport.

# F-14 — Frontend React

## Prioritas: Must (FE-12 Should) | SRS §4.14, §8 | Trace: FR-14-01 s.d. FR-14-04, UI-01 s.d. UI-23

## 1. Ringkasan

React SPA menyediakan seluruh halaman sesuai role. Semua operasi inti
terintegrasi API (bukan mock). Mengikuti aturan state, aksesibilitas,
dan responsivitas.

## 2. Pemetaan Halaman (SRS §8.1)

| Route                      | Akses         | Fungsi            |
| -------------------------- | ------------- | ----------------- |
| /register, /login          | Publik        | Auth              |
| /app                       | Aktif         | Shell             |
| /app/dashboard             | S/W           | Dashboard         |
| /app/items, /app/items/:id | S/W; mutasi W | Item              |
| /app/lots, /app/lots/:id   | S/W; mutasi W | Lot               |
| /app/jobs, /app/jobs/:id   | S             | Job               |
| /app/jobs/:id/consume      | S             | FEFO + Consume    |
| /app/lots/:id/disposal     | W             | Disposal + Status |
| /app/audit                 | S/W           | Audit             |
| /app/profile               | Aktif         | Profil            |
| /app/users, /app/users/:id | W             | User Management   |

## 3. Aturan UI (SRS §8.2–8.5)

| ID    | Aturan                                                                  |
| ----- | ----------------------------------------------------------------------- |
| UI-01 | Register: tanpa pemilih role.                                           |
| UI-05 | Quantity tidak dibulatkan diam-diam; expiry tidak dikonversi diam-diam. |
| UI-07 | Preview menjelaskan stok belum direservasi.                             |
| UI-08 | Submit pending tidak dapat digandakan.                                  |
| UI-09 | Setelah commit, tampilkan bukti server; pengurangan stok lokal ≠ bukti. |
| UI-10 | Konflik → reload/reconfirm; tidak retry otomatis.                       |
| UI-13 | Satu adapter HTTP; data dari server, bukan kalkulasi client.            |
| UI-18 | Viewport 360/768/1280 px.                                               |
| UI-20 | Setiap input memiliki label; error terhubung field.                     |
| UI-21 | Keyboard accessible, fokus terlihat.                                    |

## 4. State Management (SRS §8.3–8.4)

| State       | Perilaku                                             |
| ----------- | ---------------------------------------------------- |
| Loading     | Spinner/skeleton; angka belum tampil sebagai nol.    |
| Pending     | Status proses + pencegahan double submit.            |
| Success     | Bukti server + refresh data.                         |
| Empty       | Bedakan belum ada data vs filter kosong.             |
| 401         | Bersihkan sesi → Login.                              |
| 409         | Reload/reconfirm berdasarkan Code.                   |
| 500/network | Tampilkan kegagalan; retry read boleh, mutasi tidak. |

## 5. Dependensi

- Semua fitur backend (F-01 s.d. F-13) harus memiliki endpoint fungsional.
- D-11: Token storage mechanism.
- Pilihan JS/TS, bundler, router, UI library belum dikunci (SRS §9.2).

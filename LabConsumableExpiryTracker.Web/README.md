# Lab Consumable Expiry Tracker Web

Frontend React + Vite untuk Lab Consumable Expiry Tracker.

## Menjalankan aplikasi

```bash
npm ci
cp .env.example .env
npm run dev
```

Sesuaikan `VITE_API_BASE_URL` di `.env` dengan alamat ASP.NET Core API, termasuk prefix `/api`.

## Cakupan implementasi saat ini

- F-11: profil mandiri dan user management untuk `WarehouseAdmin`.
- F-12: register publik tanpa pemilih role.
- F-13: login, penyimpanan sesi sementara pada `sessionStorage`, logout, dan redirect route terlindungi.
- F-14: app shell responsif, navigasi berbasis role, dashboard melalui API, state loading/error/empty, serta route siap integrasi untuk Item/Lot, Job/Consumption, dan Audit.
- F-15: consumption dua fase — draft dulu, stok baru berkurang setelah tiap lot discan (`verify-scan`).
- F-16: QR label per Lot (`LCET|ItemId|LotId|LotNumber`) dengan cetak satu label atau massal.

Endpoint akun mengikuti SRS: `/auth/register`, `/auth/login`, `/users/me`, dan `/users`.

## Alur barcode lot (F-15 + F-16)

1. **Buat lot** di `Items & Lots` → isi `Lot Number`, quantity, expiry, lokasi.
2. **Cetak label**: di tabel lot centang lot lalu "Print Selected Labels (N)", atau buka detail satu lot lalu "Print Label". Dialog print browser → pilih A4, skala 100%. Tempel label ke kemasan di gudang.
3. **Konsumsi**: buka `Consume`, pilih job + item + quantity, jalankan preview FEFO, tekan "Create Draft". Tidak ada stok yang berkurang pada langkah ini.
4. **Scan** tiap label lot yang benar-benar diambil dari gudang. Tiap scan memverifikasi satu draft dan mengurangi stok lot itu. First-to-Scan Wins: kalau stok sudah habis lebih dulu oleh orang lain, scan ini gagal dan preview FEFO harus diulang.
5. Setelah semua lot terverifikasi, alur selesai.

Scanner boleh mengirim isi QR utuh (`LCET|...`) atau nomor lot polos — keduanya diterima. Payload QR yang rusak ditolak di browser sebelum request dikirim.

## Verifikasi

```bash
npm run lint
npm run build
```

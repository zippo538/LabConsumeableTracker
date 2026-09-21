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

Endpoint akun mengikuti SRS: `/auth/register`, `/auth/login`, `/users/me`, dan `/users`.

## Verifikasi

```bash
npm run lint
npm run build
```

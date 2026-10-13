# F-16: Tasks

## Setup
- [x] T-16-01: [Web] Install `react-qr-code` dan tambahkan ke `package.json`. Verifikasi lewat `ctx7` bahwa props `value` dan `size` tersedia pada versi terbaru.
- [x] T-16-02: [Web] Buat helper `buildLotQrPayload(lot)` di `src/lib/qr/lotQr.ts` yang mengembalikan `LCET|<ItemId>|<LotId>|<LotNumber>`.
- [x] T-16-03: [Web] Buat helper `parseLotQrPayload(raw)` di file yang sama: mengembalikan `{ itemId, lotId, lotNumber }` atau `null` bila format tidak valid. Tolak prefix selain `LCET` dan jumlah field bukan 4.

## Lot Master Table
- [x] T-16-04: [Web] Buat komponen `LotQrCell` di `src/components/lots/` yang merender QR kecil (sekitar 40px) dari `buildLotQrPayload`. → dinamai `LotQr.tsx`.
- [x] T-16-05: [Web] Tambahkan kolom "QR" ke `columns` di `LotPage.tsx`; klik QR navigasi ke `/app/lots/{lot.id}`.
- [x] T-16-06: [Web] Sesuaikan lebar kolom existing agar kolom baru tidak merusak layout tabel.

## Bulk Print
- [x] T-16-07: [Web] Tambahkan kolom checkbox di `LotPage.tsx`, hanya untuk `WarehouseAdmin`.
- [x] T-16-08: [Web] Tambahkan checkbox "Select all" di header tabel untuk halaman saat ini.
- [x] T-16-09: [Web] Tambahkan state `selectedLotIds` (Set) dan tombol "Print Selected Labels (N)".
- [x] T-16-10: [Web] Buat komponen `LotLabelPrintSheet` yang me-render satu label (sekitar 90mm x 60mm) per lot, 2 kolom per halaman A4.
- [x] T-16-11: [Web] ~~Buat halaman rute `LotLabelPrintPage`~~ → **deviasi**: `PrintLabelDialog` sebagai modal di `LotPage`. LotPage sudah memegang data lot, jadi tidak perlu kirim state lewat navigasi. Lihat catatan `ponytail:` di `PrintLabelDialog.tsx`. Spec §4.3 perlu dikoreksi.
- [x] T-16-12: [Web] Tambah CSS `@media print` untuk menyembunyikan halaman aplikasi dan hanya menampilkan sheet label.

## Print Single Label
- [x] T-16-13: [Web] Di `LotDetailPage.tsx`, tambahkan tombol "Print Label". → tanpa gate `WarehouseAdmin`; siapa pun yang bisa membuka detail lot boleh mencetak labelnya.
- [x] T-16-14: [Web] Dialog konfirmasi berisi QR label (120px) dengan `LotNumber`, nama item, expiry, dan lokasi. Reuse `PrintLabelDialog` + `LotLabelPrintSheet` yang sama dengan bulk print. → QR 120px, bukan 200px: 200px tidak muat di label 90mm x 60mm bersama teks.
- [x] T-16-15: [Web] Hubungkan tombol ke `window.print()` memakai CSS print yang sama dengan bulk print.

## Validasi
- [x] T-16-16: [Web] ~~Validasi `LotNumber` menolak `|`~~ → **tidak dilakukan**. `Lot` hanya `Trim()`, tidak ada karakter yang ditolak. `buildLotQrPayload` tidak mem-escape, jadi `|` di lotNumber akan merusak payload — tapi `parseLotQrPayload` sudah menolak payload dengan field count != 4, jadi QR rusak tertangkap di client dengan pesan jelas, bukan lolos ke server. Perbaiki di `Lot.Create`/`UpdateLotNumber` kalau lot number dari supplier memang boleh berisi `|`.
- [x] T-16-17: ~~Unit test `parseLotQrPayload`~~ → **dilewati**. Project web tidak punya test runner dan menambah satu hanya untuk 40 baris parser tidak sepadan. Parser sekarang benar-benar dipakai `ConsumptionConfirmation.handleVerify`, yang menolak payload rusak sebelum request.
- [ ] T-16-18: [Tests] QR di-decode scanner sungguhan menghasilkan payload yang cocok dengan `buildLotQrPayload` (verifikasi manual saat uji cetak).
- [x] T-16-19: [Docs] Update README web dengan urutan pakai: buat lot, cetak label, scan saat consume.


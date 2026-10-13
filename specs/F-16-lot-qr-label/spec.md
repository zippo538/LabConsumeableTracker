# F-16: Lot QR Code & Label Printing

## 1. Ringkasan

F-16 melengkapi F-15 dengan sisi "cetak" dari siklus scan barcode. F-15 mengasumsikan barcode sudah ada di Lot — frontend membaca barcode, backend hanya menerima `ScannedLotNumber`. F-16 menyediakan barcode tersebut: setiap Lot memperoleh QR code yang dapat dicetak dan ditempelkan pada kemasan fisik di penyimpanan.

F-16 murni fitur presentation. **Tidak ada perubahan pada Domain, API, atau skema database.** Semua data yang dibutuhkan sudah ada pada entitas `Lot` saat ini.

## 2. Aturan Domain (BR)

- **BR-22**: Setiap Lot memiliki tepat satu label QR yang deterministik, diturunkan dari `LotId`, `ItemId`, dan `LotNumber`.
- **BR-23**: Isi QR hanya berisi pengenal (identifier), bukan data bisnis sensitif.
- **BR-24**: Label dapat dicetak satu per satu (dari detail Lot) maupun massal (beberapa Lot sekaligus).
- **BR-25**: Format isi QR terdokumentasi dan konsisten, karena frontend F-15 wajib mem-parse-nya saat scan.

## 3. Format Isi QR

Payload mengikuti konvensi majemuk yang sudah disinggung F-15 §5, diperluas dengan `LotId`:

```
LCET|<ItemId>|<LotId>|<LotNumber>
```

Contoh:

```
LCET|7c9e6679-7425-40de-944b-e07fc1f90ae7|1b4e28ba-2fa1-11d2-883f-0016d3cca427|LOT-2026-001
```

**Aturan parsing** (dikerjakan di frontend F-15, tidak berubah di F-16):

| Field | Tipe | Keterangan |
|---|---|---|
| `LCET` | marker | Prefix wajib, membedakan QR LCET dari string biasa. |
| `ItemId` | GUID | Item pemilik Lot. Dikirim sebagai `ItemId` ke `verify-scan`. |
| `LotId` | GUID | Lot yang discan. Memungkinkan lookup langsung tanpa berbasis `LotNumber`. |
| `LotNumber` | string | Nomor lot yang dicetak di kemasan. Dikirim sebagai `ScannedLotNumber`. |

Parser harus menolak payload yang tidak punya 4 bagian atau prefix-nya bukan `LCET`, lalu menampilkan pesan "Barcode tidak valid" — bukan menebak-nebak field.

**Keputusan desain — kenapa `LotId` ikut disertakan:** F-15 `verify-scan` menerima `ItemId` + `ScannedLotNumber` dan mencocokkan draft berdasarkan `LotNumber` (BR-20). `LotId` tidak dikirim ke API pada alur F-15, tetapi menyertakannya di QR membuat label mandiri dan memungkinkan validasi lot sebelum memanggil API, tanpa parsing `LotNumber` yang bisa ambigu antar item.

**Catatan praktis:** bagian `LotNumber` dicetak dalam font monospace agar hasil baca scanner persis sama dengan yang tercetak, tanpa pemotongan karakter.

## 4. Spesifikasi UI

### 4.1. Kolom QR di Tabel Lot (`LotPage`)

Ditambahkan kolom baru "QR" (lebar sekitar 56px) yang menampilkan QR kecil (±40px) per baris, sehingga scientist bisa scan langsung dari daftar tanpa membuka detail.

- QR di-resolve dari `lot.id`, `lot.itemId`, dan `lot.lotNumber` yang sudah tersedia di response `GET /api/lots`.
- 20 baris per halaman menghasilkan 20 QR. Untuk menjaga performa tabel, MVP me-render seluruh baris; jika diukur lambat, tambahkan lazy-load via `IntersectionObserver`.
- Klik QR membuka halaman detail Lot, sama seperti klik pada kolom Code / Name.

### 4.2. Tombol Print Label (`LotDetailPage`)

Ditambahkan tombol "Print Label" pada halaman detail Lot, yang membuka dialog konfirmasi berisi:

```
┌────────────────────────────────────┐
│         ▓▓▓▓▓▓▓▓▓▓▓▓▓               │
│         ▓▓▓▓▓▓▓▓▓▓▓▓▓   ← QR ±120px  │
│         ▓▓▓▓▓▓▓▓▓▓▓▓▓               │
│                                    │
│      LOT-2026-001                  │
│      [ nama item ]                 │
│      Exp: 30 Des 2026              │
│      Loc: Gudang A / Rak 3         │
└────────────────────────────────────┘
```

- Aksi: `window.print()` dengan CSS `@media print` yang menyembunyikan seluruh halaman kecuali label.
- Tidak ada dependency PDF — memakai kemampuan cetak browser.
- Dialog ini adalah komponen yang sama dengan bulk print (§4.3), bukan turunan terpisah.
- Tombol tidak dibatasi role: siapa pun yang bisa membuka detail lot boleh mencetak labelnya. Membatasi hak cetak label tidak menambah keamanan — label hanya berisi pengenal lot yang sudah bisa dibaca siapa pun yang punya akses ke halaman detail.

### 4.3. Bulk Print (`LotPage`)

- Ditambahkan checkbox per baris tabel (khusus role `WarehouseAdmin`).
- Checkbox "Select all" di header tabel.
- Tombol "Print Selected Labels (N)" muncul ketika minimal satu lot dipilih.
- Dialog cetak menampilkan satu label per lot, disusun 2 kolom per halaman A4 (label sekitar 90mm × 60mm).
- Jumlah lot terpilih divalidasi sebelum membuka dialog cetak.

Dialog, bukan route terpisah. `LotPage` sudah memegang data lot, jadi tidak perlu mengirim state lewat navigasi. Kalau nanti dibutuhkan share-link hasil cetak, pindah ke route dengan lot id di query string.

## 5. Model Data

**Tidak ada perubahan.** Semua field yang dibutuhkan sudah ada:

| Sumber | Field |
|---|---|
| `Lot.Id` | `LotId` untuk payload QR |
| `Lot.ItemId` | `ItemId` untuk payload QR |
| `Lot.LotNumber` | `LotNumber` untuk payload QR dan teks label |

Tidak ada kolom baru, tidak ada entitas baru, tidak ada migration.

## 6. Dependensi

- **Library `react-qr-code`**: komponen React yang merender QR sebagai SVG. Dipilih karena ringan, tanpa canvas, dan mendukung sizing langsung lewat prop `size`.
- **F-15 (Scanner Verification)**: QR yang dihasilkan F-16 dibaca oleh flow `verify-scan` F-15. Format payload di §3 adalah kontrak yang dipegang kedua fitur.
- **F-14 (React Frontend)**: mengikuti konvensi halaman dan tabel yang sudah ada (`DataTable`, `AppLayout`).

## 7. Risiko & Mitigasi

- **Risiko**: Format QR berubah di masa depan sehingga label tercetak lama tidak lagi bisa di-parse.
  - **Mitigasi**: prefix `LCET` berfungsi sebagai version marker. Format baru harus menaikkan versi, misalnya `LCET2|...`, sehingga parser dapat menerima keduanya selama masa transisi.
- **Risiko**: `LotNumber` yang dibuat user mengandung karakter `|` sehingga merusak parsing.
  - **Realisasi**: `Lot` hanya memanggil `Trim()`, tidak ada karakter yang ditolak. `buildLotQrPayload` tidak mem-escape `|`.
  - **Mitigasi (saat ini)**: `parseLotQrPayload` menolak payload dengan jumlah field bukan 4, jadi QR rusak tertangkap di client dengan pesan "Barcode tidak valid", bukan lolos ke server. Lot yang lotNumber-nya mengandung `|` tidak bisa discan — perlu diuji sebelum rollout.
  - **Perbaikan yang benar (di luar F-16)**: validasi `LotNumber` di level Domain untuk menolak `|`, atau ubah delimiter ke karakter yang tidak mungkin muncul di nomor lot.
- **Risiko**: QR terlalu kecil di tabel sehingga tidak terbaca scanner.
  - **Mitigasi**: QR di tabel bersifat visual bantu; pemindaian yang authoritative dilakukan dari label tercetak (§4.2) yang jauh lebih besar. Label cetak wajib diuji dengan scanner sungguhan sebelum rollout.
- **Risiko**: 20 QR per halaman memperlambat render tabel.
  - **Mitigasi**: `react-qr-code` menghasilkan SVG murni tanpa permintaan jaringan, sehingga biaya render lokal. Jika masih lambat, lazy-load dengan `IntersectionObserver` (§4.1).
- **Risiko**: F-15 mengirim string hasil scan mentah ke `verify-scan` sebagai `ScannedLotNumber`, sehingga QR F-16 yang berisi 4 field tidak akan pernah cocok dengan lot di server.
  - **Realisasi**: terkonfirmasi selama implementasi F-16. `ConsumptionPage.handleVerifyScan` meneruskan input apa adanya; `LotService.VerifyScan` mencocokkan lewat `GetByItemIdAndLotNumberAsync` dan akan membalas `LOT_NOT_FOUND`.
  - **Mitigasi**: `ConsumptionConfirmation.handleVerify` memanggil `parseLotQrPayload` lebih dulu dan mengirim `parsed.lotNumber`. Payload yang mengandung `|` tapi gagal parse ditolak di client dengan pesan jelas. Scanner yang mengirim nomor lot polos (tanpa prefix) tetap diterima, jadi alur lama tidak rusak.

# F-16: Acceptance Criteria

## AC-F16-01: QR Tampil di Tabel Lot

**Given** `WarehouseAdmin` membuka halaman Lot Master.
**When** daftar lot dimuat.
**Then** setiap baris menampilkan QR code di kolom "QR".
**And** QR tersebut men-decoded oleh scanner menghasilkan payload berbentuk `LCET|<ItemId>|<LotId>|<LotNumber>` yang nilainya cocok dengan data lot tersebut.

## AC-F16-02: QR pada Label Detail Lot

**Given** pengguna membuka halaman detail satu Lot.
**When** menekan tombol "Print Label".
**Then** dialog menampilkan label dengan QR (±120px) yang muat di label 90mm × 60mm bersama teks.
**And** label memuat `LotNumber`, nama item, tanggal expiry, dan lokasi penyimpanan.
**And** hasil cetak (`window.print()`) hanya menampilkan label, bukan elemen halaman lain.

## AC-F16-03: Bulk Print Beberapa Lot

**Given** `WarehouseAdmin` memilih 3 lot melalui checkbox di halaman Lot Master.
**When** menekan "Print Selected Labels (3)".
**Then** dialog cetak menampilkan tepat 3 label, satu per lot terpilih.
**And** setiap label memuat QR untuk lot tersebut.
**And** label disusun 2 kolom per halaman A4.

## AC-F16-04: Label Tidak Ekspos Data Sensitif

**Given** payload QR sebuah Lot.
**When** payload di-decode.
**Then** isinya hanya `LCET`, `ItemId`, `LotId`, dan `LotNumber`.
**And** tidak memuat harga, supplier, catatan, atau data pengguna.

## AC-F16-05: Parser Menerima Kiri Lama dan Menolak QR Rusak

**Given** scanner mengembalikan `LCET|<ItemId>|<LotId>|<LotNumber>` yang valid.
**When** `ConsumptionConfirmation.handleVerify` memproses string itu.
**Then** parser mengembalikan payload dan `lotNumber`-nya yang dikirim ke `verify-scan`, bukan string mentah.

**Given** scanner mengembalikan nomor lot polos (`LOT-2026-001`), tanpa prefix.
**When** string itu diproses.
**Then** string diteruskan apa adanya ke `verify-scan`, sehingga alur F-15 sebelum F-16 tetap berfungsi.

**Given** scanner mengembalikan string yang mengandung `|` tapi gagal di-parse (misalnya `LCET|a|b` yang hanya 3 bagian, atau prefix selain `LCET`).
**When** string itu diproses.
**Then** parser menolak payload.
**And** frontend menampilkan pesan barcode tidak valid di bawah input, tanpa mengirim request ke `verify-scan`.

## AC-F16-06: Tidak Ada Perubahan Database

**Given** F-16 sudah diimplementasikan penuh.
**When** diperiksa seluruh migration EF Core.
**Then** tidak ada migration baru yang ditambahkan oleh F-16.
**And** API untuk Lot tidak berubah.

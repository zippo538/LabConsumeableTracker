# F-17: Acceptance Criteria

## AC-F17-01: Alur Manual Tetap Utuh

**Given** Scientist menolak izin kamera, atau memblokirnya di pengaturan browser.
**When** membuka halaman Consume, membuat draft, lalu mengetik nomor lot atau memakai handheld scanner.
**Then** seluruh alur consumption F-15 berhasil normal.
**And** tidak ada error yang muncul, dan tidak ada fungsi yang hilang.
**And** kamera tidak pernah diminta otomatis saat halaman dibuka.

Ini AC paling kritis. Kamera adalah pintasan, bukan syarat. Kalau AC ini gagal, F-17 tidak boleh di-rollout.

## AC-F17-02: Scan Kamera Mengisi Input yang Sama

**Given** Scientist sudah membuat draft sehingga tombol "Scan with Camera" muncul.
**When** menekan tombol tersebut.
**Then** dialog terbuka dengan live preview dari kamera belakang (`facingMode: "environment"`).

**Given** dialog kamera terbuka dan QR label terlihat di frame.
**When** decoder membaca QR tersebut.
**Then** string yang terbaca masuk ke input lot number yang sama dengan yang dipakai input manual dan handheld.
**And** dialog tertutup otomatis.
**And** verifikasi berjalan lewat jalur `parseLotQrPayload` → `verify-scan`, sama seperti jalur lain.

**And** tidak ada parsing format QR terpisah di F-17 — hanya satu parser (`parseLotQrPayload` dari F-16).

## AC-F17-03: Auto-scan Tidak Memicu Salah Verifikasi

**Given** dialog kamera terbuka dan ada QR label lain yang terlihat di frame (misalnya label di meja sebelah atau kemasan yang belumuctions).
**When** decoder membacanya.
**Then** string-nya tetap melewati validasi payload F-16 sebelum request dikirim.
**And** kalau payload-nya tidak valid, verifikasi ditolak dengan pesan "Barcode tidak valid" dan tidak ada request yang dikirim.
**And** kalau payload-nya valid tapi lot-nya bukan bagian draft, server membalas `NO_PENDING_CONSUMPTION` dan stok tidak berubah.

## AC-F17-04: Error yang Bisa Ditindaklanjuti

**Given** aplikasi diakses lewat `http://` pada IP LAN (bukan localhost).
**When** Scientist menekan "Scan with Camera".
**Then** dialog menampilkan pesan bahwa kamera hanya bisa diakses lewat HTTPS, plus arahan memakai handheld atau ketik manual.
**And** tidak ada crash atau unhandled error.

**Given** Izin kamera ditolak.
**When** Scientist membuka dialog kamera.
**Then** pesan menjelaskan cara mengaktifkan kembali izin di pengaturan browser, atau alternatif ketik manual.

**Given** Perangkat tidak punya kamera, atau browser tidak mendukung pembacaan QR.
**When** Scientist membuka dialog kamera.
**Then** pesan menjelaskan keadaannya dan menawarkan input manual.

**Given** Stream kamera gagal dimuat.
**When** dialog terbuka.
**Then** pesan error ditampilkan dan tombol "Coba Lagi" tersedia.

## AC-F17-05: Tidak Ada Perubahan Backend

**Given** F-17 sudah diimplementasikan penuh.
**When** diperiksa seluruh migration EF Core dan API.
**Then** tidak ada migration baru yang ditambahkan oleh F-17.
**And** tidak ada endpoint, DTO, atau perubahan kontrak `verify-scan`.
**And** decode kamera adalah operasi frontend sepenuhnya — tidak ada gambar yang dikirim ke server.

## AC-F17-06: Prasyarat Rollout Terverifikasi

**Given** F-17 akan dipakai di produksi.
**When** tim infra dicek sebelum rollout.
**Then** dua hal berikut terkonfirmasi:

1. **Scheme URL deployment** — `https://` atau `http://localhost`. Kalau ternyata HTTP di IP LAN, fitur kamera tidak akan pernah jalan dan harus diperbaiki (sertifikat) atau Fitur di-rollout.
2. **Browser target scientist** —Safari, Chrome, atau campuran. Hasilnya menentukan apakah `BarcodeDetector` native bisa dipakai atau harus fallback ke library decoder.

**And** bila prasyarat 1 tidak terpenuhi, camera scan ditunda sampai sertifikat tersedia — sementara alur manual dan handheld tetap menjadi jalur utama.

## AC-F17-07: Kamera Berhenti Bersih

**Given** Scientist menutup dialog kamera.
**When** dialog ditutup.
**Then** semua track kamera dihentikan dan indikasi kamera (light) mati.

**Given** Scientist sedang scan lalu pindah halaman atau menavigasi keluar.
**When** komponen dilepas.
**Then** kamera tetap dihentikan lewat cleanup, bukan hanya saat tombol "Tutup" ditekan.

**And** decode loop berhenti — tidak ada proses yang berjalan di background setelah dialog tertutup.

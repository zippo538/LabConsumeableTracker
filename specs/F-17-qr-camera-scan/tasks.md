# F-17: Tasks

## Prasyarat (harus selesai sebelum kode)
- [ ] T-17-01: [Infra] Konfirmasi scheme URL deployment: `https://` atau `http://localhost`. Kalau HTTP di IP LAN, kamera diblokir browser — fitur harus ditunda sampai sertifikat tersedia. Lihat spec §5.1 dan AC-F17-06.
- [ ] T-17-02: [Riset] Konfirmasi browser yang dipakai scientist di lokasi (Safari, Chrome, atau campuran). Hasilnya menentukan apakah `BarcodeDetector` native bisa dipakai. Lihat spec §5.2.
- [ ] T-17-03: [Riset] **Wajib** — riset dokumentasi aktual library QR decoder lewat `ctx7` atau dokumentasi resmi. Jangan dari ingatan; surface API berubah antar versi. Bandingkan `BarcodeDetector` native vs library decoder JS vs wrapper composite (spec §5.3), lalu tulis keputusannya di sini.

  **Keputusan library: [ ] belum dipilih**

## Fondasi
- [ ] T-17-04: [Web] Audit `ConsumptionConfirmation.tsx`. Pastikan `handleVerify` menerima string dari luar (bukan hanya dari input) supaya webcam bisa mengirim string yang sama tanpa menduplikasi logika.
- [ ] T-17-05: [Web] Pastikan handheld HID scanner tetap works setelah perubahan — ketik karakter + Enter harus tetap memicu verifikasi.

## Dialog Kamera
- [ ] T-17-06: [Web] Buat `CameraScanDialog` di `src/components/consumption/`. Preview `<video>` dengan `facingMode: "environment"`.
- [ ] T-17-07: [Web] Tombol "Aktifkan Kamera" terpisah, bukan otomatis di `useEffect` — iOS Safari butuh user gesture eksplisit untuk izin kamera.
- [ ] T-17-08: [Web] Stop semua track kamera di cleanup effect (bukan hanya saat tombol "Tutup") supaya kamera mati saat navigasi keluar. Lihat AC-F17-07.
- [ ] T-17-09: [Web] Auto-scan: begitu QR terbaca, kirim string ke parent, tutup dialog. Loop decode berhenti saat dialog ditutup.
- [ ] T-17-10: [Web] Throttle frekuensi decode. Kalau pengukuran menunjukkan CPU masalah, pindahkan ke Web Worker (spec §7).

## Error Handling
- [ ] T-17-11: [Web] Deteksi secure context sebelum membuka dialog. Kalau bukan `https://` dan bukan `localhost`, tampilkan pesan yang jelas tanpa memanggil `getUserMedia` sama sekali.
- [ ] T-17-12: [Web] Bedakan jenis error: izin ditolak vs tidak ada kamera vs stream gagal vs browser tidak mendukung. Masing-masing punya pesan berbeda (spec §4.3).
- [ ] T-17-13: [Web] Tombol "Coba Lagi" saat stream gagal.

## Integrasi
- [ ] T-17-14: [Web] Tombol "Scan with Camera" di `ConsumptionConfirmation`, hanya saat `hasDraft` sudah true (spec §4.1).
- [ ] T-17-15: [Web] String hasil bacaan kamera masuk ke input yang sama dengan input manual — nol state baru, nol jalur verifikasi terpisah.
- [ ] T-17-16: [Web] Pastikan tidak ada parsing format QR di F-17. Satu parser saja: `parseLotQrPayload` dari F-16. Lihat AC-F17-02.

## Verifikasi
- [ ] T-17-17: [Tests] Alur manual tetap works dengan kamera dimatikan / ditolak. Lihat AC-F17-01 — ini gate rollout.
- [ ] T-17-18: [Tests] QR di-decode dari webcam menghasilkan payload yang cocok dengan `buildLotQrPayload` F-16, dan diteruskan sebagai `lotNumber` yang benar.
- [ ] T-17-19: [Tests] Payload rusak dari kamera ditolak dengan pesan jelas, tanpa request ke `verify-scan`. Lihat AC-F17-03.
- [ ] T-17-20: [Tests] Kamera berhenti setelah dialog ditutup dan setelah unmount. Indikator light harus mati.
- [ ] T-17-21: [Docs] Update README web: cara pakai scan kamera, dan syarat HTTPS.

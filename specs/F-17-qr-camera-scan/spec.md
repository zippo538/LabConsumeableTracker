# F-17: Kamera QR Scan

## 1. Ringkasan

F-15 menyediakan alur consumption dua fase, tapi input scan-nya cuma text field. Sampai sekarang tidak ada pembacaan dari kamera: scientist harus mengetik nomor lot manual atau memakai handheld barcode scanner mode keyboard.

F-17 menambahkan pembacaan QR lewat webcam, langsung di halaman Consume. Banyak scientist bekerja di laptop tanpa alat scan khusus, jadi mengetik nomor lot satu per satu tidak realistis.

F-17 **tidak mengubah kontrak scan**. Hasil bacaan kamera adalah string biasa yang masuk ke input yang sudah ada, lalu melewati `parseLotQrPayload` (F-16) → `verify-scan` (F-15). Tidak ada endpoint, DTO, atau state baru.

## 2. Aturan Domain (BR)

- **BR-26**: Kamera opsional. Tidak ada Scientist yang terkunci dari alur consumption karena kamera tidak tersedia atau ditolak.
- **BR-27**: Hasil bacaan QR tidak pernah diverifikasi otomatis sebelum melewati validasi payload F-16.
- **BR-28**: Kamera berhenti streaming saat komponen dilepas atau dialog ditutup.

BR-26 adalah yang paling penting. Halaman Consume harus tetap berfungsi penuh untuk scientist yang menolak izin kamera, memakai handheld scanner, atau mengetik manual. Kamera adalah pintasan, bukan syarat.

## 3. Konvensi Pemindaian

Ditetapkan di F-16 §3 dan tidak diubah:

```
LCET|<ItemId>|<LotId>|<LotNumber>
```

Reader kamera harus mengembalikan persis string ini, lalu melepaskannya ke input yang sama dengan scanner handheld. Setelah itu `parseLotQrPayload` yang memutuskan valid atau tidak. Tidak ada parsing QR duplikat di F-17.

**Alasan tidak ada parsing terpisah:** kalau reader kamera punya parser sendiri, ada dua tempat yang harus tahu format QR, dan keduanya pasti akan simpang seiring waktu.

### Tiga jalur input, satu jalur verifikasi

| Jalur | Alur | Tujuan |
|---|---|---|
| Webcam | QR → decoder → string → input | `setLotNumber(decodedText)` |
| Handheld HID | keyboard → string → input | sudah works, tanpa kode |
| Manual | keyboard → string → input | sudah works, tanpa kode |

Ketiganya bertemu di `ConsumptionConfirmation.handleVerify`, yang sudah memanggil `parseLotQrPayload` (F-16) dan menolak payload rusak sebelum request dikirim.

## 4. Spesifikasi UI

### 4.1. Tombol Scan dengan Kamera

Di `ConsumptionPage`, section step 3 (Verify consumption) mendapat tombol "Scan with Camera" di samping input teks.

```
┌──────────────────────────────────────────┐
│  Scan each lot below to release stock    │
│                                          │
│  [LOT-2026-001]  [LOT-2026-002]          │
│                                          │
│  [ Scan or type lot number...      ]     │
│  [ 📷 Scan with Camera ] [ Verify Scan ] │
└──────────────────────────────────────────┘
```

- Tombol hanya muncul saat sudah ada draft (`hasDraft`). Tidak ada gunanya memindai sebelum draft ada — lot yang discan tidak akan cocok dengan apa pun.
- Klik membuka dialog dengan pratinjau video.
- Dialog punya tombol "Muat Ulang" (kalau stream gagal) dan "Tutup".

### 4.2. Dialog Kamera

```
┌────────────────────────────────────┐
│  Scan Lot          [Aktifkan Kamera]│
│                                    │
│  ┌──────────────────────────────┐  │
│  │                              │  │
│  │      live video preview      │  │
│  │                              │  │
│  └──────────────────────────────┘  │
│                                    │
│  Arahkan kamera ke label lot.      │
│                                    │
│  [ Tutup ]                         │
└────────────────────────────────────┘
```

- Live preview harus memakai `facingMode: "environment"` supaya HP memakai kamera belakang, bukan kamera depan yang tidak akan pernah membaca label.
- Auto-scan: begitu QR terbaca, string langsung masuk ke input, dialog tertutup, verifikasi berjalan. Tidak perlu menekan tombol kedua.
- Dialog ditutup otomatis setelah bacaan pertama berhasil.

### 4.3. Error Handling

Semua kegagalan harus punya pesan yang bisa ditindaklanjuti, bukan `alert()` generik.

| Kondisi | Pesan |
|---|---|
| HTTPS bukan localhost | "Kamera hanya bisa diakses lewat HTTPS. Gunakan handheld scanner atau ketik nomor lot." |
| Izin ditolak | "Izin kamera ditolak. Aktifkan kamera di pengaturan browser atau ketik nomor lot." |
| Tidak ada kamera | "Tidak ada kamera terdeteksi pada perangkat ini." |
| Stream gagal | "Kamera tidak dapat diakses. Coba lagi atau ketik nomor lot." |
| Browser tidak mendukung | "Browser ini tidak mendukung pembacaan QR lewat kamera. Gunakan scanner atau ketik manual." |

Kondisi HTTPS perlu penekanan khusus: ini bukan error yang bisa diperbaiki user, hanya info deployment. Lihat §5.2.

## 5. Prasyarat

### 5.1. Dukungan Browser

`getUserMedia` butuh **secure context**: `https://` atau `http://localhost`. Kalau aplikasi di-deploy di IP LAN (`http://192.168.x.x`), browser memblokir kamera secara otomatis dan tidak ada kode sisi-aplikasi yang bisa mengatasinya. Scanner handheld tetap berfungsi normal karena tidak butuh kamera.

**Penting untuk rollout:** status deployment belum dikonfirmasi (lihat §5.2). Kalau ternyata HTTP LAN, fitur kamera harus gagal dengan baik — dialog menampilkan pesan jelas, bukan crash.

### 5.2. Prasyarat yang Belum Dikonfirmasi

Pekerjaan implementasi tidak boleh dianggap selesai sebelum dua hal ini dicek:

1. **Apakah deployment memakai HTTPS?** Kalau tidak, fitur kamera tidak akan pernah jalan di produksi dan perlussl certificate dulu.
2. **Browser apa yang dipakai scientist?** Webcam QR reading dari `getUserMedia` + `BarcodeDetector` tidak didukung merata di semua browser, terutama Safari.

Keduanya masuk sebagai AC, bukan asumsi.

### 5.3. Library

Pilihan library belum final dan **wajib riset dokumentasi aktual sebelum implementasi** (`ctx7` atau dokumentasi resmi) — jangan dari ingatan. Surface API berubah antar versi.

Kandidat yang perlu dievaluasi:

| Opsi | Catatan |
|---|---|
| `BarcodeDetector` API native | API browser modern, tanpa dependency. Dukungan browser tidak merata — perlu dicek, terutama Safari. Tidak bisa diasumsikan tersedia. |
| Library decoder JS (mis. `jsQR`) | Bekerja di mana pun `getUserMedia` bekerja, tapi menambah dependency dan CPU decode di thread utama. |
| Wrapper composite | Beberapa library membungkus kamera + decoder sekaligus, simplifying banyak kode glue. Perlu cek apakah pas dengan React 19 dan project ini. |

Kriteria pemilihan: dukungan browser target (§5.2), compatible dengan React 19, tidak menambah build weight yang tidak perlu, dan bisa_decode dari `<video>` element yang sudah di-ref.

## 6. Model Data

**Tidak ada perubahan.** Tidak ada endpoint baru, tidak ada DTO baru, tidak ada migration. `verify-scan` sudah menerima `ScannedLotNumber` apa adanya.

## 7. Risiko & Mitigasi

- **Risiko**: QR label di meja atau di kemasan lain terbaca kamera secara tidak sengaja, memicu verifikasi yang tidak diinginkan.
  - **Mitigasi**: kamera hanya aktif saat dialog dibuka, dan langsung tertutup saat bacaan pertama. Dialog tidak berjalan di background.
- **Risiko**: decode berjalan terus-menerus dan membebani CPU thread utama, membuat halaman melambat.
  - **Mitigasi**: hentikan loop decode saat dialog ditutup, dan throttle frekuensi decode. Kalau pengukuran menunjukkan masalah, pindahkan ke Web Worker.
- **Risiko**: kamera tidak tersedia di sebagian besar perangkat scientist, fitur terasa seperti tambahan yang tidak bisa dipakai.
  - **Mitigasi**: alur manual dan handheld tetap fully supported dan jadi default. Kamera tidak pernah menjadi syarat.
- **Risiko**: iOS Safari butuh HTTPS dan user gesture eksplisit untuk memberi izin kamera.
  - **Mitigasi**: tombol "Aktifkan Kamera" terpisah, jadi request stream dipicu dari klik yang jelas — bukan dari `useEffect` yang bisa ditolak browser.
- **Risiko**: stream kamera tidak berhenti saat pindah halaman atau unmount, kamera tetap menyala.
  - **Mitigasi**: hentikan semua track di cleanup effect, bukan hanya saat tombol "Tutup" ditekan.

## 8. Di Luar Scope

- Scan multi-label sekaligus dalam satu frame.
- Scan dari video/gambar yang di-upload.
- Audio atau feedback beep saat capture sukses.
- Konfigurasi kamera (pilihan device, resolusi).
- Persistensi kamera terakhir yang dipakai.

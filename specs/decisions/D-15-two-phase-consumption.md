# D-15: Two-Phase Consumption & Scan Verification

**Status**: DECIDED
**Tanggal**: 24 Mei 2024 (Sesuaikan dengan tanggal saat ini)
**Mempengaruhi**: F-06 (Consumption), FR-06-04

## Konteks
SRS v1.0 (FR-06-04) menyatakan bahwa deduksi stok dan pembuatan Consumption harus atomik saat konfirmasi. Namun, kebutuhan bisnis baru mengharuskan bahwa **stok fisik hanya boleh berkurang jika dan hanya jika Lot tersebut telah di-scan secara fisik** (atau LotNumber divalidasi eksplisit sebagai tindakan scan). Konfirmasi di UI tidak lagi mengurangi stok.

## Keputusan
1. **Two-Phase Consumption**: Alur konsumsi dibagi menjadi dua fase.
   - **Fase 1 (Draft/Intent)**: Scientist mengonfirmasi alokasi FEFO di UI. Backend membuat record `Consumption` dengan status `PendingScan`. **Stok TIDAK berkurang**.
   - **Fase 2 (Scan Verification)**: Scientist memindai barcode fisik (atau mengetik LotNumber). Backend memvalidasi kecocokan `ItemId` dan `LotNumber`, mengubah status menjadi `Verified`, dan **baru mengurangi stok**.
2. **First-to-Scan Wins**: Tidak ada mekanisme *soft-reserve* saat Fase 1. Jika dua Scientist membuat draft untuk Lot yang sama, siapa yang berhasil melakukan scan fisik (Fase 2) lebih dulu yang akan mendapatkan stok. Yang kedua akan menerima error `INSUFFICIENT_STOCK` dan harus me-refresh preview FEFO.
3. **Format Input**: Endpoint verifikasi menerima `ItemId` dan `ScannedLotNumber` (string mentah dari scanner/kamera). Backend memvalidasi kecocokan dengan record `PendingScan`.

## Dampak
- Meng-override perilaku atomik langsung pada FR-06-04 menjadi dua langkah terkoordinasi.
- Menambahkan field `Status` dan `VerifiedAt` pada entitas `Consumption`.
- Menambahkan enum `ConsumptionStatus` (`PendingScan`, `Verified`).
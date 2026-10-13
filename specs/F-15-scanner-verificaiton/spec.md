# F-15: Scanner Verification & Two-Phase Consumption

## 1. Ringkasan
F-15 mengubah alur F-06 (Consumption) menjadi dua fase untuk memastikan stok hanya berkurang setelah verifikasi fisik (scan barcode/input LotNumber). 

## 2. Aturan Domain (BR)
- **BR-17**: Konsumsi memiliki dua status: `PendingScan` (Fase 1) dan `Verified` (Fase 2).
- **BR-18**: `RemainingQuantity` Lot HANYA berkurang saat `Consumption` bertransisi dari `PendingScan` ke `Verified`.
- **BR-19**: Pembuatan draft (`PendingScan`) tidak mengurangi stok dan tidak memblokir Lot dari preview FEFO pengguna lain (First-to-Scan Wins).
- **BR-20**: Verifikasi scan harus memvalidasi bahwa `ScannedLotNumber` cocok dengan `LotNumber` dari Lot yang memiliki draft `PendingScan` untuk `ItemId` dan `JobId` yang sama.
- **BR-21**: Verifikasi scan harus mengevaluasi ulang eligibility (Expired, Active, RemainingQuantity) pada waktu scan aktual.

## 3. Spesifikasi API

### 3.1. Fase 1: Membuat Draft Konsumsi
Menggantikan endpoint `POST /api/jobs/{jobId}/consumptions` untuk alur baru.
- **Endpoint**: `POST /api/jobs/{jobId}/consumptions/draft`
- **Akses**: `scientis`
- **Request Body**:
  ```json
  {
    "ItemId": "guid",
    "Allocations": [
      { "LotId": "guid", "Quantity": 4.0, "ExpectedRowVersion": "base64" }
    ]
  }

- Respons Sukses (201 Created):
```json
  {
    "JobId": "guid",
    "ItemId": "guid",
    "Consumptions": [
      { "Id": "guid", "LotId": "guid", "Quantity": 4.0, "Status": "PendingScan", "CreatedAt": "timestamp" }
    ]
  }
```

- Error: 400 (Validasi), 404 (Job/Item/Lot tidak ada), 409 (INSUFFICIENT_STOCK, CONCURRENCY_CONFLICT, INVALID_JOB_STATE).

### 3.2. Fase 2: Verifikasi Scan
- **Endpoint**: POST /api/jobs/{jobId}/ consumptions/verify-scan
- **Akses**: scientis
- **Request Body:**
```json
  {
    "ItemId": "guid",
    "ScannedLotNumber": "LOT-2026-001" 
  }
```

- Respons Sukses (200 OK):

```json
  {
    "ConsumptionId": "guid",
    "LotId": "guid",
    "LotNumber": "LOT-2026-001",
    "Quantity": 4.0,
    "Status": "Verified",
    "VerifiedAt": "timestamp",
    "UpdatedLot": { "LotId": "guid", "RemainingQuantity": 6.0, "RowVersion": "base64" }
  }
```

- **Error:**
- 400 `VALIDATION_ERROR`: Format input salah.
- 404 `LOT_NOT_FOUND`: LotNumber yang di-scan tidak ada di sistem untuk ItemId tersebut.
- 409 `NO_PENDING_CONSUMPTION`: Lot ditemukan, tapi tidak ada draft konsumsi yang menunggu scan untuk Lot ini di Job ini.
- 409 `INSUFFICIENT_STOCK`: Stok Lot tidak cukup saat scan (karena diambil user lain).
- 409 `LOT_NOT_ELIGIBLE` / `LOT_EXPIRED`: Lot berubah status/expired sejak draft dibuat.

## 4. Model Data
- **Enum Baru:** ConsumptionStatus (PendingScan = 0, Verified = 1).
- **Entitas** Consumption: Tambah ConsumptionStatus Status, DateTimeOffset? VerifiedAt.

## 5. Konvensi Scanner
- Frontend bertanggung jawab membaca barcode (kamera/handheld) dan mengekstrak LotNumber.
- Jika barcode berisi format kompleks (misal: LCET|ItemId|LotNumber), Frontend harus mem-parsing dan mengirim ItemId serta ScannedLotNumber secara terpisah ke API.


---

### 3. Rencana Implementasi
Simpan file ini di `specs/F-15-scanner-verification/plan.md`

```markdown
# F-15: Implementation Plan

## 1. Strategi Implementasi
1. **Domain**: Tambahkan enum `ConsumptionStatus`. Modifikasi entitas `Consumption` untuk mendukung state `PendingScan` dan metode `VerifyScan()`.
2. **Application**: 
   - Buat `CreateDraftConsumptionCommand` (tanpa memanggil `lot.DeductStock()`).
   - Buat `VerifyScanCommand` (mencari Lot by LotNumber, mencari draft PendingScan, memvalidasi ulang, lalu memanggil `lot.DeductStock()` dan `consumption.VerifyScan()`).
3. **Infrastructure**: Update `ConsumptionConfiguration` di EF Core untuk field baru. Tambahkan method `GetByItemIdAndLotNumberAsync` dan `GetPendingByJobAndLotAsync` di Repository.
4. **API**: Update `ConsumptionsController`. Non-aktifkan/Replace endpoint consume lama dengan `/draft`, dan tambahkan `/verify-scan`.

## 2. Dependensi
- **F-05 (FEFO Preview)**: Endpoint draft sangat bergantung pada struktur data yang dikembalikan oleh preview FEFO (terutama `ExpectedRowVersion`).
- **F-06 (Consumption)**: F-15 memodifikasi alur inti F-06.

## 3. Risiko & Mitigasi
- **Risiko**: Race condition saat dua user men-scan Lot yang sama.
  - **Mitigasi**: Gunakan `IsolationLevel.Serializable` atau optimistic concurrency (`RowVersion`) pada saat `DeductStock`. Jika user B scan setelah user A, `RemainingQuantity` akan kurang dari yang diminta, memicu error `INSUFFICIENT_STOCK` di validasi domain. UI harus menangkap error ini dan meminta user me-refresh preview.
- **Risiko**: Draft `PendingScan` menumpuk dan mengotori DB.
  - **Mitigasi**: (Opsional untuk MVP) Bisa ditambahkan background job untuk membersihkan draft yang tidak pernah di-scan setelah Job di-Complete/Cancel. Untuk MVP, biarkan saja tercatat sebagai riwayat intent.
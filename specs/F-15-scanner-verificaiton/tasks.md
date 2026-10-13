# F-15: Tasks

## Domain
- [ ] T-15-01: [Domain] Buat enum `ConsumptionStatus` (`PendingScan`, `Verified`) di `Models/Enums/`.
- [ ] T-15-02: [Domain] Modifikasi entitas `Consumption`: tambah property `Status`, `VerifiedAt`, dan factory method `CreateDraft()`.
- [ ] T-15-03: [Domain] Tambahkan method `VerifyScan(DateTimeOffset now)` di entitas `Consumption` dengan validasi state.

## Application
- [ ] T-15-04: [Application] Buat DTO `CreateDraftConsumptionRequestDto` dan `VerifyScanRequestDto`.
- [ ] T-15-05: [Application] Buat DTO `DraftConsumptionResponseDto` dan `VerifyScanResponseDto`.
- [ ] T-15-06: [Application] Tambahkan method `CreateDraftAsync` di `IConsumptionService` dan implementasikan di `ConsumptionService` (Validasi Job, Lot, RowVersion, tapi JANGAN deduksi stok).
- [ ] T-15-07: [Application] Tambahkan method `VerifyScanAsync` di `IConsumptionService` dan implementasikan (Cari Lot by LotNumber, cari draft, validasi ulang eligibility & stok, deduksi stok, ubah status).
- [ ] T-15-08: [Application] Buat Validator `VerifyScanRequestValidator` (Pastikan ItemId dan ScannedLotNumber tidak kosong).

## Infrastructure
- [ ] T-15-09: [Infrastructure] Update `ConsumptionConfiguration.cs` (EF Core) untuk mapping `Status` dan `VerifiedAt`.
- [ ] T-15-10: [Infrastructure] Tambahkan method `GetByItemIdAndLotNumberAsync(Guid itemId, string lotNumber)` di `ILotRepository` dan `LotRepository`.
- [ ] T-15-11: [Infrastructure] Tambahkan method `GetPendingByJobAndLotAsync(Guid jobId, Guid lotId)` di `IConsumptionRepository` dan `ConsumptionRepository`.
- [ ] T-15-12: [Infrastructure] Generate EF Core Migration untuk penambahan kolom `Status` dan `VerifiedAt` di tabel `Consumptions`.

## API
- [ ] T-15-13: [API] Update `ConsumptionsController`: Ubah endpoint `POST /api/jobs/{jobId}/consumptions` menjadi `POST .../consumptions/draft` yang memanggil `CreateDraftAsync`.
- [ ] T-15-14: [API] Tambahkan endpoint `POST /api/jobs/{jobId}/consumptions/verify-scan` di `ConsumptionsController` yang memanggil `VerifyScanAsync`.
- [ ] T-15-15: [API] Pastikan mapping `ServiceResult` ke HTTP Status (404 untuk LOT_NOT_FOUND, 409 untuk NO_PENDING_CONSUMPTION/INSUFFICIENT_STOCK).

## Tests
- [ ] T-15-16: [Tests] Unit Test Domain: `Consumption.CreateDraft()` menghasilkan status `PendingScan`.
- [ ] T-15-17: [Tests] Unit Test Domain: `Consumption.VerifyScan()` berhasil mengubah status dan gagal jika sudah verified.
- [ ] T-15-18: [Tests] Unit Test Application: `CreateDraftAsync` tidak memanggil `lot.DeductStock()`.
- [ ] T-15-19: [Tests] Unit Test Application: `VerifyScanAsync` memanggil `lot.DeductStock()` dan mengembalikan error jika Lot tidak ditemukan.
- [ ] T-15-20: [Tests] Integration Test: Dua user membuat draft untuk Lot yang sama. User A scan berhasil (stok berkurang). User B scan gagal dengan `INSUFFICIENT_STOCK`.
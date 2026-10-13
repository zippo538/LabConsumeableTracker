# F-15: Acceptance Criteria

## AC-F15-01: Fase 1 - Pembuatan Draft Tanpa Deduksi Stok
**Given** sebuah Job dengan status `InProgress` dan Lot A memiliki `RemainingQuantity` 10.
**When** `scientis` memanggil `POST /api/jobs/{jobId}/consumptions/draft` untuk Lot A sebanyak 4.
**Then** Sistem mengembalikan 201 Created dengan record Consumption berstatus `PendingScan`.
**And** `RemainingQuantity` Lot A di database tetap 10 (tidak berkurang).

## AC-F15-02: Fase 2 - Verifikasi Scan Berhasil
**Given** terdapat record Consumption `PendingScan` untuk Lot A (LotNumber: "LOT-001") sebanyak 4.
**When** `scientis` memanggil `POST /api/jobs/{jobId}/consumptions/verify-scan` dengan `ScannedLotNumber` = "LOT-001".
**Then** Sistem mengembalikan 200 OK dengan status `Verified`.
**And** `RemainingQuantity` Lot A di database berkurang menjadi 6.
**And** `VerifiedAt` terisi dengan waktu server.

## AC-F15-03: Fase 2 - Scan Lot yang Salah / Tidak Ada Draft
**Given** terdapat record Consumption `PendingScan` untuk Lot A.
**When** `scientis` memanggil `verify-scan` dengan `ScannedLotNumber` = "LOT-999" (Lot lain yang tidak punya draft).
**Then** Sistem mengembalikan 409 Conflict dengan kode `NO_PENDING_CONSUMPTION`.
**And** Stok Lot A dan Lot 999 tidak berubah.

## AC-F15-04: Fase 2 - Race Condition (First-to-Scan Wins)
**Given** Scientist A dan Scientist B sama-sama membuat draft `PendingScan` untuk Lot A (Stok awal 10, masing-masing draft 6).
**When** Scientist A melakukan `verify-scan` lebih dulu dan berhasil (Stok Lot A menjadi 4).
**And** Scientist B melakukan `verify-scan` setelahnya.
**Then** Request Scientist B ditolak dengan 409 Conflict kode `INSUFFICIENT_STOCK`.
**And** Stok Lot A tetap 4 (tidak menjadi negatif).

## AC-F15-05: Evaluasi Ulang Eligibility Saat Scan
**Given** terdapat record `PendingScan` untuk Lot A.
**And** Lot A mencapai masa `Expired`
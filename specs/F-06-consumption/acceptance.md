# F-06 — Acceptance Criteria

### AC-F06-01: Konsumsi Sukses

**Given** Lot eligible, quantity positif ≤ remaining
**When** consume
**Then** deduksi dan Consumption tercipta atomik.

### AC-F06-02: Quantity Invalid

**Given** quantity nol / negatif / melebihi remaining
**When** consume
**Then** ditolak tanpa perubahan stok.

### AC-F06-03: Lot Expired Antara Preview dan Commit

**Given** Lot valid pada preview lalu expired
**When** consume aktual
**Then** ditolak 409 LOT_EXPIRED.

### AC-X-01: RowVersion Mismatch

**Given** RowVersion stale
**When** update stock
**Then** conflict tanpa menimpa perubahan pihak lain.

### AC-X-02: Atomicity

**Given** salah satu validasi gagal
**When** akhir operasi
**Then** tidak ada quantity/audit sukses parsial.

### AC-FE-11: UI Consume

**Given** submit konsumsi pending/sukses
**When** klik berulang
**Then** UI mencegah duplikasi; sukses menampilkan bukti server dan refresh.

### AC-FE-12: Conflict Consume

**Given** expiry berubah atau token stale
**When** consume
**Then** 409, reload/reconfirm, tanpa silent substitution.

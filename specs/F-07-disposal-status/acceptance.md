# F-07 — Acceptance Criteria

### AC-F07-01: Disposal Sukses

**Given** Lot Active, quantity valid, Reason terisi
**When** dispose
**Then** quantity dan Disposal commit bersama.

### AC-F07-02: Blocked/Quarantined Tidak Eligible

**Given** Lot blocked/quarantined
**When** alokasi FEFO
**Then** tidak eligible.

### AC-FE-13: UI Disposal

**Given** disposal invalid/valid
**When** admin submit
**Then** validasi/konfirmasi terlihat; sukses menunjukkan bukti dan stock/audit terbaru.

### AC-API-04: Status + Disposal

**Given** disposal/status valid, invalid, stale
**When** admin submit
**Then** akses, reason, transaksi, dan token sesuai D-08.

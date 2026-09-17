# F-05 — Acceptance Criteria

### AC-F05-01: Expiry Ascending

**Given** beberapa Lot eligible pada Item yang sama
**When** alokasi FEFO
**Then** Lot diurutkan expiry ascending.

### AC-F05-02: Tie-Breaker

**Given** expiry sama
**When** alokasi
**Then** ReceivedAt ascending, lalu Id ascending.

### AC-F05-03: Insufficient Stock

**Given** total eligible kurang dari requested
**When** alokasi
**Then** seluruh permintaan ditolak tanpa partial persistence.

### AC-FE-10: UI Preview

**Given** preview multi-Lot
**When** ditinjau
**Then** urutan, quantity/unit, total, dan penjelasan "bukan reservasi" terlihat.

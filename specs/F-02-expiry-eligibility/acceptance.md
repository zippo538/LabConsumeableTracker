# F-02 — Acceptance Criteria

### AC-F02-01: Valid

**Given** now sebelum cutoff dan di luar warning window
**When** evaluasi expiry
**Then** ExpiryCondition = Valid.

### AC-F02-02: ExpiringSoon dan Eligible

**Given** expiry dalam warning window, Lot Active, remaining > 0
**When** evaluasi
**Then** ExpiryCondition = ExpiringSoon dan IsEligible = true.

### AC-F02-03: Expired Ditolak dari Alokasi

**Given** now pada/setelah cutoff
**When** alokasi FEFO
**Then** Lot dikeluarkan dari kandidat.

### AC-F02-04: ExpiryCondition Bukan Field Persisten

**Given** Lot disimpan di database
**When** dibaca kembali
**Then** ExpiryCondition dihitung ulang, bukan dibaca dari kolom DB.

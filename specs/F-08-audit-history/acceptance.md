# F-08 — Acceptance Criteria

### AC-F08-01: Audit Read

**Given** konsumsi/disposal sukses
**When** baca audit
**Then** LotId, quantity, waktu, actor, serta Job/Reason tersedia sesuai jenis.

### AC-API-05: Filter Audit

**Given** dua jenis event
**When** filter/pagination
**Then** actor, waktu, quantity benar; Job hanya Consumption, Reason pada Disposal.

### AC-FE-14: UI Audit

**Given** audit dipilih/difilter
**When** render
**Then** Consumption memiliki Job, Disposal memiliki Reason; filter/pagination berfungsi.

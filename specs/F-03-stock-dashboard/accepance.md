# F-03 — Acceptance Criteria

### AC-F03-01: UsableStock per Item

**Given** Item dengan banyak Lot (beberapa eligible, beberapa tidak)
**When** hitung UsableStock
**Then** hanya usable remaining dijumlahkan, lalu dibanding threshold sesuai D-06.

### AC-API-01: Stock Read

**Given** stock ada dan kosong
**When** kedua role membaca
**Then** jumlah per Item benar, unit/EvaluatedAt tersedia, threshold sesuai D-06.

### AC-FE-05: Dashboard State

**Given** dashboard loading / gagal / kosong
**When** render
**Then** state dibedakan dan kegagalan bukan stock nol.

### AC-FE-06: Unit Berbeda

**Given** Item dengan unit berbeda
**When** baca dashboard/detail
**Then** usable stock dari API benar, tanpa total quantity lintas unit.

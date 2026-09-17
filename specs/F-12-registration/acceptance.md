# F-12 — Acceptance Criteria

### AC-F12-01: Register Sukses

**Given** register valid/unik
**When** submit
**Then** 201 akun aktif scientis, tanpa JWT otomatis, UI ke Login.

### AC-F12-02: Register Invalid

**Given** required/email/password invalid
**When** register
**Then** 400 tanpa akun tersimpan.

### AC-F12-03: Register Duplikat

**Given** username/email sudah digunakan
**When** register
**Then** 409 tanpa duplikasi.

### AC-F12-04: Privilege Escalation

**Given** payload publik menyisipkan role/status
**When** register
**Then** 400 tanpa privilege escalation.

### AC-F12-05: Role Assignment Gagal

**Given** pembuatan akun berhasil, assignment role gagal
**When** akhir transaksi
**Then** tidak ada akun registrasi parsial.

### AC-FE-03: UI Register

**Given** register invalid/duplikat/valid
**When** submit
**Then** error jelas atau sukses ke Login, tanpa pemilih role.

# F-13 — Acceptance Criteria

### AC-F13-01: Login Sukses

**Given** akun aktif dan password benar
**When** login
**Then** 200 JWT, Bearer, expiresAt, profil/role aman.

### AC-F13-02: Login Gagal

**Given** password salah atau akun nonaktif/terkunci
**When** login
**Then** gagal dengan pesan umum.

### AC-F13-03: Token Invalid

**Given** token invalid/expired atau role tidak sesuai
**When** endpoint terlindungi
**Then** 401 atau 403 sesuai kondisi, tanpa mutasi.

### AC-F13-04: Logout

**Given** sesi aktif
**When** logout
**Then** data sesi client dibersihkan dan UI ke Login.

### AC-FE-01: Route Terlindungi

**Given** belum login
**When** membuka route terlindungi
**Then** Login dan hanya kembali ke route internal yang sah.

### AC-FE-02: Role Berbeda

**Given** role berbeda
**When** membuka menu/route
**Then** UI mengikuti izin, route dilarang ditolak, server tetap memeriksa.

### AC-FE-04: Login/Logout Flow

**Given** login berhasil/gagal lalu logout
**When** jalankan alur
**Then** feedback dan navigasi tepat, cache/sesi lama dibersihkan.

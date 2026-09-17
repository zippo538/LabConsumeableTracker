# F-11 — Acceptance Criteria

### AC-F11-01: Profil Sendiri

**Given** akun aktif
**When** baca atau edit profil sendiri
**Then** hanya profil sendiri dan FullName dapat diperbarui.

### AC-F11-02: scientis Ditolak

**Given** scientis
**When** panggil daftar/detail/mutasi user admin
**Then** 403 tanpa perubahan.

### AC-F11-03: Admin Kelola Akun

**Given** WarehouseAdmin dan input valid
**When** kelola akun/status/role
**Then** hasil aman, tersimpan, dan audit tersedia.

### AC-F11-04: Akun Nonaktif

**Given** akun dirujuk transaksi
**When** dinonaktifkan
**Then** audit historis tetap, login dan akses berikutnya ditolak.

### AC-F11-05: Admin Terakhir

**Given** admin aktif terakhir
**When** dinonaktifkan atau kehilangan role
**Then** 409 dan minimal satu admin aktif tetap ada.

### AC-F11-06: Role Dicabut

**Given** JWT telah terbit
**When** role dicabut atau akun nonaktif
**Then** request berikutnya tidak memakai hak lama.

### AC-FE-15: UI User Management

**Given** kelola status/role atau edit profil
**When** submit
**Then** field/izin sesuai F-11 dan error admin terakhir terlihat.

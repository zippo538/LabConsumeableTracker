# F-01 — Acceptance Criteria

## Dari SRS §10.1

### AC-F01-01: Create Item

**Given** data Item valid (Code, Name, BaseUnit, MinimumStock, ExpiringSoonDays)
**When** WarehouseAdmin menyimpan
**Then** seluruh atribut sah tersimpan sesuai input dan dapat dibaca kembali.

### AC-F01-02: Dua Expiry = Dua Lot

**Given** satu Item dengan dua bahan berbeda expiry
**When** WarehouseAdmin mendaftarkan keduanya
**Then** tercipta dua Lot terpisah di bawah Item yang sama.

### AC-F01-03: InitialQuantity = RemainingQuantity

**Given** InitialQuantity valid (mis. 100)
**When** Lot diregistrasi
**Then** InitialQuantity tersimpan 100 dan RemainingQuantity awal = 100.

## Tambahan dari Spec

### AC-F01-04: Client Tidak Set RemainingQuantity

**Given** payload registrasi Lot menyertakan RemainingQuantity
**When** request dikirim
**Then** server menolak 400 VALIDATION_ERROR.

### AC-F01-05: Uniqueness Code/LotNumber

**Given** Item dengan Code "ABC" sudah ada
**When** WarehouseAdmin membuat Item baru dengan Code "ABC"
**Then** server menolak 409 DUPLICATE_RESOURCE.

### AC-F01-06: Deactivate Item

**Given** Item aktif dengan histori Lot
**When** WarehouseAdmin menonaktifkan Item
**Then** Item tidak muncul di list aktif, histori Lot tetap dapat dibaca.

### AC-F01-07: Concurrency Update Lot

**Given** dua admin membaca Lot yang sama (RowVersion = v1)
**When** admin A update berhasil (RowVersion → v2), lalu admin B update dengan v1
**Then** admin B mendapat 409 CONCURRENCY_CONFLICT tanpa overwrite.

### AC-FE-07: UI Item/Lot

**Given** admin dan scientis membuka halaman Item/Lot
**When** admin mengedit / scientis mencoba mengedit
**Then** hanya admin dapat mengelola field sah; InitialQuantity/RemainingQuantity
tidak dapat diedit bebas oleh siapapun.

### AC-FE-08: Detail Lot

**Given** Lot dibaca di UI
**When** detail dirender
**Then** status administratif, expiry, eligibility, unit, lokasi, dan waktu
evaluasi dapat dibedakan.

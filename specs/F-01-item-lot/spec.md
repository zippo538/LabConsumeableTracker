# F-01 — Master Item dan Registrasi/Pembaruan Lot

## Prioritas: Must | SRS §4.1 | Trace: FR-01-01 s.d. FR-01-09

## 1. Ringkasan

WarehouseAdmin mengelola master Item (kode, nama, unit, minimum stock,
warning days) dan mendaftarkan Lot (batch inventaris terkecil) yang merujuk
Item. Kedua role dapat membaca. Tidak ada hard delete; penonaktifan Item
mempertahankan histori.

## 2. Aturan Domain

| ID       | Aturan                                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------------------------- |
| BR-01    | Item memiliki Lot secara langsung; Lot adalah unit inventaris terkecil.                                             |
| BR-03    | RemainingQuantity awal = InitialQuantity; remaining tidak negatif; InitialQuantity tidak berubah setelah transaksi. |
| BR-05    | Unit transaksi mengikuti BaseUnit Item; tidak ada konversi otomatis.                                                |
| FR-01-05 | Server menginisialisasi RemainingQuantity = InitialQuantity. Client tidak menentukan RemainingQuantity.             |
| FR-01-06 | Bahan dengan expiry berbeda = Lot berbeda, walaupun Item sama.                                                      |
| FR-01-08 | InitialQuantity/RemainingQuantity tidak tersedia sebagai field edit bebas setelah registrasi.                       |

## 3. Model Data (SRS §6.1)

### Item

| Atribut          | Tipe          | Ketentuan                              |
| ---------------- | ------------- | -------------------------------------- |
| Id               | TBD (D-04)    | PK                                     |
| Code             | string        | Unik (D-04)                            |
| Name             | string        | Required                               |
| BaseUnit         | UnitOfMeasure | Enum: Milliliter, Gram, Unit, Vial     |
| MinimumStock     | decimal       | >= 0, precision/scale D-04             |
| ExpiringSoonDays | int           | >= 0                                   |
| IsActive         | bool          | Representasi penonaktifan (GAP-SRS-02) |

### Lot

| Atribut              | Tipe                     | Ketentuan                    |
| -------------------- | ------------------------ | ---------------------------- |
| Id                   | TBD (D-04)               | PK                           |
| ItemId               | FK → Item                | Required                     |
| LotNumber            | string                   | Unik (D-04)                  |
| InitialQuantity      | decimal                  | > 0, precision/scale D-04    |
| RemainingQuantity    | decimal                  | Server-set = InitialQuantity |
| ExpiryDate           | DateOnly/DateTime (D-05) | Required                     |
| ReceivedAt           | DateTime UTC             | Server-set                   |
| AdministrativeStatus | LotAdministrativeStatus  | Default: Active (D-08)       |
| StorageLocation      | string                   | Opsional                     |
| RowVersion           | byte[] / uint (D-03)     | Concurrency token            |

## 4. Endpoint (SRS §7.11 — kontrak belum dirinci, GAP-SRS-01)

> **Catatan:** Endpoint berikut adalah rancangan yang harus difinalisasi
> dalam OpenAPI sebelum implementasi.

| Method | Route                      | Akses | Deskripsi                                 |
| ------ | -------------------------- | ----- | ----------------------------------------- |
| POST   | /api/items                 | W     | Create Item                               |
| GET    | /api/items                 | S/W   | List Item (search, filter, pagination)    |
| GET    | /api/items/{id}            | S/W   | Detail Item + Lot terkait                 |
| PATCH  | /api/items/{id}            | W     | Update field master yang diizinkan        |
| PATCH  | /api/items/{id}/deactivate | W     | Nonaktifkan Item                          |
| POST   | /api/items/{itemId}/lots   | W     | Registrasi Lot                            |
| GET    | /api/lots                  | S/W   | List Lot (search, filter, pagination)     |
| GET    | /api/lots/{id}             | S/W   | Detail Lot                                |
| PATCH  | /api/lots/{id}             | W     | Update administratif Lot (field terbatas) |

## 5. Validasi (FR-01-09, D-04)

- Required: Code, Name, BaseUnit, MinimumStock, ExpiringSoonDays (Item);
  LotNumber, InitialQuantity, ExpiryDate, ReceivedAt (Lot).
- Uniqueness: Code (Item), LotNumber (Lot) — scope D-04.
- Quantity: > 0, precision/scale D-04, tidak ada pecahan unit jika D-04
  melarang.
- Tanggal: ExpiryDate tidak boleh di masa lalu saat registrasi (D-04).
- Client tidak boleh mengirim RemainingQuantity, RowVersion (saat create),
  atau field server lainnya.

## 6. Error

| HTTP | Code                 | Kondisi                        |
| ---- | -------------------- | ------------------------------ |
| 400  | VALIDATION_ERROR     | Field invalid, field terlarang |
| 404  | RESOURCE_NOT_FOUND   | Item/Lot tidak ada             |
| 409  | DUPLICATE_RESOURCE   | Code/LotNumber duplikat        |
| 409  | CONCURRENCY_CONFLICT | RowVersion stale (update)      |

## 7. Dependensi Keputusan

- D-04: Tipe ID, precision/scale, uniqueness, pecahan unit, registrasi expired.
- D-05: Representasi ExpiryDate.
- D-08: Status awal Lot saat registrasi.
- GAP-SRS-01: Kontrak CRUD belum dirinci.
- GAP-SRS-02: Representasi penonaktifan Item.

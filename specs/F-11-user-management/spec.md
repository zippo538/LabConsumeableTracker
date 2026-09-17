# F-11 — User Management

## Prioritas: Must | SRS §4.11 | Trace: FR-11-01 s.d. FR-11-08, AUTH-01 s.d. AUTH-06

## 1. Ringkasan

Pengguna aktif melihat profil sendiri. WarehouseAdmin mengelola akun:
list, create, update profil, ubah status aktif, assignment role. Akun
yang dirujuk audit dinonaktifkan, bukan dihapus.

## 2. Aturan

| ID       | Aturan                                                              |
| -------- | ------------------------------------------------------------------- |
| AUTH-01  | Role kanonis: `scientis` dan `WarehouseAdmin`.                      |
| AUTH-03  | Semua endpoint terlindungi memeriksa identitas, status aktif, izin. |
| AUTH-04  | Multi-role hanya jika disetujui (GAP-SRS-03).                       |
| FR-11-04 | Akun dirujuk audit → nonaktif, bukan hard delete.                   |
| FR-11-05 | Tolak penonaktifan admin aktif terakhir.                            |
| FR-11-07 | Username/email unik setelah normalisasi Identity.                   |
| FR-11-08 | Profil aman: tanpa PasswordHash, SecurityStamp, token.              |

## 3. Endpoint (SRS §7.2)

| Method | Route                  | Akses | Deskripsi         |
| ------ | ---------------------- | ----- | ----------------- |
| GET    | /api/users/me          | Aktif | Profil sendiri    |
| PATCH  | /api/users/me          | Aktif | Edit FullName     |
| GET    | /api/users             | W     | List akun         |
| GET    | /api/users/{id}        | W     | Detail akun       |
| POST   | /api/users             | W     | Create akun       |
| PATCH  | /api/users/{id}        | W     | Update profil     |
| PATCH  | /api/users/{id}/status | W     | Ubah status aktif |
| PATCH  | /api/users/{id}/roles  | W     | Assignment role   |

## 4. DTO Profil Aman

```json
{
  "Id": "...",
  "FullName": "...",
  "UserName": "...",
  "Email": "...",
  "IsActive": true,
  "Roles": ["scientis"]
}
```

## 5. Error

| HTTP | Code               | Kondisi                       |
| ---- | ------------------ | ----------------------------- |
| 400  | VALIDATION_ERROR   | Field invalid                 |
| 403  | FORBIDDEN          | scientis akses admin endpoint |
| 404  | RESOURCE_NOT_FOUND | User tidak ada                |
| 409  | DUPLICATE_RESOURCE | Username/email duplikat       |
| 409  | LAST_ADMIN         | Penonaktifan admin terakhir   |

6. Dependensi
   - D-11: Multi-role, DTO mapping Scientist.
   - GAP-SRS-03: Cardinality membership.
   - GAP-SRS-04: Casing response.
   - GAP-SRS-09: Pemetaan DTO Scientist.

---

## `specs/F-11-user-management/plan.md`

```markdown
# F-11 — Plan Implementasi

## Strategi

Integrasi ASP.NET Core Identity. User entity custom (`ApplicationUser`)
menambahkan FullName dan IsActive. Role management melalui Identity
RoleManager.

## Urutan

1. `ApplicationUser` + Identity configuration.
2. Seed data: default WarehouseAdmin (idempotent, NFR-08).
3. Profil sendiri (GET/PATCH /me).
4. Admin: list, create, update, status, roles.
5. Proteksi admin terakhir.
6. Controller + DTO.
7. Frontend user management (F-14).

## Risiko

| Risiko                    | Mitigasi                   |
| ------------------------- | -------------------------- |
| GAP-SRS-09: DTO Scientist | Refactor nama setelah D-11 |
| Seed hardcoded password   | Gunakan config/env, NFR-08 |
```

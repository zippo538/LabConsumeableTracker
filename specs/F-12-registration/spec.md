# F-12 — Register

## Prioritas: Must | SRS §4.12 | Trace: FR-12-01 s.d. FR-12-05, AUTH-02

## 1. Ringkasan

Registrasi publik menghasilkan akun aktif dengan role `scientis`. Tidak ada
pemilih role. Sukses → 201 tanpa JWT otomatis.

## 2. Aturan

| ID       | Aturan                                                |
| -------- | ----------------------------------------------------- |
| AUTH-02  | Registrasi mandiri → `scientis`.                      |
| FR-12-03 | Buat akun + assignment role = satu operasi konsisten. |
| FR-12-04 | Payload menyisipkan role/IsActive/UserId → 400.       |
| FR-12-05 | Sukses 201, tanpa JWT; UI → Login.                    |

## 3. Endpoint (SRS §7.2)

`POST /api/auth/register` — Publik

**Request:**

```json
{
  "FullName": "Budi Santoso",
  "UserName": "budi",
  "Email": "budi@lab.com",
  "Password": "P@ssw0rd!",
  "ConfirmPassword": "P@ssw0rd!"
}
```

**Response 201:**

```json
{
  "Id": "...",
  "FullName": "Budi Santoso",
  "UserName": "budi",
  "Email": "budi@lab.com",
  "IsActive": true,
  "Roles": ["scientis"]
}
```

## 4. Validasi (FR-12-02)

- FluentValidation: required, format email, password match.
- Identity: password policy, uniqueness username/email.
- Tolak field terlarang: role, IsActive, UserId, actor.

## 5. Error

| HTTP | Code               | Kondisi                        |
| ---- | ------------------ | ------------------------------ |
| 400  | VALIDATION_ERROR   | Field invalid, field terlarang |
| 409  | DUPLICATE_RESOURCE | Username/email duplikat        |

---

## `specs/F-12-registration/plan.md`

```markdown
# F-12 — Plan Implementasi

## Urutan

1. `RegisterCommand` + handler: create user via Identity, assign role `scientis`.
2. FluentValidation + Identity validation.
3. Transaksi: user + role = satu operasi; rollback jika role gagal (FR-12-03).
4. Controller `POST /api/auth/register`.
5. DTO + response aman.
6. Frontend register page (F-14).

## Risiko

| Risiko                               | Mitigasi                                                 |
| ------------------------------------ | -------------------------------------------------------- |
| Assignment role gagal → akun parsial | Gunakan transaction scope; rollback user jika role gagal |
```

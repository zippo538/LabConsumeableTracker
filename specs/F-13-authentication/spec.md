# F-13 — Login dan Akses Sesi

## Prioritas: Must | SRS §4.13 | Trace: FR-13-01 s.d. FR-13-08

## 1. Ringkasan

Login menghasilkan JWT Bearer. Endpoint terlindungi memvalidasi token,
status aktif, dan authorization. Logout adalah perilaku client.

## 2. Aturan

| ID       | Aturan                                                                                       |
| -------- | -------------------------------------------------------------------------------------------- |
| FR-13-01 | Login: UserName + Password → Identity verifikasi.                                            |
| FR-13-02 | Sukses: 200 JWT, Bearer, expiresAt, profil aman.                                             |
| FR-13-03 | Gagal: pesan umum, tidak ungkap keberadaan akun.                                             |
| FR-13-04 | Endpoint terlindungi: validasi signature, issuer, audience, lifetime, status, authorization. |
| FR-13-05 | Penonaktifan/role dicabut → izin lama tidak berlaku pada request berikutnya.                 |
| FR-13-06 | JWT invalid/expired → 401; valid tanpa izin → 403.                                           |
| FR-13-07 | Logout: bersihkan state client; tidak mengklaim revocation server.                           |

## 3. Endpoint (SRS §7.2)

`POST /api/auth/login` — Publik

**Request:**

```json
{ "UserName": "budi", "Password": "P@ssw0rd!" }
```

**Response:**

```json
{
  "token": "eyJ...",
  "tokenType": "Bearer",
  "expiresAt": "2026-09-11T09:00:00Z",
  "user": {
    "Id": "...",
    "FullName": "...",
    "UserName": "...",
    "Email": "...",
    "IsActive": true,
    "Roles": ["scientis"]
  }
}
```

## 4. Error

| HTTP Code | Status          | Kondisi                                                |
| --------- | --------------- | ------------------------------------------------------ |
| 401       | UNAUTHENTICATED | Kredensial salah, akun nonaktif, token invalid/expired |
| 403       | FORBIDDEN       | Identitas sah, izin tidak cukup                        |

## 5. Dependensi

- D-11: Lifetime, refresh, revocation, storage, password/lockout policy.

---

## `specs/F-13-authentication/plan.md`

```markdown
# F-13 — Plan Implementasi

## Urutan

1. Konfigurasi JWT Bearer di `Program.cs` (issuer, audience, key, lifetime).
2. `LoginCommand` handler — Identity SignInManager.
3. Generate JWT dengan claims: sub, name, role, SecurityStamp.
4. Middleware authorization: validasi token + status aktif + role.
5. Controller `POST /api/auth/login`.
6. Frontend login + token storage + logout (F-14).

## Risiko

| Risiko            | Mitigasi                                           |
| ----------------- | -------------------------------------------------- |
| D-11 belum final  | Default lifetime 1 jam, tanpa refresh              |
| Token storage XSS | sessionStorage untuk MVP; evaluasi httpOnly cookie |
```

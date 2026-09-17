# D-11 — Authentication, Session, dan Identity Policy

## Status: PARTIAL

## Konteks (SRS §4.11–4.13, §12.1)

Beberapa aspek Identity/JWT sudah final (register scientis, admin kelola user),
tetapi detail sesi masih terbuka.

## Sudah Final (DECIDED)

- Role kanonis: `scientis` dan `WarehouseAdmin` (AUTH-01).
- Registrasi mandiri → `scientis` (AUTH-02).
- JWT Bearer, ASP.NET Core Identity (NFR-01).
- Endpoint terlindungi memeriksa signature, issuer, audience, lifetime,
  status aktif, dan authorization (FR-13-04).

## Masih Terbuka (OPEN)

| #   | Aspek                  | Opsi                                            | Dampak              |
| --- | ---------------------- | ----------------------------------------------- | ------------------- |
| 1   | JWT lifetime           | 15m / 1h / 24h                                  | UX, security        |
| 2   | Refresh token          | Ya / Tidak                                      | Kompleksitas, UX    |
| 3   | Revocation mechanism   | Token blacklist / SecurityStamp / Tidak ada     | FR-13-05            |
| 4   | Token storage React    | localStorage / sessionStorage / httpOnly cookie | Security, XSS       |
| 5   | Password policy        | Default Identity / Custom                       | FR-12-02            |
| 6   | Lockout policy         | Ya (berapa attempt?) / Tidak                    | FR-13-03            |
| 7   | Pemetaan DTO Scientist | Refactor nama `Scientist` → `User` / tetap      | AUTH-05             |
| 8   | Multi-role cardinality | Satu role / banyak role                         | AUTH-04, GAP-SRS-03 |

## Usulan (PROPOSED)

- Lifetime: 1 jam, tanpa refresh token untuk MVP.
- Revocation: periksa `SecurityStamp` / `IsActive` setiap request.
- Token storage: `sessionStorage` (MVP).
- Password: default Identity (min 6 char, require digit).
- Lockout: 5 attempt, 15 menit.
- Multi-role: tidak untuk MVP; satu role per akun.

## Gate

- [ ] Client / Security / IT menyetujui lifetime dan storage.
- [ ] Lockout policy dikonfirmasi.
- [ ] DTO mapping Scientist diselaraskan (AUTH-05, GAP-SRS-09).

## Referensi SRS

AUTH-01 s.d. AUTH-06, FR-11-01 s.d. FR-11-08, FR-12-01 s.d. FR-12-05,
FR-13-01 s.d. FR-13-08, AC-F11-01 s.d. AC-F11-06, AC-F12-01 s.d. AC-F12-05,
AC-F13-01 s.d. AC-F13-04, GAP-SRS-03/04/09.

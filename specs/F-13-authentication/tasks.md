# F-13 — Task List

## Infrastructure

- [ ] T-13-01: [Infrastructure] `JwtSettings` configuration class (§9.2)
- [ ] T-13-02: [Infrastructure] Konfigurasi JWT Bearer authentication (NFR-01, NFR-07)
- [ ] T-13-03: [Infrastructure] `JwtTokenGenerator` — generate JWT dengan claims (FR-13-02)

## Application

- [ ] T-13-04: [Application] DTO `LoginRequestDto` + `LoginValidator` (FluentValidation)
- [ ] T-13-05: [Application] DTO `AuthResponseDto` (token, tokenType, expiresAt, user)
- [ ] T-13-06: [Application] Interface `IAuthenticationService`
- [ ] T-13-07: [Application] `AuthenticationService.LoginAsync` — verify credentials (FR-13-01)
- [ ] T-13-08: [Application] Pesan gagal umum — tidak ungkap akun (FR-13-03)

## API

- [ ] T-13-09: [API] Endpoint `POST /api/auth/login` (§7.2)
- [ ] T-13-10: [API] Authorization middleware — validasi token + aktif + role (FR-13-04)
- [ ] T-13-11: [API] Pemeriksaan status aktif setiap request (FR-13-05)

## Tests

- [ ] T-13-12: [Tests/Integration] Integration test login sukses (AC-F13-01)
- [ ] T-13-13: [Tests/Integration] Integration test login gagal (AC-F13-02)
- [ ] T-13-14: [Tests/Integration] Integration test token invalid/expired (AC-F13-03)
- [ ] T-13-15: [Tests/Integration] Integration test role dicabut pada JWT terbit (AC-F11-06)

## [BLOCKED]

- [BLOCKED] T-13-16: [Infrastructure] Finalisasi lifetime/refresh/lockout setelah D-11

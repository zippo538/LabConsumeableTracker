# F-12 — Task List

## Application

- [ ] T-12-01: [Application] DTO `RegisterRequestDto` + `RegisterValidator` (FR-12-01, FR-12-02)
- [ ] T-12-02: [Application] Tolak field terlarang (role, IsActive, UserId) (FR-12-04)
- [ ] T-12-03: [Application] Interface `IRegistrationService`
- [ ] T-12-04: [Application] `RegistrationService.RegisterAsync` — create user + assign `scientis` (FR-12-03, AUTH-02)
- [ ] T-12-05: [Application] Transaksi atomik: user + role (FR-12-03, AC-F12-05)
- [ ] T-12-06: [Application] Response profil aman tanpa JWT (FR-12-05)

## API

- [ ] T-12-07: [API] Endpoint `POST /api/auth/register` → 201 / 400 / 409 (§7.2)
- [ ] T-12-08: [API] Authorization: Publik

## Tests

- [ ] T-12-09: [Tests/Integration] Integration test register valid (AC-F12-01)
- [ ] T-12-10: [Tests/Integration] Integration test register invalid (AC-F12-02)
- [ ] T-12-11: [Tests/Integration] Integration test register duplikat (AC-F12-03)
- [ ] T-12-12: [Tests/Integration] Integration test privilege escalation (AC-F12-04)
- [ ] T-12-13: [Tests/Integration] Integration test role assignment gagal (AC-F12-05)

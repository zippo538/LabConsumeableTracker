# F-11 — Task List

## Infrastructure

- [ ] T-11-01: [Infrastructure] `ApplicationUser` extends `IdentityUser` + FullName, IsActive (FR-11-07)
- [ ] T-11-02: [Infrastructure] Konfigurasi Identity di `Program.cs` (NFR-01)
- [ ] T-11-03: [Infrastructure] Seed role `scientis` dan `WarehouseAdmin` (AUTH-01)
- [ ] T-11-04: [Infrastructure] Seed default WarehouseAdmin (idempotent, NFR-08)

## Application

- [ ] T-11-05: [Application] DTO `UserProfileResponse`, `UpdateProfileRequest`, `CreateUserRequest`, `UserResponse`
- [ ] T-11-06: [Application] `UpdateProfileValidator`, `CreateUserValidator` (FluentValidation)
- [ ] T-11-07: [Application] Interface `IUserService`
- [ ] T-11-08: [Application] `UserService.GetProfileAsync` (FR-11-01)
- [ ] T-11-09: [Application] `UserService.UpdateProfileAsync` — FullName only (FR-11-01)
- [ ] T-11-10: [Application] `UserService.ListAsync` (FR-11-02)
- [ ] T-11-11: [Application] `UserService.CreateAsync` (FR-11-02)
- [ ] T-11-12: [Application] `UserService.UpdateAsync` (FR-11-02)
- [ ] T-11-13: [Application] `UserService.UpdateStatusAsync` (FR-11-03)
- [ ] T-11-14: [Application] `UserService.UpdateRolesAsync` (FR-11-03)
- [ ] T-11-15: [Application] Proteksi admin aktif terakhir (FR-11-05)
- [ ] T-11-16: [Application] DTO profil aman — tanpa PasswordHash/SecurityStamp (FR-11-08)
- [ ] T-11-17: [Application] AutoMapper profile

## API

- [ ] T-11-18: [API] `UsersController` — semua endpoint (§7.2)
- [ ] T-11-19: [API] Authorization policy per role (§3.1)

## Tests

- [ ] T-11-20: [Tests/Integration] Integration test RBAC (AC-F11-02, AC-F11-03)
- [ ] T-11-21: [Tests/Integration] Integration test admin terakhir (AC-F11-05)
- [ ] T-11-22: [Tests/Integration] Integration test role dicabut pada JWT terbit (AC-F11-06)

## [BLOCKED]

- [BLOCKED] T-11-23: [Application] Finalisasi multi-role setelah D-11/GAP-SRS-03
- [BLOCKED] T-11-24: [Application] Refactor DTO Scientist setelah GAP-SRS-09

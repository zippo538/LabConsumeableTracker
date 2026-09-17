# F-11 — Plan Implementasi

## Strategi

Integrasi ASP.NET Core Identity. `ApplicationUser` extends `IdentityUser`
dengan `FullName` dan `IsActive`. Role management melalui Identity `RoleManager`.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-11                                                                               |
| ------------------ | ------------------------------------------- | ------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | Tidak ada entity baru; menggunakan `ApplicationUser` dari Infrastructure                    |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `Identity/ApplicationUser.cs`, `Identity/UserSeeder.cs`, `Identity/IdentityService.cs`      |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/UserService.cs`, `Interfaces/IUserService.cs`, `DTOs/Users/`, `Validators/Users/` |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/UsersController.cs`                                                            |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Integration/Services/UserServiceTests.cs`, `Integration/Api/RbacTests.cs`                  |

## Urutan Implementasi

1. **[Infrastructure]** `ApplicationUser` extends `IdentityUser` + FullName, IsActive (FR-11-07)
2. **[Infrastructure]** Konfigurasi Identity di `Program.cs` (NFR-01)
3. **[Infrastructure]** Seed role `scientis` dan `WarehouseAdmin` (AUTH-01)
4. **[Infrastructure]** Seed default WarehouseAdmin (idempotent, NFR-08)
5. **[Application]** DTO: `UserProfileResponse`, `UpdateProfileRequest`, `CreateUserRequest`, `UserResponse`
6. **[Application]** `UpdateProfileValidator`, `CreateUserValidator` (FluentValidation)
7. **[Application]** Interface `IUserService`
8. **[Application]** `UserService.GetProfileAsync` — GET /api/users/me (FR-11-01)
9. **[Application]** `UserService.UpdateProfileAsync` — PATCH /api/users/me, FullName only (FR-11-01)
10. **[Application]** `UserService.ListAsync` — GET /api/users (FR-11-02)
11. **[Application]** `UserService.CreateAsync` — POST /api/users (FR-11-02)
12. **[Application]** `UserService.UpdateAsync` — PATCH /api/users/{id} (FR-11-02)
13. **[Application]** `UserService.UpdateStatusAsync` — PATCH /api/users/{id}/status (FR-11-03)
14. **[Application]** `UserService.UpdateRolesAsync` — PATCH /api/users/{id}/roles (FR-11-03)
15. **[Application]** Proteksi admin aktif terakhir (FR-11-05)
16. **[Application]** DTO profil aman — tanpa PasswordHash/SecurityStamp (FR-11-08)
17. **[Application]** AutoMapper profile
18. **[API]** `UsersController` — semua endpoint (§7.2)
19. **[API]** Authorization policy per role (§3.1)
20. **[Tests/Integration]** Integration test RBAC (AC-F11-02, AC-F11-03)
21. **[Tests/Integration]** Integration test admin terakhir (AC-F11-05)
22. **[Web]** Halaman user management (F-14)

## Risiko

| Risiko                    | Mitigasi                              |
| ------------------------- | ------------------------------------- |
| GAP-SRS-09: DTO Scientist | Refactor nama setelah D-11            |
| Seed hardcoded password   | Gunakan config/env, NFR-08            |
| D-11: Multi-role          | Default: satu role per akun untuk MVP |

## Referensi

FR-11-01 s.d. FR-11-08, AUTH-01 s.d. AUTH-06, NFR-01/NFR-08, `specs/architecture.md`.

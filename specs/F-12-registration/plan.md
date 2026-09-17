# F-12 — Plan Implementasi

## Strategi

Registrasi publik menggunakan ASP.NET Core Identity. Transaksi atomik:
create user + assign role `scientis`. Jika assign role gagal, rollback user.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-12                                                                                                                                      |
| ------------------ | ------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | Tidak ada entity baru                                                                                                                              |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | Menggunakan `ApplicationUser` dan Identity existing                                                                                                |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/RegistrationService.cs`, `Interfaces/IRegistrationService.cs`, `DTOs/Auth/RegisterRequestDto.cs`, `Validators/Auth/RegisterValidator.cs` |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/AuthController.cs` — `POST /api/auth/register`                                                                                        |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Integration/Services/RegistrationServiceTests.cs`                                                                                                 |

## Urutan Implementasi

1. **[Application]** DTO `RegisterRequestDto` + `RegisterValidator` (FluentValidation) (FR-12-01, FR-12-02)
2. **[Application]** Tolak field terlarang: role, IsActive, UserId (FR-12-04)
3. **[Application]** Interface `IRegistrationService`
4. **[Application]** `RegistrationService.RegisterAsync` — create user + assign `scientis` (FR-12-03, AUTH-02)
5. **[Application]** Transaksi atomik: user + role (FR-12-03, AC-F12-05)
6. **[Application]** Response profil aman tanpa JWT (FR-12-05)
7. **[API]** Endpoint `POST /api/auth/register` → 201 / 400 / 409 (§7.2)
8. **[API]** Authorization: Publik
9. **[Tests/Integration]** Integration test register valid (AC-F12-01)
10. **[Tests/Integration]** Integration test register invalid (AC-F12-02)
11. **[Tests/Integration]** Integration test register duplikat (AC-F12-03)
12. **[Tests/Integration]** Integration test privilege escalation (AC-F12-04)
13. **[Tests/Integration]** Integration test role assignment gagal (AC-F12-05)
14. **[Web]** Halaman register (F-14)

## Risiko

| Risiko                               | Mitigasi                                                     |
| ------------------------------------ | ------------------------------------------------------------ |
| Assignment role gagal → akun parsial | Gunakan transaction scope; rollback user jika role gagal     |
| Password policy                      | Default Identity (min 6 char, require digit) — menunggu D-11 |

## Referensi

FR-12-01 s.d. FR-12-05, AUTH-02, NFR-01, `specs/architecture.md`.

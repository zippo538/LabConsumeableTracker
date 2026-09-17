# F-13 — Plan Implementasi

## Strategi

Login menggunakan ASP.NET Core Identity `SignInManager`. JWT di-generate
dengan claims role + SecurityStamp. Authorization middleware memvalidasi
token + status aktif + role setiap request.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-13                                                                                                                                                                    |
| ------------------ | ------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | Tidak ada entity baru                                                                                                                                                            |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `Identity/JwtTokenGenerator.cs`, `Configurations/JwtSettings.cs`                                                                                                                 |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/AuthenticationService.cs`, `Interfaces/IAuthenticationService.cs`, `DTOs/Auth/LoginRequestDto.cs`, `DTOs/Auth/AuthResponseDto.cs`, `Validators/Auth/LoginValidator.cs` |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/AuthController.cs` — `POST /api/auth/login`, middleware authorization                                                                                               |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Integration/Services/AuthenticationServiceTests.cs`, `Integration/Api/AuthTests.cs`                                                                                             |

## Urutan Implementasi

1. **[Infrastructure]** `JwtSettings` configuration class (§9.2)
2. **[Infrastructure]** Konfigurasi JWT Bearer authentication di `Program.cs` (NFR-01, NFR-07)
3. **[Infrastructure]** `JwtTokenGenerator` — generate JWT dengan claims role + SecurityStamp (FR-13-02)
4. **[Application]** DTO `LoginRequestDto` + `LoginValidator` (FluentValidation)
5. **[Application]** DTO `AuthResponseDto` (token, tokenType, expiresAt, user)
6. **[Application]** Interface `IAuthenticationService`
7. **[Application]** `AuthenticationService.LoginAsync` — verify credentials (FR-13-01)
8. **[Application]** Pesan gagal umum — tidak ungkap akun (FR-13-03)
9. **[API]** Endpoint `POST /api/auth/login` (§7.2)
10. **[API]** Authorization middleware — validasi token + aktif + role (FR-13-04)
11. **[API]** Pemeriksaan status aktif setiap request (FR-13-05)
12. **[Tests/Integration]** Integration test login sukses (AC-F13-01)
13. **[Tests/Integration]** Integration test login gagal (AC-F13-02)
14. **[Tests/Integration]** Integration test token invalid/expired (AC-F13-03)
15. **[Tests/Integration]** Integration test role dicabut pada JWT terbit (AC-F11-06)
16. **[Web]** Halaman login + token storage + logout (F-14)

## Risiko

| Risiko              | Mitigasi                                           |
| ------------------- | -------------------------------------------------- |
| D-11 belum final    | Default lifetime 1 jam, tanpa refresh              |
| Token storage XSS   | sessionStorage untuk MVP; evaluasi httpOnly cookie |
| SecurityStamp check | Periksa setiap request via middleware              |

## Referensi

FR-13-01 s.d. FR-13-08, NFR-01/NFR-07, `specs/architecture.md`.

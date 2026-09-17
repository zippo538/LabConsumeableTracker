# F-02 — Plan Implementasi

## Strategi

Expiry engine adalah domain service murni (NFR-03). `ExpiryEvaluator` dan
`EligibilityChecker` tidak bergantung DB/UI. Waktu `now` di-inject dari
`TimeProvider` pada application boundary.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-02                                                                                                               |
| ------------------ | ------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | `Services/ExpiryEvaluator.cs` (pure function), `Services/EligibilityChecker.cs`, `Constants/ExpiryRules.cs`                 |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `Services/ExpiryPolicy.cs` (implementasi `IExpiryPolicy` dari D-05), `Services/DateTimeService.cs` (wrapper `TimeProvider`) |
| **Application**    | `LabConsumableExpiryTracker.Application`    | Dipanggil oleh `LotService`, `StockService`, `LotSelectionService`, `ConsumptionService` — tidak ada service khusus F-02    |
| **API**            | `LabConsumableExpiryTracker.API`            | Tidak ada endpoint khusus F-02; expiry dievaluasi pada response endpoint lain                                               |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Unit/Domain/ExpiryEvaluatorTests.cs`, `Unit/Domain/EligibilityCheckerTests.cs`                                             |

## Urutan Implementasi

1. **[Domain]** `ExpiryEvaluator.Evaluate(Lot, DateTime now, int expiringSoonDays)` — pure function
2. **[Domain]** `EligibilityChecker.IsEligible(Lot, DateTime now)` — pure function
3. **[Infrastructure]** `IExpiryPolicy` interface di Domain
4. **[Infrastructure]** `ExpiryPolicy` implementasi (D-05)
5. **[Infrastructure]** `DateTimeService` wrapper `TimeProvider`
6. **[Infrastructure]** Registrasi `TimeProvider.System` di DI (`Program.cs`)
7. **[Tests/Unit]** Unit test boundary Valid/ExpiringSoon/Expired
8. **[Tests/Unit]** Unit test eligibility kombinasi status + remaining + expiry
9. **[Application]** Integrasi `ExpiryEvaluator` ke read model Lot response (FR-03-01)
10. **[Application]** Integrasi ke `LotSelectionService` (F-05)
11. **[Application]** Integrasi ke `ConsumptionService` (F-06)

## Risiko

| Risiko                            | Mitigasi                                                       |
| --------------------------------- | -------------------------------------------------------------- |
| D-05 belum final                  | Gunakan interface `IExpiryPolicy` agar mudah swap implementasi |
| Timezone salah                    | Semua `now` dari `TimeProvider.GetUtcNow()`                    |
| Evaluasi bocor ke client          | Frontend hanya menerima `ExpiryCondition` dari server (UI-13)  |
| Boundary test tidak deterministik | Unit test dengan `DateTime` eksplisit, bukan `DateTime.UtcNow` |

## Referensi

FR-02-01 s.d. FR-02-06, BR-06/08, NFR-03, `specs/architecture.md`.

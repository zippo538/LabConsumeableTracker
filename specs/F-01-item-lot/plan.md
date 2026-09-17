# F-01 — Plan Implementasi

## Strategi

Implementasi bertahap mengikuti dependency flow Clean Architecture:
**Domain → Infrastructure → Application → API → Tests → Web**.
Kontrak OpenAPI harus disepakati sebelum frontend dimulai (GAP-SRS-01).

## Pemetaan Layer (sesuai `architecture.md`)

| Layer              | Proyek                                      | Komponen untuk F-01                                                                                                                                                                               |
| ------------------ | ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | `Entities/Item.cs`, `Entities/Lot.cs`, `Enums/UnitOfMeasure.cs`, `Enums/LotAdministrativeStatus.cs`, `Interfaces/IItemRepository.cs`, `Interfaces/ILotRepository.cs`, `Interfaces/IUnitOfWork.cs` |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | `Persistence/Configurations/ItemConfiguration.cs`, `LotConfiguration.cs`, `Repositories/ItemRepository.cs`, `LotRepository.cs`, `UnitOfWork.cs`, `Migrations/`                                    |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/ItemService.cs`, `LotService.cs`, `Interfaces/IItemService.cs`, `ILotService.cs`, `DTOs/Items/`, `DTOs/Lots/`, `Validators/Items/`, `Validators/Lots/`, `Mappings/MappingProfile.cs`    |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/ItemsController.cs`, `LotsController.cs`                                                                                                                                             |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Unit/Domain/ItemTests.cs`, `Unit/Services/ItemServiceTests.cs`, `Integration/Repositories/ItemRepositoryTests.cs`                                                                                |
| **Web**            | `LabConsumableExpiryTracker.Web`            | `src/features/items/`, `src/features/lots/`                                                                                                                                                       |

## Urutan Implementasi

1. **[Domain]** Entity `Item`, `Lot` + enum + interface repository
2. **[Domain]** Invariant: `RemainingQuantity = InitialQuantity` saat create
3. **[Infrastructure]** `ItemConfiguration` + `LotConfiguration` (EF Core fluent)
4. **[Infrastructure]** Migration `CreateItemsAndLots`
5. **[Infrastructure]** `ItemRepository`, `LotRepository`, `UnitOfWork`
6. **[Application]** DTO: `CreateItemRequest`, `ItemResponse`, `RegisterLotRequest`, `LotResponse`
7. **[Application]** Validator: `CreateItemValidator`, `RegisterLotValidator`
8. **[Application]** `ItemService` + `LotService` (orkestrasi use case)
9. **[Application]** AutoMapper `MappingProfile`
10. **[API]** `ItemsController` + `LotsController` + authorization policy
11. **[API]** Exception handling global + pemetaan `Result<T>` → HTTP
12. **[Tests]** Unit test invariant domain
13. **[Tests]** Integration test repository di PostgreSQL
14. **[Tests]** Integration test concurrency RowVersion
15. **[Web]** Halaman Item/Lot (setelah F-14 shell siap)

## Risiko

| Risiko                          | Mitigasi                                                                  |
| ------------------------------- | ------------------------------------------------------------------------- |
| GAP-SRS-01: kontrak belum final | Alignment meeting sebelum sprint API                                      |
| GAP-SRS-02: penonaktifan Item   | Tambahkan `IsActive` di Domain; refactor jika D-04 mengubah               |
| D-04 belum final                | Default sementara `Guid` dan `decimal(18,4)`                              |
| Kebocoran EF ke Application     | Review PR: tidak ada `using Microsoft.EntityFrameworkCore` di Application |

## Referensi

NFR-01 s.d. NFR-06, §9.1 Tanggung jawab implementasi, §9.2 Penempatan berkas,
`specs/architecture.md`.

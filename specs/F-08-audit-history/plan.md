# F-08 — Plan Implementasi

## Strategi

Audit adalah read-only projection dari tabel Consumption dan Disposal.
Endpoint terpadu (`/api/audit-events`) menggunakan UNION atau query gabungan.

## Pemetaan Layer

| Layer              | Proyek                                      | Komponen F-08                                                            |
| ------------------ | ------------------------------------------- | ------------------------------------------------------------------------ |
| **Domain**         | `LabConsumableExpiryTracker.Domain`         | Tidak ada entity baru; menggunakan `Consumption` dan `Disposal` existing |
| **Infrastructure** | `LabConsumableExpiryTracker.Infrastructure` | Query EF Core untuk audit terpadu dan terpisah                           |
| **Application**    | `LabConsumableExpiryTracker.Application`    | `Services/AuditService.cs`, `Interfaces/IAuditService.cs`, `DTOs/Audit/` |
| **API**            | `LabConsumableExpiryTracker.API`            | `Controllers/AuditController.cs`                                         |
| **Tests**          | `LabConsumableExpiryTracker.Tests`          | `Integration/Services/AuditServiceTests.cs`                              |

## Urutan Implementasi

1. **[Application]** DTO: `ConsumptionResponse`, `DisposalResponse`, `AuditEventResponse`, `AuditFilterRequest`
2. **[Application]** Interface `IAuditService`
3. **[Application]** `AuditService.GetConsumptionsAsync` — filter + pagination (FR-08-01, FR-08-03)
4. **[Application]** `AuditService.GetDisposalsAsync` — filter + pagination (FR-08-01, FR-08-03)
5. **[Application]** `AuditService.GetAuditEventsAsync` — gabungan Consumption + Disposal (FR-08-01)
6. **[Application]** Validasi filter: Disposal + JobId → 400 (FR-08-04)
7. **[Application]** Detail Consumption + Disposal (FR-08-02)
8. **[Application]** Pastikan histori tetap terbaca saat Lot expired / actor nonaktif (FR-08-06)
9. **[API]** `AuditController` — semua endpoint (§7.8)
10. **[API]** Authorization policy: S/W
11. **[Infrastructure]** Index database untuk performa audit (NFR-15)
12. **[Tests/Integration]** Integration test filter + pagination (AC-F08-01, AC-API-05)
13. **[Web]** Halaman audit (F-14)

## Risiko

| Risiko                     | Mitigasi                                    |
| -------------------------- | ------------------------------------------- |
| Performance query gabungan | Index pada `OccurredAt`, `ItemId`, `LotId`  |
| Actor dinonaktifkan        | JOIN ke User tetap, tampilkan nama terakhir |
| D-10: Retention audit      | Tunggu keputusan final                      |

## Referensi

FR-08-01 s.d. FR-08-07, BR-15, NFR-15, `specs/architecture.md`.

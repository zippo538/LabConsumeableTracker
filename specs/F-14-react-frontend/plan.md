# F-14 — Plan Implementasi

## Strategi

Frontend dibangun setelah API stabil. Gunakan adapter HTTP tunggal (UI-13).
Mock hanya untuk development awal; acceptance memerlukan API nyata (FR-14-02).

## Pemetaan Layer

| Layer   | Proyek                           | Komponen F-14          |
| ------- | -------------------------------- | ---------------------- |
| **Web** | `LabConsumableExpiryTracker.Web` | Seluruh frontend React |

## Struktur Folder Web

LabConsumableExpiryTracker.Web/
├── src/
│ ├── app/ # Router, shell, providers
│ ├── features/
│ │ ├── auth/ # Register, Login, Logout
│ │ ├── dashboard/ # Dashboard summary
│ │ ├── items/ # Item list, detail, create/edit
│ │ ├── lots/ # Lot list, detail, registrasi
│ │ ├── jobs/ # Job list, detail, lifecycle
│ │ ├── consumption/ # Preview FEFO + consume
│ │ ├── disposal/ # Disposal + status actions
│ │ ├── audit/ # Audit history
│ │ └── users/ # Profile + user management
│ ├── components/ # Shared components
│ ├── layouts/ # Shell, sidebar
│ ├── services/ # HTTP adapter tunggal (UI-13)
│ ├── hooks/ # Custom hooks
│ ├── utils/ # Utility functions
│ ├── styles/ # Global styles
│ └── tests/ # Frontend tests
├── package.json
└── vite.config.ts

## Urutan Implementasi

1. **[Web]** Inisialisasi project React + router + HTTP client (FR-14-01)
2. **[Web]** Adapter HTTP tunggal ke ASP.NET Core API (UI-13)
3. **[Web]** Konfigurasi auth context + token storage (UI-17)
4. **[Web]** Halaman Register + validasi (UI-01, AC-FE-03)
5. **[Web]** Halaman Login + error handling (UI-02, AC-FE-04)
6. **[Web]** Logout + bersihkan state (UI-16, AC-F13-04)
7. **[Web]** Route guard — redirect ke Login (AC-FE-01)
8. **[Web]** Shell layout + sidebar + navigasi role-based (FE-02, AC-FE-02)
9. **[Web]** Dashboard summary cards (FE-03, AC-FE-05)
10. **[Web]** Loading/empty/error states (UI-18 s.d. UI-22)
11. **[Web]** Item list + detail (FE-04, AC-FE-07)
12. **[Web]** Lot list + detail (FE-05, AC-FE-08)
13. **[Web]** Form create/edit Item (admin only) (UI-04)
14. **[Web]** Form registrasi Lot (admin only) (UI-04)
15. **[Web]** Job list + detail + lifecycle buttons (FE-06, AC-FE-09)
16. **[Web]** Consume page: preview FEFO + konfirmasi (FE-07, AC-FE-10, AC-FE-11)
17. **[Web]** Conflict handling consume (UI-10, AC-FE-12)
18. **[Web]** Disposal dialog (FE-08, UI-11, AC-FE-13)
19. **[Web]** Status actions (Block/Quarantine/Release) (UI-11)
20. **[Web]** Audit page + filter + pagination (FE-09, AC-FE-14)
21. **[Web]** Profile page (FE-10, AC-F11-01)
22. **[Web]** User management pages (FE-11, AC-FE-15)
23. **[Web]** Responsivitas 360/768/1280 (UI-18, UI-19, AC-FE-17)
24. **[Web]** Keyboard navigation + fokus (UI-21, AC-FE-17)
25. **[Web]** Label + error + kontras (UI-20, UI-22)
26. **[Web]** Race condition search (UI-15, AC-FE-18)

## Risiko

| Risiko           | Mitigasi                          |
| ---------------- | --------------------------------- |
| API belum stabil | Contract testing; mock sementara  |
| Token storage    | sessionStorage MVP; evaluasi D-11 |
| Responsivitas    | Test 360/768/1280 dari awal       |

## Referensi

FR-14-01 s.d. FR-14-04, UI-01 s.d. UI-23, §8 Frontend, `specs/architecture.md`.

# Specs Driven Development — Lab Consumable Expiry Tracker

## 1. Tujuan

Repositori `specs/` merupakan sumber kebenaran tunggal antara SRS dan kode.
Setiap fitur MVP (F-01 s.d. F-17) memiliki folder sendiri yang berisi:

| Berkas          | Isi                                                               |
| --------------- | ----------------------------------------------------------------- |
| `spec.md`       | Kontrak perilaku: aturan domain, endpoint, DTO, error, invariant. |
| `plan.md`       | Strategi implementasi: lapisan, dependensi, urutan, risiko.       |
| `tasks.md`      | Checklist task granular yang dapat di-assign ke sprint.           |
| `acceptance.md` | Kriteria penerimaan terverifikasi (Given/When/Then).              |

## 2. Cara Kerja

1. **Spec-first** — `spec.md` ditulis dan direview SEBELUM kode.
2. **Decision gating** — Folder `decisions/` menyimpan keputusan terbuka
   (D-05, D-06, D-08, D-11). Task yang bergantung pada keputusan terbuka
   ditandai `[BLOCKED]` dan tidak boleh dikerjakan sampai keputusan final.
3. **Task decomposition** — `tasks.md` memecah spec menjadi unit kerja
   ≤ 1 hari. Setiap task merujuk FR/BR/NFR di SRS.
4. **Acceptance-driven** — `acceptance.md` berisi skenario uji yang harus
   hijau sebelum fitur dinyatakan Done.
5. **Traceability** — Setiap spec/plan/task/acceptance mencantumkan ID SRS
   (FR-xx-yy, BR-xx, NFR-xx, UI-xx, AC-xx).

## 3. Konvensi

- Bahasa: Indonesia (konsisten dengan SRS).
- Prioritas: `Must` = gate MVP, `Should` = gate release jika masuk scope,
  `Nice` = fase lanjut.
- Status task: `[ ]` belum mulai, `[~]` in progress, `[x]` selesai,
  `[BLOCKED]` menunggu keputusan.
- Role kanonis: `scientis` (huruf kecil) dan `WarehouseAdmin`.
- Semua timestamp UTC; tampilan lokal menyebut zona waktu.

## 4. Struktur Folder

specs/
├── README.md ← Anda di sini
├── decisions/ ← Keputusan terbuka yang memengaruhi implementasi
│ ├── D-05-expiry-cutoff.md
│ ├── D-06-low-stock-threshold.md
│ ├── D-08-lot-status-transition.md
│ └── D-11-auth-session-policy.md
├── F-01-item-lot/
├── F-02-expiry-eligibility/
├── F-03-stock-dashboard/
├── F-04-job-lifecycle/
├── F-05-fefo-allocation/
├── F-06-consumption/
├── F-07-disposal-status/
├── F-08-audit-history/
├── F-11-user-management/
├── F-12-registration/
├── F-13-authentication/
├── F-14-react-frontend/
├── F-15-scanner-verificaiton/
├── F-16-lot-qr-label/
└── F-17-qr-camera-scan/

## 5. Referensi

| Dokumen | Versi | Lokasi                                       |
| ------- | ----- | -------------------------------------------- |
| PRD     | v1.4  | `PRD_Lab_Consumable_Expiry_Tracker_v1_4.pdf` |
| SRS     | v1.0  | `srs4.md`                                    |
| Diagram | —     | `diagram.mmd`                                |
| Agent   | —     | `agen2.md`                                   |
| Skill   | —     | `SKILL.md`                                   |

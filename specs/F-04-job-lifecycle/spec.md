# F-04 — Job Lifecycle

## Prioritas: Must | SRS §4.4 | Trace: FR-04-01 s.d. FR-04-06

## 1. Ringkasan

Scientis membuat Job (Draft), memulai (InProgress), menyelesaikan (Completed),
dan membatalkan (Cancelled). Transisi ilegal ditolak. Lifecycle harus aman
terhadap operasi bersamaan.

## 2. State Machine

     Create
     ──────────────→ Draft
     │
     Start │ Cancel (D-07)
     ↓ ↓
     InProgress Cancelled
     │
     Complete │ Cancel (D-07)
     ↓ ↓

| From                | To         | Trigger  | Status                 |
| ------------------- | ---------- | -------- | ---------------------- |
| —                   | Draft      | Create   | DECIDED                |
| Draft               | InProgress | Start    | DECIDED                |
| InProgress          | Completed  | Complete | DECIDED                |
| Draft/InProgress    | Cancelled  | Cancel   | PROPOSED D-07          |
| Completed/Cancelled | *          | Reopen   | Tidak diizinkan (D-07) |

## 3. Aturan

| ID       | Aturan                                            |
| -------- | ------------------------------------------------- |
| FR-04-02 | Create → Draft; StartedAt dan CompletedAt null.   |
| FR-04-03 | Start → InProgress; StartedAt = server time.      |
| FR-04-04 | Complete → Completed; CompletedAt = server time.  |
| FR-04-05 | Transisi tidak sah ditolak tanpa perubahan state. |
| FR-04-06 | Validasi lifecycle aman terhadap concurrency.     |

## 4. Endpoint (SRS §7.4)

| Method | Route                      | Akses | Body                           | Response                        |
| ------ | -------------------------- | ----- | ------------------------------ | ------------------------------- |
| GET    | /api/jobs                  | S     | Search, Status, Page, PageSize | 200 list                        |
| GET    | /api/jobs/{jobId}          | S     | —                              | 200 detail + ringkasan konsumsi |
| POST   | /api/jobs                  | S     | JobNumber                      | 201 Draft                       |
| POST   | /api/jobs/{jobId}/start    | S     | —                              | 200 InProgress                  |
| POST   | /api/jobs/{jobId}/complete | S     | —                              | 200 Completed                   |
| POST   | /api/jobs/{jobId}/cancel   | S     | —                              | 200 Cancelled (D-07)            |

## 5. Error

| HTTP | Code               | Kondisi                   |
| ---- | ------------------ | ------------------------- |
| 400  | VALIDATION_ERROR   | JobNumber invalid         |
| 404  | RESOURCE_NOT_FOUND | Job tidak ada             |
| 409  | INVALID_JOB_STATE  | Transisi tidak sah        |
| 409  | DUPLICATE_RESOURCE | JobNumber duplikat (D-04) |

## 6. Dependensi

- D-04: Validasi JobNumber, uniqueness.
- D-07: Cancel transition, terminal state, stock reversal.
- GAP-SRS-06: Race lifecycle concurrency mechanism.

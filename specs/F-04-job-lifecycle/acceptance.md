# F-04 — Acceptance Criteria

### AC-F04-01: Start Job

**Given** Job Draft
**When** Start
**Then** Status = InProgress dan StartedAt terisi waktu server.

### AC-F04-02: Complete Job

**Given** Job InProgress
**When** Complete
**Then** Status = Completed dan CompletedAt terisi waktu server.

### AC-API-02: Lifecycle + Race

**Given** Job berbagai state
**When** lifecycle dijalankan
**Then** hanya transisi sah; race consume/complete/cancel tidak melanggar policy.

### AC-FE-09: UI Job Actions

**Given** Job berbagai state
**When** scientis melihat aksi
**Then** hanya transisi sah sesuai policy backend tersedia.

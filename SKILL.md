---
description: Implement or review the .NET 8 Lab Consumable Expiry
  Tracker application according to PRD v1.5, SRS , and agent.md
  . Preserve the canonical Item-Lot inventory model, FEFO
  allocation, expiry eligibility, role security, atomic transactions,
  auditability, and Clean Architecture rules.
name: consumabletracker
---

# Lab Consumable Expiry Tracker

## Source of Truth

Use these sources according to their responsibility:

1.  `PRD_Lab_Consumable_Expiry_Tracker_v1_5.pdf` --- product goals,
    scope, user stories, features, acceptance criteria, and product
    decisions.
2.  `srs.md` --- implementation requirements, requirement IDs,
    traceability, architecture boundaries, and testing expectations.
3.  `agent.md` --- agent workflow, source reconciliation, and
    implementation constraints.
4.  `diagram.mmd` --- canonical domain classes, members, relationships,
    and enums when available.

When sources conflict: - Do not silently choose a source. - Report the
conflict and isolate dependent changes. - Follow explicit latest owner
decisions recorded in SRS/agent instructions.

## Product Goal

Build a laboratory consumable management system that prevents expired or
invalid materials from being used in new Jobs, recommends eligible
inventory using FEFO, records Consumption and Disposal atomically,
maintains audit history, and provides accurate usable-stock and
low-stock information.

## Technology Baseline

The implementation target is:

-   Backend: ASP.NET Core Web API (.NET 8)
-   Frontend: React
-   Database: PostgreSQL
-   ORM: Entity Framework Core
-   Identity: ASP.NET Core Identity
-   Authentication: JWT Bearer

Use Repository Pattern, Dependency Injection, DTOs, AutoMapper, and
FluentValidation where defined by the project structure.

## Core Inventory Rule: Item-Lot Model

`Lot` is the smallest tracked inventory unit.

Rules: - `Item` owns zero or more `Lot` entities directly. - Never add
inventory layers below Lot. - Never add lot splitting. - Different
expiry dates must be represented by different Lot records. -
Consumption, Disposal, and LotAllocation always reference `LotId`.

A Lot contains: - Id - ItemId - LotNumber - InitialQuantity -
RemainingQuantity - ExpiryDate - ReceivedAt - AdministrativeStatus -
StorageLocation - RowVersion

Never recalculate InitialQuantity after transactions.

## Canonical Entities

### Item

Properties: - Id - Code - Name - BaseUnit - MinimumStock -
ExpiringSoonDays

`IsLowStock(totalQuantity)` evaluates usable stock against MinimumStock.

### Lot

Responsibilities: - Evaluate expiry condition. - Determine
eligibility. - Consume stock and create Consumption records. - Dispose
stock and create Disposal records. - Block administrative state.

AdministrativeStatus: - Active - Quarantined - ManuallyBlocked -
Disposed

ExpiryCondition: - Valid - ExpiringSoon - Expired

Do not combine expiry state, administrative state, and low-stock
condition.

## Eligibility Rules

A Lot is eligible only when:

-   AdministrativeStatus == Active
-   RemainingQuantity \> 0
-   Not expired according to the approved expiry policy

Expired Lots are excluded automatically during allocation and
consumption.

`ExpiringSoon` is a warning condition and remains eligible unless
product policy changes.

Do not mutate AdministrativeStatus only because time passed.

## FEFO Rules

For allocation:

1.  Load candidate Lots by ItemId.
2.  Evaluate eligibility using controlled time from TimeProvider.
3.  Remove expired, empty, quarantined, manually blocked, and disposed
    Lots.
4.  Sort:
    -   ExpiryDate ascending
    -   ReceivedAt ascending
    -   Id ascending as deterministic tie breaker
5.  Allocate across multiple Lots when required.
6.  Reject completely if eligible stock is insufficient.
7.  Allocation is only a recommendation and is not a reservation.

## Time Handling

Mandatory rules:

-   Use `.NET 8 TimeProvider.GetUtcNow()`.
-   Pass decision time into domain methods.
-   Domain must not read system clock directly.
-   Persist timestamps as UTC.
-   Keep DateOnly expiry policy explicit; do not invent hidden cutoff
    behavior.

## Roles and Authorization

MVP has exactly two roles:

-   `LabOperator`
-   `WarehouseAdmin`

Do not create additional roles.

LabOperator: - View dashboard, Item, Lot, status, stock, audit -
Create/start/complete/cancel Job - Request FEFO - Confirm Consumption

WarehouseAdmin: - Manage Item - Register/update Lot administrative
data - Dispose - Block/quarantine/release Lot - Manage user accounts,
status, and role assignment

Authorization MUST be enforced on server/API, not only hidden UI
buttons.

Actor identity for Consumption and Disposal must come from authenticated
context, not request payload.

## Transaction and Audit Rules

Consumption and Disposal must:

-   Validate authorization.
-   Validate quantity.
-   Re-check eligibility at actual execution time.
-   Use current RowVersion.
-   Update stock and create audit record in one transaction.
-   Roll back completely on failure.

Reject: - quantity \<= 0 - quantity greater than RemainingQuantity -
disposed Lot - expired/quarantined/manually blocked Lot for
consumption - RowVersion conflict

Never allow silent overwrite.

## Architecture Rules

Maintain Clean Architecture boundaries:

Domain: - Business entities - Expiry rules - FEFO rules - Inventory
invariants

Application: - Use cases - Authorization abstraction - Repository
interfaces - Unit of Work - Validation orchestration

Infrastructure: - EF Core - PostgreSQL mapping - Identity adapter -
Persistence implementation

Presentation: - API controllers - React UI - Input/output mapping

Domain and Application must not depend on database, UI, or
Infrastructure implementation.

## Quality Requirements

Before completing changes:

-   Build and test the application.
-   Enable TreatWarningsAsErrors.
-   Keep static analysis clean.
-   Test critical scenarios:
    -   expiry boundaries
    -   ExpiringSoon threshold
    -   low stock calculation
    -   partial/full consumption
    -   disposal
    -   FEFO across multiple Lots
    -   insufficient eligible stock
    -   identical expiry dates
    -   mid-job expiry
    -   optimistic concurrency conflict

Every model, DTO, mapping, repository, service, migration, test, and
document must preserve Lot as the smallest tracked inventory unit.


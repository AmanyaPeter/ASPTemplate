# Innovation Management Tracking System (IMTS)

IMTS is an ASP.NET Core 8 application for submitting, reviewing, tracking, and reporting innovation ideas. The repository is being repaired from the data layer upward. This document describes the current architecture, the changes made from the former arrangement, and how to run and test each feature without assuming unfinished screens are functional.

> **Current status:** the solution builds with zero errors using .NET 8 when MSBuild is run with one worker (`-m:1`). The Data project builds with zero warnings. Core and Web still have nullable/code-quality warnings. Account, role, and audit-log infrastructure exists; most innovation-management screens still require controllers and services.

## Contents

- [Architecture](#architecture)
- [Request and data flow](#request-and-data-flow)
- [What changed](#what-changed)
- [Current feature status](#current-feature-status)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Database setup](#database-setup)
- [Build and run](#build-and-run)
- [Feature-by-feature testing](#feature-by-feature-testing)
- [Automated verification](#automated-verification)
- [Migration workflow](#migration-workflow)
- [Troubleshooting](#troubleshooting)
- [Rules for future development](#rules-for-future-development)

## Architecture

The solution uses four projects. Dependencies point toward the shared/domain layers:

```text
Template.Web
    |
    v
Template.Core
    |
    v
Template.Data
    |
    v
Template.Common
```

### `Template.Common`

Contains types shared by all other layers:

- Audit base classes and interfaces
- Workflow enums such as `IdeaStage` and `IdeaStatus`
- Notification, report, scheduling, and resource enums
- Permission and role-name constants

This project must not reference Data, Core, or Web.

### `Template.Data`

Owns persistence and the domain entities:

- `ApplicationDbContext`
- ASP.NET Identity user storage
- Innovation ideas, drafts, categories, comments, and attachments
- Timeline and stage-history records
- Notifications and preferences
- Reports, resources, surveys, settings, and sessions
- EF Core migrations and reference-data seeding

Important design rules:

- ASP.NET Identity is the only role system.
- Workflow stage and status are enums stored as readable strings.
- `InnovationIdea.RowVersion` is a SQL Server row-version token.
- Delete behavior is explicit so business and audit records are not accidentally cascaded.
- Column limits and unique business keys are configured centrally in `ApplicationDbContext`.

### `Template.Core`

Contains application behavior that can be called by Web:

- Account and role repositories
- Permission-policy provider and authorization handler
- Active Directory authentication
- Audit interceptor
- Mapping profiles and application/view models shared with Web

Controllers should be thin. Business rules, workflow transitions, and reusable queries belong in Core services.

### `Template.Web`

Contains the presentation and application startup:

- MVC controllers and Razor views
- Web-only form/view models
- Middleware and view components
- CSS, JavaScript, fonts, and images
- Authentication cookies, routing, logging, Swagger, and dependency injection

The application currently uses MVC controller routing. Existing `asp-page` and `asp-page-handler` markup is legacy/incomplete and will be replaced with `asp-controller` and `asp-action` as each feature receives a controller.

## Request and data flow

The intended flow for a write operation is:

```text
Browser form
  -> MVC controller validates the form DTO
  -> Core application service enforces business rules
  -> ApplicationDbContext persists the entity
  -> AuditSaveChangesInterceptor fills audit columns
  -> Controller redirects to a GET action
  -> Razor view renders a read model
```

Do not bind write actions directly to EF entities. Use a dedicated form model with validation attributes, then map only allowed fields.

## What changed

### Former arrangement versus current arrangement

| Area | Former arrangement | Current arrangement | Reason |
|---|---|---|---|
| Roles | Identity roles plus a second integer `Role` entity and `ApplicationUser.RoleId` | Identity roles and `AspNetUserRoles` only | Prevent two role sources from disagreeing |
| DbContext roles | Custom `DbSet<Role> Roles` hid Identity's inherited `Roles` | Hidden property and custom entity removed | Removes compiler warning and authorization ambiguity |
| Idea workflow | `CurrentStage` and `CurrentStatus` were unrestricted strings | `IdeaStage` and `IdeaStatus` enums stored as names | Prevents invalid states and keeps database values readable |
| Timeline stage | Both `StageId` and `IdeaStage Stage` | Only `IdeaStage Stage` | Removes duplicated state |
| Concurrency | Ordinary `varbinary(max)` called `RowVersion` | Real SQL Server `rowversion` | Detects simultaneous reviewer updates |
| Entity constraints | Most strings became `nvarchar(max)`; few unique indexes | Explicit lengths and unique keys | Improves validation, indexing, and schema quality |
| Relationships | Only some relationships had delete behavior | Important relationships are explicitly configured | Prevents cascade-path errors and accidental record loss |
| Database startup | `EnsureCreatedAsync()` bypassed migration history | `MigrateAsync()` applies versioned migrations | Supports controlled upgrades |
| Administrator seed | Source contained `admin` / `Admin@123` | Predictable administrator seed removed | Removes a critical credential vulnerability |
| LDAP login | Password validation returned `true` unconditionally | `PrincipalContext.ValidateCredentials` is called | Restores actual password verification |
| LDAP settings | Server and directory container hard-coded in `Program.cs` | Values come from configuration | Supports environments without source edits |
| DbContext DI | Context factory plus two direct registrations | One audited DbContext registration | Avoids registrations overriding one another |
| Auditing | Interceptor existed but was not attached | Interceptor is attached to the active DbContext | Audit columns are populated consistently |
| Session | 365-day session with a 15-minute cookie | Both use a 15-minute security window | Avoids stale session state |
| Error detail | Detailed Blazor errors enabled everywhere | Enabled only in Development | Prevents information leakage |
| Build dependencies | Core and Web referenced `Microsoft.Build` 17.9.5 | Application-level references removed | Avoids SDK/MSBuild conflicts |
| Nullable analysis | Disabled in Core | Enabled | Exposes unsafe null handling for gradual repair |
| Configuration example | README described an untracked file only | `appsettings.example.json` is checked in | Gives new developers a safe starting point |

### Corrective migration

`20260721081558_NormalizeDomainModel` performs the schema transition. It:

- Drops the obsolete custom role table and `AspNetUsers.RoleId`
- Removes the redundant timeline `StageId`
- Creates a real row-version column
- Converts enum-backed integer columns into readable enum names
- Applies string limits and unique indexes
- Changes the idea submitter relationship to restricted deletion

Review and back up a database before applying this migration because role cleanup and the row-version replacement are intentionally destructive.

## Current feature status

| Feature | Status | Notes |
|---|---|---|
| Login/logout | Backend present | Requires Windows-hosted AD connectivity and a pre-provisioned application user |
| User accounts | Partially implemented | List/create/update/role assignment exist; validation and warning cleanup remain |
| Identity roles | Implemented | Create/edit/list and permission claims exist |
| Audit-log list | Read path implemented | Audit coverage must still be extended to every business action |
| Home/UI kit/privacy | Implemented as static pages | Dashboard data is not connected |
| Categories | UI prototype | Controller returns no model and has no CRUD service |
| Idea submission/drafts | UI prototype | No controller, service, or persistence workflow |
| My Ideas/details | UI prototype | No controller/query implementation |
| Review workflow | UI prototype | No assignment or transition service |
| Comments/attachments | Entities and UI only | No secure upload/download or comment actions |
| Notifications | Entity and UI prototype | No delivery/read-state service |
| Resources | Entity and UI prototype | No upload/download controller |
| Reports | Entity and UI prototype | No generation/export service |
| Dashboards | Static/prototype | View models exist but are not populated |
| Settings/timelines/surveys | Data entities only | No application or presentation layer |

## Prerequisites

1. .NET 8 SDK
2. SQL Server 2019 or newer, SQL Server Express, or LocalDB
3. EF Core 8 CLI tool
4. Access to the configured Active Directory domain for login testing
5. Windows hosting for `System.DirectoryServices.AccountManagement`

Check the SDK:

```bash
dotnet --version
```

Install EF tooling if needed:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

> LocalDB and the current AD library are Windows-specific. On Linux, use a reachable SQL Server connection. The application can compile on Linux, but AD login cannot be exercised with `PrincipalContext` there.

## Configuration

Copy the example file:

```bash
cp Template.Web/appsettings.example.json Template.Web/appsettings.json
```

`appsettings.json` is intentionally ignored by Git. Set these values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR SQL SERVER CONNECTION STRING"
  },
  "Authentication": {
    "Ldap": {
      "Server": "YOUR-DOMAIN-CONTROLLER",
      "Container": "DC=example,DC=org"
    }
  }
}
```

For deployed environments, prefer environment variables or a secret store:

```text
ConnectionStrings__DefaultConnection
Authentication__Ldap__Server
Authentication__Ldap__Container
```

Never commit passwords, production connection strings, LDAP service credentials, or certificates.

### First administrator

The insecure hard-coded administrator was removed. A database now seeds role names only. Until a dedicated bootstrap command is implemented, an administrator must be provisioned through an approved deployment process using ASP.NET Identity's `UserManager<ApplicationUser>` and assigned the `Admin` role. Do not restore the former `Admin@123` seed.

## Database setup

Back up an existing database before applying the normalization migration.

List migrations:

```bash
dotnet ef migrations list \
  --project Template.Data \
  --startup-project Template.Web
```

Apply migrations:

```bash
dotnet ef database update \
  --project Template.Data \
  --startup-project Template.Web
```

The application also calls `MigrateAsync()` during startup. Running the command explicitly is preferred in controlled deployments because migration failure is detected before application traffic is switched over.

## Build and run

Restore packages:

```bash
dotnet restore Template.sln
```

Build the solution:

```bash
dotnet build Template.sln -m:1 --no-restore
```

`-m:1` is currently required in the audited environment because parallel MSBuild project-reference discovery can fail without reporting an error. This should be investigated in CI rather than silently removed.

Run the application:

```bash
dotnet run --project Template.Web
```

Or use hot reload:

```bash
dotnet watch --project Template.Web run
```

Development URLs from `launchSettings.json` are:

- `https://localhost:7254`
- `http://localhost:5104`

Swagger is available at `/swagger` outside Development under the current middleware configuration. Swagger currently documents mapped Identity API endpoints, not the MVC UI.

## Feature-by-feature testing

Use a disposable test database. For every write test, verify both the visible result and the database/audit side effects.

### 1. Startup and migrations

1. Point `DefaultConnection` to an empty test database.
2. Run `dotnet ef database update`.
3. Start the Web project.
4. Confirm tables, including Identity and innovation tables, are created.
5. Confirm roles `Admin`, `Staff`, and `InnovationTeam` exist.
6. Confirm there is no custom `Roles` table and no `AspNetUsers.RoleId` column.

Expected: startup completes without creating a predictable administrator.

### 2. Authentication

Prerequisites: a Windows host connected to the AD domain and an application user matching an AD username.

1. Open `/Account/Login`.
2. Submit the correct username with an incorrect password.
3. Confirm login is rejected.
4. Submit valid AD credentials.
5. Confirm the Identity cookie is issued and the user reaches `/Home/Index`.
6. Wait longer than the configured idle period and confirm re-authentication is required.
7. Use logout and confirm the cookie and session are cleared.

Security assertion: an incorrect password must never authenticate. If LDAP is unavailable, login must fail closed.

### 3. Account administration

1. Sign in with `AccountController Index` permission.
2. Open `/Account` and confirm application users are listed.
3. With create permission, open `/Account/Create`.
4. Enter an AD username that does not exist and confirm validation fails.
5. Enter a valid, unprovisioned AD username and confirm the user is created.
6. Attempt the same username again and confirm duplicates are rejected.
7. Update active/end-date fields and confirm changes persist.
8. Remove the relevant permission and confirm access is denied.

Known limitation: account DTO validation and several nullable paths still need repair.

### 4. Role administration

1. Open `/Role` with role-view permission.
2. Create a uniquely named test role.
3. Confirm it appears in `AspNetRoles`.
4. Try creating the same name and confirm it is rejected.
5. Edit the role name and confirm normalized Identity fields update.
6. Open Manage Permissions, select claims, save, and inspect `AspNetRoleClaims`.
7. Assign the role to a test user from Account Manage Roles.
8. Confirm only `AspNetUserRoles` is changed; no custom role table should exist.

### 5. Permission enforcement

1. Create one role with a chosen permission and one without it.
2. Assign each role to separate test users.
3. Request the protected action as both users.
4. Confirm only the user with the `Permission` claim succeeds.
5. Remove the claim and repeat to confirm access is revoked.

### 6. Audit-log list

1. Insert or generate audit entries in the test database.
2. Open `/AuditLog` as a user with audit-log permission.
3. Confirm mapped fields render correctly.
4. Confirm an unauthorized user receives access denied.
5. Modify an auditable entity through a tracked application operation and confirm `CreatedBy`, `ModifiedBy`, and UTC timestamps are populated.

Known limitation: not every application event currently creates a dedicated `AuditLog` row.

### 7. Static pages

While authenticated, test:

- `/Home/Index`
- `/Home/Privacy`
- `/Home/UiKit`
- `/Home/TestPage`

Expected: pages render, but dashboard values and many buttons are still static.

### 8. Categories

Current expected result: `/Category` is incomplete because its controller does not supply `CategoriesModel`, and create/edit/delete handlers do not exist.

When implemented, test in this order:

1. Empty list
2. Create with required-name validation
3. Duplicate-name rejection
4. Edit
5. Activate/deactivate
6. Search and status filters
7. Pagination
8. Delete/deactivate rules when ideas reference a category
9. Authorization and antiforgery rejection

### 9. Idea submission and drafts

Current expected result: views exist but no `IdeaController` handles them.

Required eventual tests:

1. Required-field and length validation
2. Category selection
3. Individual/team submission rules
4. Draft save, reload, and version compatibility
5. Unique reference-number generation under concurrent submissions
6. Attachment size, extension, MIME, and path-traversal rejection
7. Successful submission with `Submitted`/`UnderReview` initial state
8. Audit, notification, and timeline creation in one transaction

### 10. My Ideas, details, comments, retract, and cancel

Current expected result: screens are prototypes and MVC actions do not exist.

Required eventual tests:

1. Users see only their own ideas
2. Search/filter/pagination
3. Details include category, attachments, comments, and timeline
4. Comment validation and ownership
5. Internal comments hidden from submitters
6. Retraction/cancellation allowed only in valid workflow states
7. Other users cannot access an idea by changing the URL ID

### 11. Review workflow

Current expected result: review UI exists without assignment/transition services.

Required eventual tests:

1. Reviewer assignment authorization
2. Valid and invalid stage/status transitions
3. Information-request and decision reasons
4. Timeline deadlines
5. `StageHistory` creation
6. Submitter notification
7. Row-version conflict between two reviewers
8. Locked/retracted/deleted idea protections

### 12. Notifications

Current expected result: entity and view prototype only.

Required eventual tests:

1. User isolation
2. Mark one/all as read
3. Read timestamp
4. Preference enforcement
5. Email retry behavior
6. Digest frequency and quiet hours
7. Safe internal links

### 13. Resources

Current expected result: entity and list prototype only.

Required eventual tests:

1. Authorized upload
2. Enum category selection
3. File validation and protected storage
4. Search/filter/pagination
5. Authorized download and counter increment
6. Missing-file behavior
7. Soft deletion and audit entries

### 14. Reports

Current expected result: filters/charts/export buttons have no backend.

Required eventual tests:

1. Inclusive date-range validation
2. Department/category/status filters
3. Summary totals match detail rows
4. PDF and Excel generation
5. Authorized download
6. Scheduled frequency
7. Generated-file cleanup and download count
8. Large-dataset performance

### 15. Dashboards

Current expected result: dashboard models/views exist but no query service populates them.

Required eventual tests:

1. Role selects the correct dashboard
2. Counts match database queries
3. Recent items obey visibility rules
4. SLA deadlines use UTC consistently
5. Empty-state rendering

### 16. Settings, timelines, surveys, and sessions

Current expected result: data entities only.

When application layers are added, test CRUD authorization, validation, audit columns, unique keys, session expiry, timeline override approval, and survey JSON schema/version handling.

## Automated verification

No test project is currently checked in. Until tests are added, the minimum verification is:

```bash
dotnet restore Template.sln
dotnet build Template.sln -m:1 --no-restore
dotnet test Template.sln -m:1 --no-build
dotnet ef migrations list --project Template.Data --startup-project Template.Web
```

`dotnet test` currently discovers no tests. Planned test projects should be:

```text
tests/
  Template.Data.IntegrationTests/
  Template.Core.UnitTests/
  Template.Web.IntegrationTests/
  Template.Web.EndToEndTests/
```

Priority automated scenarios are authentication failure, permission enforcement, workflow transitions, concurrent review updates, upload security, migration from the former schema, and the complete submit-to-decision journey.

## Migration workflow

After changing an entity or EF configuration:

```bash
dotnet ef migrations add MeaningfulMigrationName \
  --project Template.Data \
  --startup-project Template.Web
```

Then:

1. Read both `Up` and `Down` methods.
2. Look for unintended drops, nullable changes, and enum conversions.
3. Generate a SQL script.
4. Test upgrading a copy of the previous schema.
5. Test rollback where rollback is supported.
6. Build the complete solution.

Generate a reviewable script:

```bash
dotnet ef migrations script \
  --idempotent \
  --project Template.Data \
  --startup-project Template.Web \
  --output artifacts/imts-migration.sql
```

Never edit the model snapshot without a corresponding migration.

## Troubleshooting

### Build fails with no errors

Use the .NET 8 SDK and disable parallel MSBuild:

```bash
dotnet build Template.sln -m:1 -v minimal
```

Confirm `dotnet --version` reports `8.x`.

### `appsettings.json` is missing

```bash
cp Template.Web/appsettings.example.json Template.Web/appsettings.json
```

Then replace the example SQL Server and LDAP values.

### LocalDB connection fails on Linux

LocalDB is Windows-only. Use SQL Server/SQL Server Express in a container, VM, or reachable server and change `DefaultConnection`.

### LDAP login fails on Linux

The current `PrincipalContext` implementation is Windows-only. Run the Web application on Windows for AD testing or replace the adapter with a cross-platform LDAP implementation in a separate change.

### Migration warns about data loss

The normalization migration intentionally removes the obsolete custom role schema and replaces invalid row-version bytes. Back up the database, inspect the migration SQL, and test on a restored copy before production.

### Access is always denied

Verify:

1. The user exists in `AspNetUsers`.
2. The role exists in `AspNetRoles`.
3. `AspNetUserRoles` contains the assignment.
4. `AspNetRoleClaims` contains claim type `Permission` with the exact expected value.

## Rules for future development

- Work upward: Common/Data, then Core, then Web.
- Add short comments explaining **why** security, concurrency, transaction, or relationship behavior is necessary.
- Do not comment obvious property declarations or restate code in English.
- Use Identity roles only.
- Use enums for controlled workflow values.
- Use form DTOs for writes and read models for display.
- Validate on both server and client; server validation is authoritative.
- Put workflow rules in Core services, not Razor views or JavaScript.
- Use POST plus antiforgery protection for every state change.
- Store files outside the public web root and validate content, size, and name.
- Use UTC for persisted timestamps.
- Add a migration for every schema change and test it against the previous schema.
- Add tests with each completed feature; do not mark a prototype screen complete merely because it renders.

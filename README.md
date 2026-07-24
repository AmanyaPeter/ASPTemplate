# Innovation Management Tracking System

The Innovation Management Tracking System (IMTS) is an ASP.NET Core application for managing the Bank of Uganda innovation lifecycle. It centralizes idea submission, review, communication, stage tracking, notifications, reporting, resources, user administration, and audit activity.

The implementation is guided by [Innovation Management System Requirements.pdf](Innovation%20Management%20System%20Requirements.pdf).

## Supported roles

| Role | Main responsibilities |
|---|---|
| Staff | Submit ideas, upload attachments, view progress, comment, receive notifications, and retract or cancel owned submissions |
| Innovation Team | Review submitted ideas, update workflow stages and statuses, assign reviewers, set deadlines, comment, and notify submitters |
| IT Administrator | Manage users, roles, permissions, account status, categories, reports, resources, and audit activity |

Role names used by the application are defined in `Template.Common/Static/RoleConstants.cs`:

- `Staff`
- `InnovationTeam`
- `Admin`

## Innovation workflow

The application supports the SRS-aligned idea lifecycle:

1. Staff completes the idea submission form.
2. The system validates required fields and attachments.
3. A reference number is generated and the idea enters the submitted/under-review state.
4. The Innovation Team reviews the idea and may assign a reviewer and deadline.
5. Stage and status changes are recorded in the timeline and stage history.
6. The submitter receives in-app notifications and may follow progress or comment.
7. Approved ideas progress through concept development, experimentation or research, deployment, and closure.

Workflow operations are separated by responsibility:

- `IdeaController` handles submission, staff lists, review queues, and details.
- `IdeaStaffActionsController` handles staff-owned actions, comments, and attachment downloads.
- `IdeaWorkflowController` handles Innovation Team review and workflow transitions.

## Solution structure

```text
Template.sln
|-- Template.Common/   Shared enums, constants, permissions, and audit base types
|-- Template.Data/     EF Core entities, DbContext, migrations, and seed data
|-- Template.Core/     Repositories, services, authorization, and application models
|-- Template.Web/      MVC controllers, Razor views, UI models, and static assets
|-- design-reference/  UI reference material
|-- Innovation Management System Requirements.pdf
`-- README.md
```

The main dependency direction is:

```text
Template.Web -> Template.Core -> Template.Data -> Template.Common
```

## Technology

- .NET 8
- ASP.NET Core MVC and Razor
- ASP.NET Core Identity with `Guid` user and role keys
- Entity Framework Core 8
- Microsoft SQL Server / SQL Server LocalDB
- Blazor Server components
- NLog
- AutoMapper
- SmartBreadcrumbs
- Swagger/OpenAPI

## Prerequisites

Install:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB, SQL Server Express, SQL Server Developer, or another accessible SQL Server instance
- Visual Studio 2022 with the **ASP.NET and web development** workload, or VS Code with C# Dev Kit
- Git

Check the SDK:

```powershell
dotnet --version
```

## Configuration

The development connection string is in `Template.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InnovationDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=false"
  }
}
```

For a shared or production database, override `ConnectionStrings:DefaultConnection` with user secrets, an environment-specific settings file, or an environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=YOUR_SERVER;Database=InnovationDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True"
```

Do not commit production credentials or secrets.

## Restore, build, and run

From the repository root:

```powershell
dotnet restore
dotnet build Template.sln
dotnet run --project Template.Web/Template.Web.csproj --launch-profile http
```

Open:

- HTTP: [http://localhost:5104](http://localhost:5104)
- HTTPS profile: [https://localhost:7254](https://localhost:7254)

For hot reload:

```powershell
dotnet watch --project Template.Web/Template.Web.csproj run --launch-profile http
```

### Visual Studio

1. Open `Template.sln`.
2. Set `Template.Web` as the startup project.
3. Select the `http` or `https` launch profile.
4. Press `F5`, or `Ctrl+F5` to run without debugging.

### VS Code

Open the repository root, then run:

```powershell
dotnet run --project Template.Web/Template.Web.csproj --launch-profile http
```

If VS Code shows stale Razor problems after a successful build, open the Command Palette and run **Developer: Reload Window**.

## Database initialization

Application startup calls `DbInitializer.SeedAsync`, which:

- Applies pending EF Core migrations.
- Creates the `Admin`, `Staff`, and `InnovationTeam` roles.
- Grants all currently defined permission claims to the Admin role.
- Creates development users when they do not exist.
- Seeds the default idea categories.
- Resets stale development login-state flags.

In Development, `DashboardSeedData.SeedAsync` also creates sample dashboard data.

### Development accounts

The default development password is `Admin@12345678`.

| Username | Role |
|---|---|
| `admin` | Admin |
| `staff` | Staff |
| `innovation` | InnovationTeam |

These accounts are for local development only. Change or remove seeded credentials before deployment.

## Entity Framework migrations

Install the EF CLI tool if necessary:

```powershell
dotnet tool install --global dotnet-ef
```

Create a migration:

```powershell
dotnet ef migrations add MigrationName --project Template.Data --startup-project Template.Web
```

Apply migrations manually:

```powershell
dotnet ef database update --project Template.Data --startup-project Template.Web
```

List migrations:

```powershell
dotnet ef migrations list --project Template.Data --startup-project Template.Web
```

Generate a deployment script:

```powershell
dotnet ef migrations script --idempotent --project Template.Data --startup-project Template.Web --output migration.sql
```

The application normally applies migrations during startup. Manual commands are mainly useful for development diagnostics and controlled deployment workflows.

## Major application areas

| Area | Purpose |
|---|---|
| Dashboard | Role-specific summary for Staff, Innovation Team, and Admin users |
| Ideas | Submission, personal idea list, review queue, details, attachments, comments, and workflow |
| Notifications | Read/unread filtering and individual or bulk read actions |
| Categories | Create, update, activate, deactivate, and classify ideas |
| Reports | Filter innovation data and export supported report formats |
| Resources | Browse and download innovation resources |
| Accounts | Create and update users and assign roles |
| Roles and permissions | Maintain Identity roles and permission claims |
| Audit logs | Review recorded system activity |
| Settings and profile | User-facing support, settings, and profile pages |

## Attachment rules

Idea attachments are validated by the server:

- Maximum size: 10 MB per file
- Supported types: PDF, Word, Excel, and PNG
- Uploaded filenames are replaced with generated storage names
- Downloads are authorized against the requesting user’s role and idea ownership

Client-side validation is useful for feedback, but server-side validation remains authoritative.

## Authentication and authorization

The application uses ASP.NET Core Identity and cookie authentication:

- Login path: `/Account/Login`
- Logout path: `/Account/Logout`
- Cookie sliding expiration: 15 minutes
- Role-based authorization protects staff, review, and administration operations
- Permission claims support finer-grained authorization for administrative features

Active Directory integration is represented by `IAdAuthenticationService`. LDAP server and container values are currently registered in `Template.Web/Program.cs`; move environment-specific values into protected configuration before production deployment.

## Logging

NLog is configured through `Template.Web/nlog.config`. Application code also uses standard `ILogger<T>` logging.

Never log:

- Passwords
- Authentication cookies or tokens
- Production connection strings
- Sensitive personal data
- Full attachment contents

## Verification

Run a forced solution rebuild before opening a pull request:

```powershell
dotnet build Template.sln --no-restore -t:Rebuild
```

Expected result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Also scan for unresolved merge markers:

```powershell
rg -n "^(<<<<<<< |=======|>>>>>>> )" Template.Common Template.Data Template.Core Template.Web -g "*.cs" -g "*.cshtml"
```

No output is expected.

## Collaboration and merge guidance

Before pulling or merging:

```powershell
git status
git fetch
```

After resolving a merge:

1. Search for unresolved conflict markers.
2. Check that controllers do not exist twice under similar filenames.
3. Verify Razor tags and `@section` blocks are balanced.
4. Run a forced rebuild.
5. Exercise the Staff submission, Innovation Team review, and Admin dashboard flows.
6. Review database migration ordering and the model snapshot.

Do not resolve a controller conflict by simply retaining both implementations. Reconcile routes, authorization roles, dependency injection, model bindings, and ownership checks.

## Production checklist

Before production deployment:

- Replace LocalDB with the approved SQL Server connection.
- Store connection strings and LDAP settings outside source control.
- Remove or secure development seed accounts and sample dashboard data.
- Enforce HTTPS and validate the reverse-proxy/IIS configuration.
- Review cookie lifetime and session policy.
- Verify role and permission assignments.
- Confirm attachment storage permissions and retention rules.
- Review NLog targets and log retention.
- Back up the database before applying migrations.
- Perform security, accessibility, performance, and user-acceptance testing against the SRS.

## Requirements reference

The repository’s functional baseline is:

[Innovation Management System Requirements.pdf](Innovation%20Management%20System%20Requirements.pdf)

When implementation behavior and this README differ, verify the SRS and update the code and documentation together.

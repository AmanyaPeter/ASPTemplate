# Innovation Management Tracking System (IMTS)

## System overview

IMTS is an ASP.NET Core 8 web application for Bank of Uganda's innovation-management process. It replaces manual idea records and communication with a central workflow from idea submission through selection, concept development, experimentation or research, deployment, and closure.

The system is based on the *Innovation Management System Requirements* SRS and supports three primary users:

- **Staff** submit individual or team ideas, add attachments, view progress, receive notifications, comment, and retract or cancel their own submissions.
- **Innovation Team** view and filter submitted ideas, review details and attachments, manage stages and statuses, communicate with submitters, and monitor stage deadlines.
- **IT Administrator** manages user accounts, roles, account status, and audit activity.

The application uses role-based personal dashboards, SQL Server persistence, ASP.NET Core Identity, audit records, notifications, reports, and downloadable resources. It is intended for on-premises deployment on Windows Server, IIS, and Microsoft SQL Server.

## Folder structure

```text
ASPTemplate/
|-- Template.sln          Solution file
|-- Template.Common/      Shared enums, role names, constants, and audit base classes
|-- Template.Data/        EF Core entities, DbContext, migrations, and seed data
|-- Template.Core/        Business models, repositories, services, and authorization
|-- Template.Web/         MVC controllers, Razor views, static files, and application startup
|-- design-reference/     UI and design reference material
|-- Innovation Management System Requirements.pdf
`-- README.md
```

Dependencies flow from `Template.Web` to `Template.Core`, then `Template.Data` and `Template.Common`.

## Run offline with Visual Studio

### Prerequisites

- Visual Studio 2022 with the **ASP.NET and web development** workload
- .NET 8 SDK
- SQL Server Express LocalDB, normally installed through Visual Studio
- NuGet packages restored at least once while internet access is available

### Start the application

1. Open `Template.sln` in Visual Studio.
2. Right-click `Template.Web` and select **Set as Startup Project**.
3. Start LocalDB from a terminal or Package Manager Console:

   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```

4. Restore packages if they are not already cached:

   ```powershell
   dotnet restore
   ```

5. Build the solution with **Build > Build Solution** or `Ctrl+Shift+B`.
6. Select the `https` launch profile and press `F5`, or press `Ctrl+F5` without debugging.

The application uses the `InnovationManagementDb` LocalDB database configured in `Template.Web/appsettings.json`. On startup it automatically applies EF Core migrations and seeds the roles, development users, categories, and sample dashboard data. No manual `Update-Database` command is normally required.

The default development addresses are:

- `https://localhost:7254`
- `http://localhost:5104`

Development accounts use the seeded password `Admin@123`:

| Role | Username |
|---|---|
| IT Administrator | `admin` |
| Staff | `staff` |
| Innovation Team | `innovation` |

### Template.Web

The presentation layer. ASP.NET Core MVC with Blazor Server integration.

| Directory / File                | Description                                                                 |
|---------------------------------|-----------------------------------------------------------------------------|
| `Program.cs`                    | Application entry point. Configures services, middleware pipeline, authentication, session, Blazor, Swagger, NLog, breadcrumbs |
| `appsettings.json`              | Configuration — connection string (`IMTSDb` on LocalDB)                     |
| `nlog.config`                   | NLog configuration for file and database logging targets                     |
| `Controllers/`                  | MVC controllers                                                             |
| `Views/`                        | Razor views (e.g., Shared `_Layoutmain.cshtml`)                             |
| `Components/Shared/`            | Blazor Server components: `BreadcrumbViewComponent`, `ProfileViewComponent`, `ToastMessages` |
| `Middleware/`                   | Custom HTTP middleware (e.g., `LastActivityMiddleware`)                     |
| `Models/`                       | UI-specific view models                                                      |
| `wwwroot/`                      | Static assets (CSS, JS, fonts, images)                                      |
| `Properties/`                   | Launch profiles, IIS settings                                               |
| `.config/`                      | Additional configuration files                                              |

**Key `Program.cs` pipeline order:**

1. NLog logging setup
2. Controllers with Views + Blazor Server
3. Cookie authentication (`LoginPath = /Account/Login`)
4. Distributed memory cache + session (365-day timeout)
5. EF Core `ApplicationDbContext` (SQL Server)
6. ASP.NET Core Identity with `IdentityUser<Guid>`
7. LDAP `AdAuthenticationService` (singleton)
8. DataServicesRegistration + CoreServicesRegistration
9. SmartBreadcrumbs
10. Database seeding via `DbInitializer.SeedAsync`
11. Middleware: Swagger (non-dev), Exception Handling (non-dev), HSTS, Response Caching, Static Files (7-day cache), HTTPS Redirection, Routing, Session, Authentication, Authorization
12. MapIdentityApi, MapControllerRoute, MapBlazorHub

---

## Key Features

- **Innovation Idea Management** — Full lifecycle: draft → submit → review → approve → implement
- **Stage-Based Workflow** — Ideas progress through defined stages with history tracking
- **Role-Based Access Control** — Granular permissions per role (IT Support, Budget Officer, Budget Holder, Budget Admin)
- **Active Directory Integration** — Authenticate users against corporate LDAP directory
- **Audit Logging** — Automatic tracking of entity changes via EF Core interceptor
- **Notifications** — In-app notifications with configurable preferences
- **Reporting** — Report generation with multiple format options
- **File Attachments** — Upload and manage attachments on ideas
- **Category Management** — Classify ideas into categories
- **Timeline Tracking** — Visual timeline of idea progression
- **Survey Support** — Collect feedback via survey responses
- **Blazor Server Dashboard** — Interactive components for enhanced UX
- **Breadcrumb Navigation** — Smart hierarchical navigation via SmartBreadcrumbs
- **Comprehensive Logging** — NLog with file and database targets

---

## Prerequisites

Ensure the following are installed on your **Windows** machine:

| Requirement               | Version / Notes                                                |
|---------------------------|----------------------------------------------------------------|
| **Windows OS**            | Windows 10 or Windows 11 (Windows Server also supported)       |
| **.NET 8 SDK**            | [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **SQL Server**            | LocalDB (installed with Visual Studio) or SQL Server Express/Developer |
| **Visual Studio 2022**    | (Recommended) Community, Professional, or Enterprise edition   |
| **OR**                    |                                                                |
| **VS Code**               | Latest version with C# Dev Kit extension                       |

Optional but recommended:
- **SQL Server Management Studio (SSMS)** — For database management
- **Git** — For version control

---

## Environment Setup — Visual Studio 2022

### Step 1: Install Visual Studio 2022

1. Download from [visualstudio.microsoft.com](https://visualstudio.microsoft.com/vs/)
2. Run the installer and select the following workloads:
   - **ASP.NET and web development**
   - **.NET desktop development** (optional, if needed)
   - **Data storage and processing** (includes SQL Server LocalDB)
3. In the **Individual components** tab, ensure these are selected:
   - .NET 8 SDK
   - SQL Server LocalDB
   - Entity Framework 6 tools (optional)
4. Complete installation and restart your machine.

### Step 2: Clone or Open the Project

```bash
git clone https://github.com/AmanyaPeter/ASPTemplate.git
```

Or open the existing project folder:

1. Launch **Visual Studio 2022**
2. Click **File → Open → Project/Solution**
3. Navigate to `..\ASPTemplate\Template.sln`
4. Click **Open**

### Step 3: Verify Dependencies

Once the solution loads:

1. **Right-click** the solution in **Solution Explorer** → **Restore NuGet Packages**
2. Wait for package restore to complete (check Output window → "NuGet Package Manager")

### Step 4: Configure the Database Connection

Open `Template.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IMTSDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": { "LogLevel": { "Default": "Information" } }
}
```

This uses **SQL Server LocalDB** which is included with Visual Studio. If you have a different SQL Server instance, update the `Server` value accordingly.

### Step 5: Apply Database Migrations

**Option A — Using Package Manager Console (PMC):**

1. **Tools → NuGet Package Manager → Package Manager Console**
2. Set **Default project** to `Template.Data`
3. Run:
```powershell
Update-Database
```

**Option B — Using .NET CLI (if PMC is unavailable):**
```powershell
To Run:
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
dotnet run --project Template.Web --no-build --launch-profile http
```

```bash
dotnet ef database update --project Template.Data --startup-project Template.Web
```

This will create the `IMTSDb` database and apply all migrations.

### Step 6: Build and Run

1. Press **Ctrl+Shift+B** to build the solution (or **Build → Build Solution**)
2. Press **F5** to run with debugging, or **Ctrl+F5** to run without debugging
3. The application will:
   - Launch in your default browser
   - Seed the database with initial roles and data (via `DbInitializer.SeedAsync`)
   - Redirect to the login page

### Step 7: Set as Startup Project (if needed)

If you encounter a "No startup project configured" error:

1. In **Solution Explorer**, **right-click** `Template.Web`
2. Select **Set as Startup Project**

---

## Environment Setup — VS Code

### Step 1: Install VS Code and Extensions

1. Download VS Code from [code.visualstudio.com](https://code.visualstudio.com/)
2. Install the following extensions:
   - **C# Dev Kit** (ms-dotnettools.csdevkit) — includes C#, .NET debugging, project management
   - **C# Extensions** (jchannon.csharpextensions) — for creating classes, interfaces, etc.
   - **MSBuild project tools** (tintoy.msbuild-project-tools) — for .csproj editing
   - **SQL Server (mssql)** — optional, for database management
   - **NuGet Gallery** — optional, for package management

### Step 2: Install .NET 8 SDK

Download and install from [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0).

Verify installation:

```bash
dotnet --version
# Expected output: 8.0.xxx
```

### Step 3: Clone or Open the Project

```bash
git clone <repository-url> C:\Users\Ozai\Desktop\ASPTemplate
```

Or:

1. **File → Open Folder** → Select `C:\Users\Ozai\Desktop\ASPTemplate`
2. When prompted, click **Yes** to trust the authors (if applicable)

### Step 4: Install SQL Server (if not already installed)

If you don't have SQL Server installed:

**Option A — Install LocalDB (recommended for development):**

1. Download [SQL Server Express with LocalDB](https://go.microsoft.com/fwlink/?linkid=866662)
2. Run the installer and select **LocalDB** installation
3. Verify LocalDB is running:
```bash
sqllocaldb info
# Should show "MSSQLLocalDB" in the list
```

**Option B — Use SQL Server Express or Developer Edition** (free from Microsoft).

### Step 5: Restore NuGet Packages

```bash
dotnet restore
```

### Step 6: Apply Database Migrations

```bash
dotnet ef database update --project Template.Data --startup-project Template.Web
```

If you get a "dotnet-ef not found" error, install the EF Core tools:

```bash
dotnet tool install --global dotnet-ef
```

Then retry the `database update` command.

### Step 7: Build and Run

```bash
dotnet build
dotnet run --project Template.Web
```

The application will be available at:
- HTTP: `http://localhost:5000` (or another port shown in terminal output)
- HTTPS: `https://localhost:5001`

### Step 8: Configure VS Code Launch Settings

For a better debugging experience, create a `.vscode/launch.json` file:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch Web",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/Template.Web/bin/Debug/net8.0/Template.Web.dll",
      "args": [],
      "cwd": "${workspaceFolder}/Template.Web",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Template.Web/Views"
      }
    }
  ]
}
```

And a `.vscode/tasks.json` file for the build task:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/Template.Web/Template.Web.csproj",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

---

## Running the Application

### Using Visual Studio

1. **Set `Template.Web` as the startup project**
2. Press **F5** (Debug) or **Ctrl+F5** (Without Debug)
3. The browser opens automatically to the application URL

### Using VS Code

```bash
# Terminal 1: Run the application
dotnet run --project Template.Web

# Or with hot reload (for development)
dotnet watch run --project Template.Web
```

### Using .NET CLI directly

```bash
cd Template.Web
dotnet run
```

### Accessing Swagger UI

Swagger is only available in **non-development** environments. To access:
1. Set `ASPNETCORE_ENVIRONMENT=Staging` or `Production`
2. Navigate to `https://localhost:5001/swagger`

Or modify `Program.cs` to enable Swagger in development as well (remove the `!app.Environment.IsDevelopment()` condition around the Swagger block).

---

## Database Migrations

### Adding a New Migration

**Package Manager Console (Visual Studio):**
```powershell
Add-Migration MigrationName -Project Template.Data -StartupProject Template.Web
```

**.NET CLI:**
```bash
dotnet ef migrations add MigrationName --project Template.Data --startup-project Template.Web
```

### Applying Migrations

**Package Manager Console:**
```powershell
Update-Database -Project Template.Data -StartupProject Template.Web
```

**.NET CLI:**
```bash
dotnet ef database update --project Template.Data --startup-project Template.Web
```

### Removing Last Migration

```bash
dotnet ef migrations remove --project Template.Data --startup-project Template.Web
```

### Generating SQL Script

```bash
dotnet ef migrations script --project Template.Data --startup-project Template.Web
```

---

## Authentication & Authorization

### Authentication

The system supports two authentication methods:

1. **Cookie Authentication** (default) — Users log in via `/Account/Login`
2. **Active Directory / LDAP Authentication** — Integrated via `AdAuthenticationService` (configured in `Program.cs`)

The LDAP service is configured as a singleton in `Program.cs`:

```csharp
builder.Services.AddSingleton<IAdAuthenticationService>(provider =>
    new AdAuthenticationService(
        "SVRHQSDC001",                           // LDAP server
        "DC=BCNET,DC=BOU,DC=OR,DC=UG",           // LDAP container
        provider.GetRequiredService<ILogger<AdAuthenticationService>>()
    ));
```

### Authorization

The system implements **permission-based authorization**:

- `ApplicationPermissionHandler` — Custom `AuthorizationHandler` that checks user permissions
- `ApplicationPermissionPolicyProvider` — Custom `IAuthorizationPolicyProvider` that dynamically creates policies
- `PermissionTagHelper` — Razor Tag Helper to conditionally render UI elements based on permissions

Roles defined in `RoleConstants`:
- `IT Support`
- `Budget Officer`
- `Budget Holder`
- `Budget Admin`
- `Budget Admin Viewer`

---

## Logging

Logging is configured via **NLog** with the following configuration (`nlog.config`):

- **File target** — Logs written to files with rotation
- **Database target** — Logs written to the application database
- ASP.NET Core internal logging is cleared and replaced with NLog

Configuration is set up in `Program.cs`:
```csharp
builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.AddLogging();
```

---

## Project Conventions

### Naming Conventions

- **Solution**: `Template.sln`
- **Projects**: `Template.Common`, `Template.Data`, `Template.Core`, `Template.Web`
- **Namespaces**: Follow folder structure, e.g., `Template.Core.Repository.Accounts`
- **Enums**: PascalCase in dedicated `enums/` folder
- **Interfaces**: Prefix with `I` (e.g., `IRepositoryBase<T, TId>`)
- **Entities**: PascalCase, singular (e.g., `InnovationIdea`, `AuditLog`, `Category`)

### Coding Standards

- Target framework: `.NET 8`
- Nullable reference types: **Enabled** (`<Nullable>enable</Nullable>`) in most projects
- Implicit usings: **Enabled** (`<ImplicitUsings>enable</ImplicitUsings>`)
- File-scoped namespaces used in most files (e.g., `namespace Template.Data.Configurations;`)
- Async/await pattern used throughout for I/O operations
- Repository pattern for data access abstraction
- AutoMapper for entity-to-DTO mapping

### DI Registration

Each layer exposes a static extension method for registering its services:
- `DataServicesRegistration.AddDataServices()` — Registers DbContext
- `CoreServicesRegistration.AddCoreServices()` — Registers repositories, services, AutoMapper, authorization

These are called from `Program.cs`:
```csharp
DataServicesRegistration.AddDataServices(builder.Services, builder.Configuration);
CoreServicesRegistration.AddCoreServices(builder.Services);
```

---

## Troubleshooting

### Common Issues & Solutions

| Issue                                         | Solution                                                                                 |
|-----------------------------------------------|------------------------------------------------------------------------------------------|
| **SQL Server LocalDB not found**              | Run `sqllocaldb start MSSQLLocalDB` or reinstall LocalDB via Visual Studio installer     |
| **"dotnet-ef" command not found**             | Run `dotnet tool install --global dotnet-ef`                                             |
| **NuGet packages not restored**               | Run `dotnet restore` or use Visual Studio's **Restore NuGet Packages**                   |
| **Database does not exist / login failed**    | Verify connection string in `appsettings.json`. Ensure SQL Server is running.            |
| **Port already in use**                       | Change the `applicationUrl` in `Properties/launchSettings.json`                          |
| **Build errors after cloning**                | Ensure .NET 8 SDK is installed. Run `dotnet restore` then `dotnet build`.                |
| **Migrations pending error**                  | Run `dotnet ef database update` to apply pending migrations                              |
| **LDAP authentication fails**                 | Update server name and LDAP container in `Program.cs` to match your AD environment       |
| **Blazor Server disconnects**                 | Ensure SignalR is configured. Check browser console for connection errors.               |
| **Permission denied on static files**         | Verify the `StaticFileOptions` in `Program.cs` and that `wwwroot/` contains the files     |

### Useful Commands

```bash
# Restore all NuGet packages
dotnet restore

# Build the solution
dotnet build

# Apply EF Core migrations
dotnet ef database update --project Template.Data --startup-project Template.Web

# List all EF Core migrations
dotnet ef migrations list --project Template.Data --startup-project Template.Web

# Run the application
dotnet run --project Template.Web

# Run with hot reload
dotnet watch run --project Template.Web

# Clear NuGet cache (if packages are corrupted)
dotnet nuget locals all --clear
```

### Getting Help

If you encounter issues not covered here:
1. Check Visual Studio's **Output** window for detailed error messages
2. For VS Code, check the **Terminal** and **Problems** panels
3. Review application logs (NLog file targets)
4. Verify your .NET SDK version: `dotnet --info`

---

*This README was generated for the ASPTemplate (IMTS) project — an Innovation Management Tracking System built with ASP.NET Core 8.*
For fully offline use, ensure the .NET SDK, LocalDB, and required NuGet packages are installed or cached before disconnecting from the network.

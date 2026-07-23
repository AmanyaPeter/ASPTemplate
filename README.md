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

For fully offline use, ensure the .NET SDK, LocalDB, and required NuGet packages are installed or cached before disconnecting from the network.

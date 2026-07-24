# IMTS DIY Development Guide

This is the beginner-friendly companion to `DEVELOPMENT_PLAN.md`. The development plan explains **what the finished system requires**. This guide explains **what to do with your hands, where each file belongs, and how to check your work**.

Do one chapter at a time. Never continue past a red build.

## 1. Learn the four boxes

The solution has four main projects. Think of them as four boxes:

```text
Template.Web     What the user sees and clicks
Template.Core    The application's reusable work and rules
Template.Data    Database tables and database connection
Template.Common  Names and values shared everywhere
```

### Where should I put a new file?

Use this quick rule:

| You are making | Put it here |
|---|---|
| Controller | `Template.Web/Controllers/` |
| Razor page displayed by a controller | `Template.Web/Views/<FeatureName>/` |
| Form or page model used only by the website | `Template.Web/Models/<FeatureName>/` |
| Repository interface and implementation | `Template.Core/Repository/<FeatureName>/` |
| Database entity/table definition | `Template.Data/Entities/` |
| Shared enum or role name | `Template.Common/` |
| CSS | `Template.Web/wwwroot/css/` |
| Database migration | `Template.Data/Migrations/` using EF tools |

## 2. Understand one MVC request

Suppose a user visits:

```text
/Category/Index
```

ASP.NET follows this path:

```text
CategoryController.Index()
        ↓
ICategoryRepository asks for categories
        ↓
ApplicationDbContext reads SQL Server
        ↓
Controller creates CategoriesModel
        ↓
Views/Category/Index.cshtml displays it
```

Remember:

- A **controller** receives browser requests.
- A **repository** reads or writes database data.
- A **model** carries page data.
- A **view** creates the HTML.
- An **entity** describes a database record.

Do not put database queries inside a Razor view.

## 3. Prepare your computer

### Step 1: Install the required software

Install:

1. Visual Studio 2022.
2. In Visual Studio Installer, select **ASP.NET and web development**.
3. Install the .NET 8 SDK.
4. Install SQL Server Express LocalDB through Visual Studio Installer.

### Step 2: Open the solution

1. Open Visual Studio.
2. Select **Open a project or solution**.
3. Open `Template.sln`.
4. In Solution Explorer, right-click `Template.Web`.
5. Select **Set as Startup Project**.

### Step 3: Start the database

Open **View > Terminal** in Visual Studio and run:

```powershell
sqllocaldb start MSSQLLocalDB
```

Expected result:

```text
LocalDB instance "MSSQLLocalDB" started.
```

### Step 4: Restore packages

Run:

```powershell
dotnet restore Template.sln
```

Do this while internet access is available. After packages are cached, normal development can work offline.

### Step 5: Build before changing anything

Run:

```powershell
dotnet build Template.sln --no-restore
```

Expected result:

```text
Build succeeded.
0 Error(s)
```

Warnings may exist. Errors must be fixed before continuing.

## 4. Your safety routine before every feature

Before touching a feature:

1. Save all files.
2. Open a terminal in the repository root.
3. Run:

   ```powershell
   git status --short
   ```

4. Write down which files were already modified.
5. Build the solution.
6. Change only files related to the feature.
7. Build again after each small group of changes.

Never use `git reset --hard` to solve a build problem. It can erase your work.

## 5. The repeatable feature recipe

Use this recipe for Categories, Resources, Timelines, Reports, and most other features.

### Part A: Check the database entity

Go to:

```text
Template.Data/Entities/
```

Find or create the entity. For Categories, the file is:

```text
Template.Data/Entities/Category.cs
```

An entity looks like this:

```csharp
public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
```

Ask:

- Does the database already contain every field I need?
- Which fields are required?
- Can this record be deactivated instead of deleted?

If the entity already has the required fields, do not make a migration.

### Part B: Create the page model

Create or open:

```text
Template.Web/Models/<FeatureName>/
```

For Categories:

```text
Template.Web/Models/Category/CategoriesModel.cs
```

Put only data required by the page in this model. Do not pass the entire database entity directly into a POST form.

Example:

```csharp
public class CategoryFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
```

### Part C: Create the repository folder

Create:

```text
Template.Core/Repository/<FeatureName>/
```

For Categories:

```text
Template.Core/Repository/Categories/
```

Inside it, create:

```text
ICategoryRepository.cs
CategoryRepository.cs
```

The interface says what work is available:

```csharp
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, bool tracking = false);
    Task<bool> NameExistsAsync(string name, int? excludingId = null);
    void Add(Category category);
    Task SaveChangesAsync();
}
```

The implementation uses `ApplicationDbContext` to perform that work.

For read-only queries, start with:

```csharp
context.Categories.AsNoTracking()
```

Use a tracked query only when you intend to modify and save that record.

### Part D: Register the repository

Open:

```text
Template.Core/CoreServicesRegistration.cs
```

Add the namespace near the top:

```csharp
using Template.Core.Repository.Categories;
```

Add the registration inside `AddCoreServices`:

```csharp
services.AddScoped<ICategoryRepository, CategoryRepository>();
```

Why? This tells ASP.NET what concrete class to create when a controller requests `ICategoryRepository`.

If you forget this step, the application will fail at runtime with a message similar to:

```text
Unable to resolve service for type ICategoryRepository
```

### Part E: Create the controller

Create:

```text
Template.Web/Controllers/<FeatureName>Controller.cs
```

For Categories:

```text
Template.Web/Controllers/CategoryController.cs
```

Start with authorization:

```csharp
[Authorize(Roles = RoleConstants.InnovationTeam)]
public class CategoryController(ICategoryRepository categories) : Controller
{
}
```

Add a GET action to display the page:

```csharp
[HttpGet]
public async Task<IActionResult> Index()
{
    // Ask repository for data.
    // Convert data to the page model.
    return View(model);
}
```

Add POST actions for changes:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Save(CategoryFormViewModel model)
{
    // Validate.
    // Create or update.
    // Save.
    return RedirectToAction(nameof(Index));
}
```

Every action that changes data should normally have:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
```

### Part F: Create the Razor view

Create:

```text
Template.Web/Views/<FeatureName>/Index.cshtml
```

For Categories:

```text
Template.Web/Views/Category/Index.cshtml
```

Start it with:

```cshtml
@model CategoriesModel
@{
    ViewData["Title"] = "Categories";
}
```

Connect a form to the controller:

```cshtml
<form asp-controller="Category" asp-action="Save" method="post">
    <input asp-for="Category.Name" />
    <button type="submit">Save</button>
</form>
```

Do not add these to a normal MVC view:

```text
@page
<html>
<head>
<body>
asp-page-handler
```

The shared layout already owns the HTML document, header, and sidebar.

### Part G: Add navigation

Open:

```text
Template.Web/Views/Shared/_Layout.cshtml
```

Place the link inside the correct role block:

```cshtml
@if (User.IsInRole(RoleConstants.InnovationTeam))
{
    <a asp-controller="Category" asp-action="Index">Categories</a>
}
```

This controls visibility only. The controller's `[Authorize]` attribute provides real security.

### Part H: Build and test

Run:

```powershell
dotnet build Template.sln --no-restore
```

Then start the application with `F5`.

Test all three conditions:

1. Innovation Team can open and use the page.
2. Staff cannot open the page by typing the URL directly.
3. Admin cannot open it unless the requirements explicitly allow it.

## 6. How to finish the Staff area

Work in this order.

### Job 1: Dynamic categories on Submit Idea

Files:

```text
Template.Web/Controllers/IdeaController.cs
Template.Web/Models/Idea/SubmitIdeaModel.cs
Template.Web/Views/Idea/Submit.cshtml
```

Steps:

1. Add a category-option list to `SubmitIdeaModel`.
2. In the GET `Submit` action, load active categories from the database.
3. Put them into the model.
4. Replace hard-coded category radio buttons with a loop over the model list.
5. In POST `Submit`, reload category options before returning an invalid form.
6. Validate that the submitted category ID exists and is active.
7. Build and test.

Checkpoint: deactivating a category removes it from new submissions but does not remove it from old ideas.

### Job 2: Save Draft

Files to create:

```text
Template.Core/Repository/Ideas/IIdeaDraftRepository.cs
Template.Core/Repository/Ideas/IdeaDraftRepository.cs
```

Files to update:

```text
Template.Web/Controllers/IdeaController.cs
Template.Web/Views/Idea/Submit.cshtml
```

Steps:

1. Inspect `InnovationDraft` and decide which form data it stores.
2. Add `SaveDraft` as a POST action.
3. Get the owner from the authenticated claim.
4. Save incomplete fields without applying final-submission validation.
5. Add My Drafts listing.
6. Add Edit Draft and Delete Draft.
7. Revalidate everything when the user finally submits.

Checkpoint: Staff A cannot open Staff B's draft, even by changing an ID in the URL.

### Job 3: Edit an unlocked idea

1. Define which stages/statuses allow editing.
2. Add GET `Edit(Guid id)` with ownership and lock checks.
3. Add POST `Edit` with the same checks repeated.
4. Include `RowVersion` in the form.
5. Catch concurrency conflicts.
6. Add an audit record describing the edit.

Checkpoint: a locked or foreign idea returns 403/404 and cannot be changed by crafted requests.

## 7. How to finish the Innovation Team area

### Job 1: Review workflow service

Create:

```text
Template.Core/Services/IdeaWorkflow/IIdeaWorkflowService.cs
Template.Core/Services/IdeaWorkflow/IdeaWorkflowService.cs
```

Move these rules out of `IdeaWorkflowController`:

- Allowed stage transitions.
- Allowed status transitions.
- Timeline completion/creation.
- Stage history creation.
- Notification creation.
- Concurrency checking.

The controller should become a coordinator:

```csharp
var result = await workflow.ChangeStageAsync(command, currentUserId);
if (!result.Succeeded)
{
    // Show message.
}
return RedirectToAction(nameof(Review), new { id });
```

Checkpoint: it is impossible to jump directly from Submitted to Closed unless the approved business rules say so.

### Job 2: Timeline settings

Create:

```text
Template.Core/Repository/Timelines/ITimelineSettingRepository.cs
Template.Core/Repository/Timelines/TimelineSettingRepository.cs
Template.Web/Models/Timeline/TimelineSettingsModel.cs
Template.Web/Controllers/TimelineController.cs
Template.Web/Views/Timeline/Index.cshtml
```

Seed these defaults:

```text
Idea selection: 30 days
Concept development: 60 days
Experimentation/research: 90 days
Launch after approval: 30 days
```

Only Innovation Team should access this feature.

Checkpoint: changing a default affects newly created stage deadlines, not historical deadlines.

## 8. How to build Notifications correctly

Keep notification creation in a service so every feature uses the same rules.

Create:

```text
Template.Core/Services/Notifications/INotificationService.cs
Template.Core/Services/Notifications/NotificationService.cs
```

Give it simple methods such as:

```csharp
Task NotifyIdeaSubmittedAsync(...);
Task NotifyCommentAddedAsync(...);
Task NotifyStageChangedAsync(...);
```

Then call it from the application workflow instead of manually repeating notification code in several controllers.

Later, add email behind this interface:

```text
Template.Core/Services/Email/IEmailSender.cs
Template.Core/Services/Email/DevelopmentEmailSender.cs
Template.Core/Services/Email/SmtpEmailSender.cs
```

Checkpoint: failure to send email must not delete or undo an already-saved idea.

## 9. How to build Resources

The current Resource page is a mockup. Convert it using the repeatable feature recipe.

Create:

```text
Template.Core/Repository/Resources/IResourceRepository.cs
Template.Core/Repository/Resources/ResourceRepository.cs
Template.Core/Services/Files/IResourceFileService.cs
Template.Core/Services/Files/ResourceFileService.cs
Template.Web/Controllers/ResourceController.cs
```

Update:

```text
Template.Web/Models/Resource/ResourcesModel.cs
Template.Web/Views/Resource/Index.cshtml
```

Rules:

1. Innovation Team can upload/manage.
2. Staff can view/download active resources.
3. Store files outside `wwwroot`.
4. Generate stored filenames on the server.
5. Never trust a path supplied by the browser.
6. Validate file size, extension, and content type.

Checkpoint: `../` in a URL or filename can never escape the storage directory.

## 10. How to build Reports

Create:

```text
Template.Core/Repository/Reports/IReportRepository.cs
Template.Core/Repository/Reports/ReportRepository.cs
Template.Core/Services/Reports/IReportExportService.cs
Template.Core/Services/Reports/ReportExportService.cs
Template.Web/Controllers/ReportController.cs
```

Update:

```text
Template.Web/Models/Report/ReportsModel.cs
Template.Web/Views/Report/Index.cshtml
```

Build in this order:

1. Make the filtered HTML report work.
2. Verify totals against SQL/database records.
3. Add Excel export.
4. Add PDF export.
5. Add audit logging for exports.

Do not start with PDF. If the underlying query is wrong, every export will also be wrong.

Checkpoint: the screen, Excel file, and PDF show the same totals for identical filters.

## 11. How to harden Admin pages

Open every action in:

```text
Template.Web/Controllers/AccountController.cs
Template.Web/Controllers/RoleController.cs
Template.Web/Controllers/AuditLogController.cs
```

Add or verify Admin authorization.

For each POST action:

1. Add `[HttpPost]`.
2. Add `[ValidateAntiForgeryToken]`.
3. Validate the model.
4. Check that the target user exists.
5. Prevent removal/disablement of the final administrator.
6. Record an audit event.
7. Redirect after success.

Test using Staff and Innovation Team accounts by entering admin URLs manually.

Checkpoint: a hidden admin menu is not enough; direct requests must return 403.

## 12. When and how to make a migration

Make a migration only when the database structure changes.

Examples that need a migration:

- Adding an entity property that must be stored.
- Adding a new entity/table.
- Changing a column type or relationship.
- Adding an index or concurrency token configuration.

Examples that do not need a migration:

- Changing CSS.
- Adding a controller.
- Adding a repository.
- Changing a Razor view.
- Fixing a query that uses the same columns.

To create a migration:

```powershell
dotnet ef migrations add ClearMigrationName --project Template.Data --startup-project Template.Web
```

Inspect the generated file before applying it.

Apply locally:

```powershell
dotnet ef database update --project Template.Data --startup-project Template.Web
```

Never delete an already-deployed migration just because it is inconvenient.

## 13. Your testing ladder

Test in this order after every feature.

### Level 1: Compiler

```powershell
dotnet build Template.sln --no-restore
```

Fix errors from top to bottom. Later errors may be caused by the first one.

### Level 2: Startup

Press `F5` and confirm the browser reaches login.

### Level 3: Happy path

Use the intended role and perform the normal action.

Example: Innovation Team creates a category.

### Level 4: Bad input

Try:

- Empty required fields.
- Duplicate values.
- Invalid IDs.
- Oversized text or files.

### Level 5: Wrong role

Log in as another role and type the URL directly.

### Level 6: Wrong owner

Use two Staff accounts. Try to open one user's record while logged in as the other.

### Level 7: Refresh and duplicate submission

After a successful POST, refresh. The operation should not repeat.

## 14. How to read common errors

### “Unable to resolve service”

Meaning: you created a repository/service but did not register it.

Look in:

```text
Template.Core/CoreServicesRegistration.cs
```

### “The view was not found”

If the action is `CategoryController.Index`, ASP.NET expects:

```text
Template.Web/Views/Category/Index.cshtml
```

### HTTP 403

The user is logged in but does not have permission.

Check the role and `[Authorize]` rule. Do not remove authorization merely to make the page open.

### HTTP 404

The route or record was not found. Check controller name, action name, ID, and ownership filter.

### Database migration/seeding failure

1. Start LocalDB.
2. Check `Template.Web/appsettings.json`.
3. Run the migration command.
4. Read the innermost exception.

### DLL is being used by another process

Visual Studio or the running application has locked build output.

1. Stop debugging with `Shift+F5`.
2. Build again.
3. Do not terminate Visual Studio unless you choose to close it.

## 15. The rule for knowing when a feature is finished

Do not say “finished” because the page looks attractive.

A feature is finished only when:

1. Correct role can use it.
2. Incorrect roles cannot use it directly.
3. Correct data is saved and reloaded.
4. Invalid data is rejected with a useful message.
5. Personal data is ownership-filtered.
6. Relevant audit and notification records are created.
7. Empty and error states look acceptable.
8. Mobile layout works.
9. Build has zero errors.
10. Happy path and abuse-path tests pass.

## 16. What to do next in this repository

Follow these exact checkpoints:

1. Run the build and fix the current syntax/build issue before adding more files.
2. Finish the active Category repository/controller/view work.
3. Log in as `innovation` and test create, edit, filter, deactivate, and delete behavior.
4. Log in as `staff` and prove `/Category` is forbidden.
5. Connect active categories to Submit Idea.
6. Implement Staff drafts.
7. Extract the workflow service.
8. Implement Timeline Settings.
9. Continue through `DEVELOPMENT_PLAN.md` one phase at a time.

After every checkpoint, write down:

```text
What I changed:
Files I changed:
Command I ran:
Expected result:
Actual result:
Remaining issue:
```

That short log makes debugging much easier and gives the next developer or AI precise context.

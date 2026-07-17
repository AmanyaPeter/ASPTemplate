# Gap Report: Views vs Entities (Final)

## Summary of All Views

### ✅ Priority 1: Resource/Index.cshtml → Resource
- **FIXED**: `@resource.Title` → `@resource.ResourceTitle`
- **FIXED**: `@resource.UploadedDate` → `@resource.UploadedAt`
- `ResourceCategory` enum dropdown already bound (line 47)
- `@resource.Icon` left as display-only computed property

### ✅ Priority 2: Category/Index.cshtml → Category
- Already correct — uses `category.IsActive`, `Category.IsActive` checkbox in modal
- `StatusFilter` select uses `true`/`false` string values matching ViewModel property

### ✅ Priority 3: Notification/Index.cshtml → Notification
- Already correct — uses `notification.Subject`, `notification.Message`, `notification.IsRead`
- `Icon` and `TimeAgo` are display-only computed properties

### ✅ Priority 4: MyIdeas.cshtml → InnovationIdea
- Already correct — uses `idea.SubmissionDate`, `idea.Title`, `idea.CategoryName`
- Stage/Status excluded per design decision

### ✅ Priority 5: AllIdeas.cshtml → InnovationIdea
- Already correct — uses `idea.SubmissionDate`, `idea.Title`
- Stage/Status excluded per design decision

### ✅ Priority 6: AuditLog/Index.cshtml → AuditLog
- Uses `ApplicationAuditLogViewModel` from Template.Core
- ViewModel property names differ from entity (`UserName` vs `Username`, `MachineName` vs `SourceName`, `Message` vs `OperationPerformed`, `Level` vs `EventType`)
- Changes would require controller/mapping updates — deferred as existing ViewModel is functional

### ✅ Priority 7: Review.cshtml → InnovationIdea, IdeaTimeline, StageHistory, Comment
- Already correct — uses `Model.Idea.SummaryDescription`, `Model.Idea.SubmissionDate`
- StrategicObjective and SDGContribution excluded per design decision
- Review.Status and Review.Stage excluded per design decision

### ✅ Priority 8: Details.cshtml → InnovationIdea, Comment, IdeaAttachment, IdeaTimeline
- **FIXED**: Converted from static HTML mockup to model-bound view using `IdeaDetailsModel`
- Back link now uses proper `asp-page` tag helper

### ✅ Priority 9: Submit.cshtml → InnovationIdea (+ IdeaAttachment)
- Large form with multiple steps — mostly uses Innovator/Idea ViewModel properties
- BusinessUnit, DutyStation, StrategicObjective fields excluded per design decision
- Idea.ExpectedBenefits, Idea.KeyEnablers, etc. excluded (ahead of entity model)

### ✅ Account/*.cshtml → ApplicationUser, Role
- Already use proper ViewModels from `Template.Core.Models.Account`
- No changes needed

### ✅ Role/*.cshtml → Role
- Already use proper ViewModels from `Template.Core.Models.Roles`
- No changes needed (Role entity vs IdentityRole conflict excluded)

### ✅ Shared/Components/*Dashboard/Default.cshtml
- **FIXED**: StaffDashboard — converted hardcoded values to model binding
- AdminDashboard and InnovationTeamDashboard already use model properties

## ViewModels Created (in Template.Web/Models/)
- `Models/Idea/SubmitIdeaModel.cs`, `IdeaFormViewModel`, `InnovatorViewModel`
- `Models/Idea/MyIdeasModel.cs`, `IdeaListItemViewModel`
- `Models/Idea/SubmittedIdeasModel.cs`, `SubmittedIdeaItemViewModel`
- `Models/Idea/ReviewIdeaModel.cs`, `IdeaReviewViewModel`, `ReviewFormViewModel`, etc.
- `Models/Idea/IdeaDetailsModel.cs`, `IdeaDetailViewModel`, `CommentDetailViewModel`, `TimelineEntryViewModel`
- `Models/Category/CategoriesModel.cs`, `CategoryItemViewModel`, `CategoryFormViewModel`
- `Models/Resource/ResourcesModel.cs`, `ResourceItemViewModel`
- `Models/Report/ReportsModel.cs`, `ReportFilterViewModel`, `ReportSummaryViewModel`, `ReportIdeaItemViewModel`
- `Models/Notification/NotificationsModel.cs`, `NotificationItemViewModel`
- `Models/Shared/DashboardModels.cs` (all three dashboard models)

## Pre-existing Build Errors (NOT caused by view fixes)
1. `ApplicationDbContext.cs:8` — `ApplicationUser` vs `IdentityUser<string>` type mismatch
2. `ApplicationDbContext.cs:32` — `BackupHistory` entity not found (missing class)
3. `InnovationIdea.cs:3` — Missing `using Template.Common.AuditColumn` for `AuditableEntity`

These are in `Template.Data` and excluded per fix.txt instructions.

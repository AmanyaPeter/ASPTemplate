using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Models.Idea;

namespace Template.Web.Controllers;

[Authorize]
public class IdeaController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment) : Controller
{
    private const int PageSize = 10;
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };

    [HttpGet]
    public async Task<IActionResult> Submit(Guid? id)
    {
        var user = await CurrentUserAsync();
        if (user == null) return Challenge();

        var model = new SubmitIdeaModel
        {
            Id = id,
            Innovator = new InnovatorViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                BusinessUnit = user.BusinessUnit,
                DutyStation = user.Station,
                Age = user.AgeBracket,
                Gender = user.Gender,
                Rank = user.Title
            }
        };

        if (id.HasValue)
        {
            var idea = await db.InnovationIdeas.AsNoTracking()
                .Include(i => i.Category)
                .SingleOrDefaultAsync(i => i.Id == id && i.SubmitterId == user.Id && !i.IsDeleted);
            if (idea == null) return NotFound();
            if (idea.IsLocked || idea.IsRetracted) return Forbid();

            model.SubmissionType = idea.SubmissionType;
            model.Idea = new IdeaFormViewModel
            {
                Title = idea.Title,
                SummaryDescription = idea.SummaryDescription,
                ProblemStatement = idea.ProblemStatement,
                ProposedSolution = idea.ProposedSolution,
                Category = idea.Category?.Name,
                ExpectedBenefits = idea.ExpectedBenefits,
                KeyEnablers = idea.KeyEnablers,
                ImplementationApproach = idea.ImplementationApproach,
                ImpactIndicators = idea.ImpactIndicators,
                StrategicObjective = idea.StrategicObjective
            };
            model.Innovator.TeamMemberNames = idea.TeamMemberNames;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Submit(SubmitIdeaModel model)
    {
        var user = await CurrentUserAsync();
        if (user == null) return Challenge();

        if (!ModelState.IsValid) return View(model);

        var category = await db.Categories.FirstOrDefaultAsync(
            c => c.IsActive && c.Name == model.Idea.Category);
        if (category == null)
        {
            ModelState.AddModelError("Idea.Category", "Please select an available category.");
            return View(model);
        }

        InnovationIdea idea;
        if (model.Id.HasValue)
        {
            idea = await db.InnovationIdeas.SingleOrDefaultAsync(
                i => i.Id == model.Id && i.SubmitterId == user.Id && !i.IsDeleted) ?? throw new InvalidOperationException();
            if (idea.IsLocked || idea.IsRetracted) return Forbid();
        }
        else
        {
            idea = new InnovationIdea
            {
                Id = Guid.NewGuid(),
                ReferenceNumber = await NextReferenceAsync(),
                SubmissionDate = DateTime.UtcNow,
                SubmitterId = user.Id,
                Submitter = user,
                CurrentStage = "Submitted",
                CurrentStatus = "Under Review",
                RowVersion = Array.Empty<byte>()
            };
            db.InnovationIdeas.Add(idea);
        }

        idea.SubmissionType = model.SubmissionType;
        idea.Title = model.Idea.Title!.Trim();
        idea.SummaryDescription = model.Idea.SummaryDescription!.Trim();
        idea.ProblemStatement = model.Idea.ProblemStatement!.Trim();
        idea.ProposedSolution = model.Idea.ProposedSolution!.Trim();
        idea.ExpectedBenefits = model.Idea.ExpectedBenefits?.Trim();
        idea.KeyEnablers = model.Idea.KeyEnablers?.Trim();
        idea.ImplementationApproach = model.Idea.ImplementationApproach?.Trim();
        idea.ImpactIndicators = model.Idea.ImpactIndicators?.Trim();
        idea.StrategicObjective = model.Idea.StrategicObjective?.Trim();
        idea.TeamMemberNames = model.SubmissionType == "team" ? model.Innovator.TeamMemberNames?.Trim() : null;
        idea.TeamCompositionJson = model.SubmissionType == "team"
            ? JsonSerializer.Serialize(Request.Form
                .Where(x => x.Key.StartsWith("Team", StringComparison.OrdinalIgnoreCase) && x.Key != "Innovator.TeamMemberNames")
                .ToDictionary(x => x.Key, x => x.Value.ToString()))
            : null;
        idea.CategoryId = category.Id;
        idea.Category = category;
        idea.SubmitterAgeBracket = user.AgeBracket;
        idea.ModifiedBy = user.UserName ?? user.Id.ToString();
        idea.ModifiedDate = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await SaveAttachmentsAsync(idea, user, model.Attachments);

        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            IdeaId = idea.Id,
            Idea = idea,
            Type = NotificationType.IdeaSubmitted,
            Subject = "Idea submitted",
            Message = $"Your idea \"{idea.Title}\" was submitted successfully.",
            LinkUrl = Url.Action(nameof(Details), new { id = idea.Id }),
            CreatedDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Idea {idea.ReferenceNumber} saved successfully.";
        return RedirectToAction(nameof(Details), new { id = idea.Id });
    }

    [HttpGet]
    public async Task<IActionResult> MyIdeas(string? searchTerm, string? statusFilter, string? categoryFilter, int page = 1)
    {
        var userId = CurrentUserId();
        if (userId == null) return Challenge();
        page = Math.Max(page, 1);

        var query = db.InnovationIdeas.AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.SubmitterId == userId && !i.IsDeleted);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(i => i.Title.Contains(searchTerm));
        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(i => i.CurrentStatus == statusFilter);
        if (!string.IsNullOrWhiteSpace(categoryFilter))
            query = query.Where(i => i.Category != null && i.Category.Name == categoryFilter);

        var count = await query.CountAsync();
        var ideas = await query.OrderByDescending(i => i.SubmissionDate)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(i => new IdeaListItemViewModel
            {
                Id = i.Id, Title = i.Title, CategoryName = i.Category != null ? i.Category.Name : null,
                Stage = i.CurrentStage, Status = i.IsRetracted ? "Retracted" : i.CurrentStatus,
                SubmissionDate = i.SubmissionDate
            }).ToListAsync();

        return View(new MyIdeasModel
        {
            SearchTerm = searchTerm, StatusFilter = statusFilter, CategoryFilter = categoryFilter,
            Ideas = ideas, CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize))
        });
    }

    [Authorize(Roles = "Admin,InnovationTeam")]
    [HttpGet]
    public async Task<IActionResult> AllIdeas(
        string? searchTerm, string? statusFilter, string? categoryFilter,
        string? departmentFilter, DateTime? dateFrom, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = db.InnovationIdeas.AsNoTracking().Include(i => i.Category).Include(i => i.Submitter)
            .Where(i => !i.IsDeleted && !i.IsRetracted);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(i => i.Title.Contains(searchTerm) || i.Submitter.FullName.Contains(searchTerm));
        if (!string.IsNullOrWhiteSpace(statusFilter)) query = query.Where(i => i.CurrentStatus == statusFilter);
        if (!string.IsNullOrWhiteSpace(categoryFilter))
            query = query.Where(i => i.Category != null && i.Category.Name == categoryFilter);
        if (!string.IsNullOrWhiteSpace(departmentFilter))
            query = query.Where(i => i.Submitter.BusinessUnit == departmentFilter);
        if (dateFrom.HasValue) query = query.Where(i => i.SubmissionDate >= dateFrom.Value);

        var count = await query.CountAsync();
        var ideas = await query.OrderByDescending(i => i.SubmissionDate)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(i => new SubmittedIdeaItemViewModel
            {
                Id = i.Id, Title = i.Title, Submitter = i.Submitter.FullName,
                Department = i.Submitter.BusinessUnit, CategoryName = i.Category != null ? i.Category.Name : null,
                Stage = i.CurrentStage, Status = i.CurrentStatus, SubmissionDate = i.SubmissionDate,
                NeedsReview = i.CurrentStatus == "Under Review"
            }).ToListAsync();
        return View(new SubmittedIdeasModel
        {
            SearchTerm = searchTerm, StatusFilter = statusFilter, CategoryFilter = categoryFilter,
            DepartmentFilter = departmentFilter, DateFrom = dateFrom, Ideas = ideas,
            CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize))
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var idea = await db.InnovationIdeas.AsNoTracking()
            .Include(i => i.Submitter).Include(i => i.Attachments)
            .Include(i => i.Comments).ThenInclude(c => c.User)
            .Include(i => i.Timeline)
            .SingleOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        if (idea == null) return NotFound();
        if (!CanAccess(idea)) return Forbid();

        return View(ToDetailsModel(idea));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Guid id, string newComment)
    {
        if (string.IsNullOrWhiteSpace(newComment)) return RedirectToAction(nameof(Details), new { id });
        var user = await CurrentUserAsync();
        var idea = await db.InnovationIdeas.Include(i => i.Submitter).SingleOrDefaultAsync(i => i.Id == id);
        if (user == null || idea == null) return NotFound();
        if (!CanAccess(idea)) return Forbid();

        db.Comments.Add(new Comment
        {
            Id = Guid.NewGuid(), IdeaId = idea.Id, Idea = idea, UserId = user.Id, User = user,
            CommentText = newComment.Trim(), IsInternal = false, CreatedBy = user.UserName ?? "system"
        });
        if (idea.SubmitterId != user.Id)
        {
            db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(), UserId = idea.SubmitterId, User = idea.Submitter,
                IdeaId = idea.Id, Idea = idea, Type = NotificationType.CommentAdded,
                Subject = "New comment on your idea", Message = newComment.Trim(),
                LinkUrl = Url.Action(nameof(Details), new { id }), CreatedDate = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,InnovationTeam")]
    [HttpGet]
    public async Task<IActionResult> Review(Guid id)
    {
        var idea = await db.InnovationIdeas.AsNoTracking()
            .Include(i => i.Submitter).Include(i => i.Attachments)
            .Include(i => i.Comments).ThenInclude(c => c.User)
            .SingleOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        if (idea == null) return NotFound();
        return View(await ToReviewModelAsync(idea));
    }

    [Authorize(Roles = "Admin,InnovationTeam")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewIdeaModel model)
    {
        var user = await CurrentUserAsync();
        var idea = await db.InnovationIdeas.Include(i => i.Submitter)
            .SingleOrDefaultAsync(i => i.Id == model.Idea.Id && !i.IsDeleted);
        if (user == null || idea == null) return NotFound();

        var oldStage = idea.CurrentStage;
        var oldStatus = idea.CurrentStatus;
        if (!string.IsNullOrWhiteSpace(model.Review.Stage)) idea.CurrentStage = model.Review.Stage;
        if (!string.IsNullOrWhiteSpace(model.Review.Status)) idea.CurrentStatus = model.Review.Status;
        if (Guid.TryParse(model.Review.AssignedReviewerId, out var reviewerId)) idea.AssignedReviewerId = reviewerId;
        idea.IsLocked = true;
        if (idea.CurrentStatus is "Approved" or "Declined")
        {
            idea.DecisionDate = DateTime.UtcNow;
            idea.DecisionReason = model.Review.Notes;
        }

        db.StageHistories.Add(new StageHistory
        {
            IdeaId = idea.Id, Idea = idea, PreviousStage = oldStage, NewStage = idea.CurrentStage,
            PreviousStatus = oldStatus, NewStatus = idea.CurrentStatus, ChangedById = user.Id,
            ChangedBy = user, ChangeReason = model.Review.Notes, ChangedAt = DateTime.UtcNow,
            CreatedBy = user.UserName ?? "system"
        });
        if (model.Review.TimelineDate.HasValue)
        {
            db.IdeaTimelines.Add(new IdeaTimeline
            {
                Id = Guid.NewGuid(), IdeaId = idea.Id, Idea = idea,
                Stage = ParseStage(idea.CurrentStage), StartDate = DateTime.UtcNow,
                DeadlineDate = model.Review.TimelineDate.Value, CreatedBy = user.UserName ?? "system"
            });
        }
        if (!string.IsNullOrWhiteSpace(model.NewComment))
        {
            db.Comments.Add(new Comment
            {
                Id = Guid.NewGuid(), IdeaId = idea.Id, Idea = idea, UserId = user.Id, User = user,
                CommentText = model.NewComment.Trim(), CreatedBy = user.UserName ?? "system"
            });
        }
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(), UserId = idea.SubmitterId, User = idea.Submitter, IdeaId = idea.Id, Idea = idea,
            Type = NotificationType.StatusChanged, Subject = "Your idea was updated",
            Message = $"Status: {idea.CurrentStatus}; Stage: {idea.CurrentStage}.",
            LinkUrl = Url.Action(nameof(Details), new { id = idea.Id }), CreatedDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Review saved.";
        return RedirectToAction(nameof(Review), new { id = idea.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retract(Guid id)
    {
        var userId = CurrentUserId();
        var idea = await db.InnovationIdeas.SingleOrDefaultAsync(i => i.Id == id && i.SubmitterId == userId);
        if (idea == null) return NotFound();
        if (idea.IsLocked) return Forbid();
        idea.IsRetracted = true;
        idea.CurrentStatus = "Retracted";
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(MyIdeas));
    }

    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(Guid id)
    {
        var attachment = await db.IdeaAttachments.Include(a => a.Idea).SingleOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (attachment == null || !CanAccess(attachment.Idea)) return NotFound();
        var fullPath = Path.GetFullPath(Path.Combine(environment.WebRootPath, attachment.FilePath.TrimStart('/', '\\')));
        var uploadRoot = Path.GetFullPath(Path.Combine(environment.WebRootPath, "uploads", "ideas"));
        if (!fullPath.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();
        attachment.DownloadCount++;
        await db.SaveChangesAsync();
        return PhysicalFile(fullPath, attachment.MimeType, attachment.FileName);
    }

    private Guid? CurrentUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private Task<ApplicationUser?> CurrentUserAsync() => userManager.GetUserAsync(User);

    private bool CanAccess(InnovationIdea idea) =>
        idea.SubmitterId == CurrentUserId() || User.IsInRole("Admin") || User.IsInRole("InnovationTeam");

    private async Task<string> NextReferenceAsync() =>
        $"IMTS-{DateTime.UtcNow:yyyy}-{(await db.InnovationIdeas.CountAsync() + 1):D5}";

    private async Task SaveAttachmentsAsync(InnovationIdea idea, ApplicationUser user, IEnumerable<IFormFile>? files)
    {
        if (files == null) return;
        var root = Path.Combine(environment.WebRootPath, "uploads", "ideas", idea.Id.ToString("N"));
        Directory.CreateDirectory(root);
        foreach (var file in files.Where(f => f.Length > 0))
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension) || file.Length > 10 * 1024 * 1024)
                continue;
            var storedName = $"{Guid.NewGuid():N}{extension}";
            await using var stream = System.IO.File.Create(Path.Combine(root, storedName));
            await file.CopyToAsync(stream);
            db.IdeaAttachments.Add(new IdeaAttachment
            {
                Id = Guid.NewGuid(), IdeaId = idea.Id, Idea = idea,
                FileName = Path.GetFileName(file.FileName),
                FilePath = Path.Combine("uploads", "ideas", idea.Id.ToString("N"), storedName),
                FileSize = file.Length, FileType = extension.TrimStart('.'),
                MimeType = file.ContentType ?? "application/octet-stream",
                UploadedById = user.Id, UploadedBy = user, CreatedBy = user.UserName ?? "system"
            });
        }
        await db.SaveChangesAsync();
    }

    private static IdeaDetailsModel ToDetailsModel(InnovationIdea idea) => new()
    {
        Idea = new IdeaDetailViewModel
        {
            Id = idea.Id, Title = idea.Title, Submitter = idea.Submitter.FullName,
            SubmissionDate = idea.SubmissionDate, SummaryDescription = idea.SummaryDescription,
            ProblemStatement = idea.ProblemStatement, ProposedSolution = idea.ProposedSolution,
            Stage = idea.CurrentStage, Status = idea.IsRetracted ? "Retracted" : idea.CurrentStatus,
            Attachments = idea.Attachments.Where(a => !a.IsDeleted).Select(a => new AttachmentViewModel
            {
                Id = a.Id, FileName = a.FileName, FilePath = a.FilePath, Icon = "paperclip"
            }).ToList()
        },
        Comments = idea.Comments.Where(c => !c.IsDeleted).OrderBy(c => c.CreatedDate).Select(c => new CommentDetailViewModel
        {
            Author = c.User.FullName, Role = c.User.Title, Avatar = Initials(c.User.FullName),
            Text = c.CommentText, CreatedAt = c.CreatedDate, TimeAgo = RelativeTime(c.CreatedDate)
        }).ToList(),
        TimelineEntries = idea.Timeline.OrderBy(t => t.StartDate).Select(t => new TimelineEntryViewModel
        {
            StageName = t.Stage.ToString(), Date = t.ActualCompletionDate ?? t.StartDate
        }).ToList()
    };

    private async Task<ReviewIdeaModel> ToReviewModelAsync(InnovationIdea idea) => new()
    {
        Idea = new IdeaReviewViewModel
        {
            Id = idea.Id, Title = idea.Title, Submitter = idea.Submitter.FullName,
            SubmitterBusinessUnit = idea.Submitter.BusinessUnit, SubmissionDate = idea.SubmissionDate,
            SummaryDescription = idea.SummaryDescription, ProblemStatement = idea.ProblemStatement,
            ProposedSolution = idea.ProposedSolution,
            Attachments = idea.Attachments.Where(a => !a.IsDeleted).Select(a => new AttachmentReviewViewModel
            { Id = a.Id, FileName = a.FileName, Icon = "paperclip" }).ToList()
        },
        Comments = idea.Comments.Where(c => !c.IsDeleted).OrderBy(c => c.CreatedDate).Select(c => new CommentReviewViewModel
        {
            Author = c.User.FullName, Role = c.User.Title, Avatar = Initials(c.User.FullName),
            Text = c.CommentText, TimeAgo = RelativeTime(c.CreatedDate)
        }).ToList(),
        Review = new ReviewFormViewModel
        {
            Status = idea.CurrentStatus, Stage = idea.CurrentStage,
            AssignedReviewerId = idea.AssignedReviewerId?.ToString()
        },
        Reviewers = await userManager.GetUsersInRoleAsync("InnovationTeam")
            .ContinueWith(t => t.Result.Select(u => new ReviewerOptionViewModel
            { Id = u.Id.ToString(), FullName = u.FullName }).ToList())
    };

    private static IdeaStage ParseStage(string stage) =>
        Enum.TryParse<IdeaStage>(stage.Replace(" ", ""), true, out var value) ? value : IdeaStage.Submitted;

    private static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => p[0])).ToUpperInvariant();

    private static string RelativeTime(DateTime value)
    {
        var elapsed = DateTime.UtcNow - value.ToUniversalTime();
        if (elapsed.TotalMinutes < 1) return "just now";
        if (elapsed.TotalHours < 1) return $"{(int)elapsed.TotalMinutes}m ago";
        if (elapsed.TotalDays < 1) return $"{(int)elapsed.TotalHours}h ago";
        return $"{(int)elapsed.TotalDays}d ago";
    }
}

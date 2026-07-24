using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Template.Common.Enums;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Models.Idea;

namespace Template.Web.Controllers;

[Authorize]
public class IdeaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private const int PageSize = 10;
    private const long MaximumAttachmentSize = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedAttachmentExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };

    public IdeaController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [Authorize(Roles = RoleConstants.Staff)]
    [HttpGet]
    public async Task<IActionResult> Submit(Guid? id)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Forbid();

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
                Rank = user.JobTitle
            }
        };

        if (id.HasValue)
        {
            var idea = await _context.InnovationIdeas
                .AsNoTracking()
                .Include(i => i.Category)
                .SingleOrDefaultAsync(i => i.Id == id.Value && i.SubmitterId == user.Id && !i.IsDeleted);
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

    [Authorize(Roles = RoleConstants.Staff)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Submit(SubmitIdeaModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Forbid();

        if (!ModelState.IsValid) return View(model);

        var category = await _context.Categories.FirstOrDefaultAsync(
            c => c.IsActive && c.Name == model.Idea.Category);
        if (category == null)
        {
            ModelState.AddModelError("Idea.Category", "Please select an available category.");
            return View(model);
        }

        InnovationIdea idea;
        bool isNew = false;

        if (model.Id.HasValue)
        {
            idea = await _context.InnovationIdeas
                .SingleOrDefaultAsync(i => i.Id == model.Id && i.SubmitterId == user.Id && !i.IsDeleted)
                ?? throw new InvalidOperationException("Idea not found.");
            if (idea.IsLocked || idea.IsRetracted) return Forbid();
        }
        else
        {
            idea = new InnovationIdea
            {
                Id = Guid.NewGuid(),
                ReferenceNumber = await GenerateReferenceNumberAsync(),
                SubmissionDate = DateTime.UtcNow,
                SubmitterId = user.Id,
                CurrentStage = nameof(IdeaStage.Submitted),
                CurrentStatus = nameof(IdeaStatus.UnderReview),
                RowVersion = new byte[8]
            };
            _context.InnovationIdeas.Add(idea);
            isNew = true;
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
        idea.CategoryId = category.Id;
        idea.SubmitterAgeBracket = user.AgeBracket;
        idea.ModifiedBy = user.UserName ?? user.Id.ToString();
        idea.ModifiedDate = DateTime.UtcNow;

        if (isNew)
        {
            _context.IdeaTimelines.Add(new IdeaTimeline
            {
                Id = Guid.NewGuid(),
                IdeaId = idea.Id,
                Idea = idea,
                StageId = (int)IdeaStage.Submitted,
                Stage = IdeaStage.Submitted,
                StartDate = idea.SubmissionDate,
                DeadlineDate = idea.SubmissionDate.AddDays(30),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = user.Id.ToString()
            });
        }

        await _context.SaveChangesAsync();

        if (model.Attachments != null && model.Attachments.Count > 0)
        {
            await SaveAttachmentsAsync(idea, user, model.Attachments);
        }

        // Create notification for new submission
        if (isNew)
        {
            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IdeaId = idea.Id,
                Type = NotificationType.IdeaSubmitted,
                Subject = "Idea submitted",
                Message = $"Your idea \"{idea.Title}\" was submitted successfully.",
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = $"Idea {idea.ReferenceNumber} saved successfully.";
        return RedirectToAction(nameof(Details), new { id = idea.Id });
    }

    [Authorize(Roles = RoleConstants.Staff)]
    [HttpGet]
    public async Task<IActionResult> MyIdeas(
        string? searchTerm, string? statusFilter, string? categoryFilter, int page = 1)
    {
        if (!TryGetCurrentUserId(out var userId)) return Forbid();
        page = Math.Max(page, 1);

        var query = _context.InnovationIdeas.AsNoTracking()
            .Where(i => i.SubmitterId == userId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(i => i.Title.Contains(searchTerm) || i.ReferenceNumber.Contains(searchTerm));
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
                SubmissionDate = i.SubmissionDate, IsRetracted = i.IsRetracted
            }).ToListAsync();

        return View(new MyIdeasModel
        {
            SearchTerm = searchTerm, StatusFilter = statusFilter, CategoryFilter = categoryFilter,
            Ideas = ideas, CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize))
        });
    }

    [Authorize(Roles = $"{RoleConstants.ItAdmin},{RoleConstants.InnovationTeam}")]
    [HttpGet]
    public async Task<IActionResult> AllIdeas(
        string? searchTerm, string? statusFilter, string? categoryFilter,
        string? departmentFilter, DateTime? dateFrom, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = _context.InnovationIdeas.AsNoTracking()
            .Include(i => i.Category).Include(i => i.Submitter)
            .Where(i => !i.IsDeleted && !i.IsRetracted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(i => i.Title.Contains(searchTerm) || i.Submitter.FullName.Contains(searchTerm));
        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(i => i.CurrentStatus == statusFilter);
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
                NeedsReview = i.CurrentStatus == nameof(IdeaStatus.UnderReview)
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
        if (!TryGetCurrentUserId(out var userId)) return Forbid();
        var isInnovationTeam = User.IsInRole(RoleConstants.InnovationTeam);

        var idea = await _context.InnovationIdeas
            .AsNoTracking()
            .Include(i => i.Submitter)
            .Include(i => i.Attachments)
            .Include(i => i.Comments.Where(c => !c.IsDeleted && (!c.IsInternal || isInnovationTeam))).ThenInclude(c => c.User)
            .Include(i => i.Timeline)
            .Where(i => !i.IsDeleted && (isInnovationTeam || i.SubmitterId == userId))
            .Select(i => new IdeaDetailsModel
            {
                Idea = new IdeaDetailViewModel
                {
                    Id = i.Id,
                    Title = i.Title,
                    Submitter = i.Submitter.FullName,
                    SubmissionDate = i.SubmissionDate,
                    SummaryDescription = i.SummaryDescription,
                    ProblemStatement = i.ProblemStatement,
                    ProposedSolution = i.ProposedSolution,
                    Stage = i.CurrentStage,
                    Status = i.IsRetracted ? "Retracted" : i.CurrentStatus,
                    Attachments = i.Attachments.Where(a => !a.IsDeleted).Select(a => new AttachmentViewModel
                    {
                        Id = a.Id, FileName = a.FileName, FilePath = a.FilePath, Icon = "paperclip"
                    }).ToList()
                },
                TimelineEntries = i.Timeline.OrderBy(t => t.StartDate).Select(t => new TimelineEntryViewModel
                {
                    StageName = t.Stage.ToString(), Date = t.StartDate
                }).ToList(),
                Comments = i.Comments.OrderBy(c => c.CreatedDate).Select(c => new CommentDetailViewModel
                {
                    Avatar = c.User.FullName.Substring(0, 1),
                    Author = c.User.FullName,
                    Role = "Participant",
                    TimeAgo = c.CreatedDate.ToString("dd MMM yyyy HH:mm"),
                    Text = c.CommentText,
                    CreatedAt = c.CreatedDate
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (idea == null) return NotFound();
        return View(idea);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Guid id, string newComment, bool isInternal = false)
    {
        if (string.IsNullOrWhiteSpace(newComment)) return RedirectToAction(nameof(Details), new { id });

        if (!TryGetCurrentUserId(out var userId)) return Forbid();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var idea = await _context.InnovationIdeas
            .Include(i => i.Submitter)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (user == null || idea == null) return NotFound();

        // Check access: submitter can comment on own ideas, Innovation Team can comment on any
        bool canAccess = idea.SubmitterId == userId || User.IsInRole(RoleConstants.InnovationTeam);
        if (!canAccess) return Forbid();

        _context.Comments.Add(new Comment
        {
            Id = Guid.NewGuid(),
            IdeaId = idea.Id,
            Idea = idea,
            UserId = user.Id,
            User = user,
            CommentText = newComment.Trim(),
            IsInternal = isInternal && User.IsInRole(RoleConstants.InnovationTeam),
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id.ToString()
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Comment added.";
        return RedirectToAction(nameof(Details), new { id });
    }

    #region Helpers

    private bool TryGetCurrentUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        if (!TryGetCurrentUserId(out var userId)) return null;
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    private async Task<string> GenerateReferenceNumberAsync()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.InnovationIdeas.CountAsync() + 1;
        return $"IMTS-{datePart}-{count:D4}";
    }

    private async Task SaveAttachmentsAsync(InnovationIdea idea, ApplicationUser user, List<IFormFile> attachments)
    {
        var uploadRoot = Path.Combine(_environment.ContentRootPath, "App_Data", "IdeaAttachments", idea.Id.ToString("N"));
        var storedFiles = new List<string>();

        try
        {
            if (attachments.Count > 0) Directory.CreateDirectory(uploadRoot);

            foreach (var attachment in attachments)
            {
                if (attachment.Length == 0 || attachment.Length > MaximumAttachmentSize) continue;

                var extension = Path.GetExtension(attachment.FileName);
                if (!AllowedAttachmentExtensions.Contains(extension)) continue;

                var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var storedPath = Path.Combine(uploadRoot, storedFileName);

                await using (var stream = System.IO.File.Create(storedPath))
                {
                    await attachment.CopyToAsync(stream);
                }

                storedFiles.Add(storedPath);

                _context.IdeaAttachments.Add(new IdeaAttachment
                {
                    Id = Guid.NewGuid(),
                    IdeaId = idea.Id,
                    Idea = idea,
                    FileName = Path.GetFileName(attachment.FileName),
                    FilePath = Path.Combine(idea.Id.ToString("N"), storedFileName),
                    FileSize = attachment.Length,
                    FileType = extension.TrimStart('.').ToUpperInvariant(),
                    MimeType = string.IsNullOrWhiteSpace(attachment.ContentType)
                        ? "application/octet-stream"
                        : attachment.ContentType,
                    UploadedById = user.Id,
                    UploadedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user.Id.ToString()
                });
            }

            await _context.SaveChangesAsync();
        }
        catch
        {
            foreach (var storedFile in storedFiles)
            {
                if (System.IO.File.Exists(storedFile))
                    System.IO.File.Delete(storedFile);
            }
            throw;
        }
    }

    #endregion
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Models.Idea;
using Template.Core.Services.Files;
using Microsoft.AspNetCore.SignalR;
using Template.Web.Hubs;

namespace Template.Web.Controllers;

[Authorize]
public class IdeaController(
    ApplicationDbContext context,
    IDatabaseFileService fileService,
    UserManager<ApplicationUser> userManager,
    IHubContext<ImtsHub> hub) : Controller
{
    [Authorize(Roles = RoleConstants.Staff)]
    [HttpGet]
    public async Task<IActionResult> Submit()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Forbid();
        }

        return View(new SubmitIdeaModel
        {
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
        });
    }

    [Authorize(Roles = RoleConstants.Staff)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitIdeaModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(model.Idea.Title) ||
            string.IsNullOrWhiteSpace(model.Idea.SummaryDescription) ||
            string.IsNullOrWhiteSpace(model.Idea.ProblemStatement) ||
            string.IsNullOrWhiteSpace(model.Idea.ProposedSolution))
        {
            ModelState.AddModelError(string.Empty, "Complete all required idea fields.");
            return View(model);
        }

        var attachments = model.Attachments?.Where(file => file.Length > 0).ToList() ?? [];
        var validatedFiles = new List<ValidatedFile>();
        foreach (var attachment in attachments)
        {
            try
            {
                await using var stream = attachment.OpenReadStream();
                validatedFiles.Add(await fileService.ValidateAsync(
                    attachment.FileName, attachment.ContentType, stream, attachment.Length, HttpContext.RequestAborted));
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(nameof(model.Attachments), $"{Path.GetFileName(attachment.FileName)}: {ex.Message}");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = string.IsNullOrWhiteSpace(model.Idea.Category)
            ? null
            : await context.Categories.FirstOrDefaultAsync(item =>
                item.Name == model.Idea.Category);

        var idea = new InnovationIdea
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = $"IMTS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..22].ToUpperInvariant(),
            SubmissionType = model.SubmissionType,
            SubmissionDate = DateTime.UtcNow,
            SubmitterId = user.Id,
            Title = model.Idea.Title.Trim(),
            SummaryDescription = model.Idea.SummaryDescription.Trim(),
            ProblemStatement = model.Idea.ProblemStatement.Trim(),
            ProposedSolution = model.Idea.ProposedSolution.Trim(),
            CategoryId = category?.Id,
            SubmitterAgeBracket = user.AgeBracket,
            CurrentStage = IdeaStage.Submitted,
            CurrentStatus = IdeaStatus.UnderReview,
            RowVersion = new byte[8],
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id.ToString()
        };

        context.InnovationIdeas.Add(idea);
        context.IdeaTimelines.Add(new IdeaTimeline
        {
            Id = Guid.NewGuid(),
            IdeaId = idea.Id,
            Idea = idea,
            StageId = (int)IdeaStage.Submitted,
            Stage = IdeaStage.Submitted,
            StartDate = idea.SubmissionDate,
            DeadlineDate = idea.SubmissionDate.AddDays(30),
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id.ToString().ToString()
        });

        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IdeaId = idea.Id,
            Idea = idea,
            Type = NotificationType.IdeaSubmitted,
            Subject = "Idea submitted",
            Message = $"{idea.ReferenceNumber} was submitted for review.",
            CreatedDate = DateTime.UtcNow
        });

        var innovationTeamMembers = (await userManager.GetUsersInRoleAsync(RoleConstants.InnovationTeam))
            .Where(member => member.IsActive)
            .ToList();
        foreach (var teamMember in innovationTeamMembers)
        {
            context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = teamMember.Id,
                IdeaId = idea.Id,
                Idea = idea,
                Type = NotificationType.IdeaSubmitted,
                Subject = "New idea submitted",
                Message = $"{user.FullName} submitted {idea.ReferenceNumber}: {idea.Title}.",
                LinkUrl = $"/Idea/Details/{idea.Id}",
                CreatedDate = DateTime.UtcNow
            });
        }

        context.EmailOutbox.Add(new EmailOutbox
        {
            Id = Guid.NewGuid(), IdempotencyKey = $"idea-submitted:{idea.Id}",
            Recipient = user.Email!, Subject = "Innovation idea submitted",
            Body = $"{idea.ReferenceNumber} has been submitted for review.",
            CreatedAtUtc = DateTime.UtcNow
        });

        foreach (var file in validatedFiles)
        {
            context.IdeaAttachments.Add(new IdeaAttachment
            {
                Id = Guid.NewGuid(), IdeaId = idea.Id, Idea = idea, FileName = file.OriginalName,
                StorageName = file.StorageName, Content = file.Content, Sha256 = file.Sha256,
                FileSize = file.Size, FileType = Path.GetExtension(file.OriginalName).TrimStart('.').ToUpperInvariant(),
                MimeType = file.MimeType, UploadedById = user.Id, UploadedBy = user,
                CreatedDate = DateTime.UtcNow, CreatedBy = user.Id.ToString()
            });
        }
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        await hub.Clients.Group($"user:{user.Id}").SendAsync(
            "notificationChanged", new { ideaId = idea.Id, type = "IdeaSubmitted" },
            HttpContext.RequestAborted);
        await hub.Clients.Group($"role:{RoleConstants.InnovationTeam}").SendAsync(
            "notificationChanged",
            new
            {
                ideaId = idea.Id,
                type = NotificationType.IdeaSubmitted.ToString(),
                subject = "New idea submitted"
            },
            HttpContext.RequestAborted);
        TempData["IdeaSubmissionSuccess"] =
            $"Idea {idea.ReferenceNumber} has been submitted to the Innovation Team for review.";
        return RedirectToAction(nameof(MyIdeas));
    }

    [Authorize(Roles = RoleConstants.Staff)]
    public async Task<IActionResult> MyIdeas(
        string? searchTerm,
        string? statusFilter,
        string? categoryFilter,
        int page = 1)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        const int pageSize = 10;
        var query = context.InnovationIdeas
            .AsNoTracking()
            .Where(idea => idea.SubmitterId == userId && !idea.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(idea =>
                idea.Title.Contains(searchTerm) ||
                idea.ReferenceNumber.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            if (Enum.TryParse<IdeaStatus>(statusFilter, true, out var parsedStatus))
                query = query.Where(idea => idea.CurrentStatus == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            query = query.Where(idea =>
                idea.Category != null && idea.Category.Name == categoryFilter);
        }

        var count = await query.CountAsync();
        var model = new MyIdeasModel
        {
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            CategoryFilter = categoryFilter,
            CurrentPage = Math.Max(page, 1),
            TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)),
            Ideas = await query
                .OrderByDescending(idea => idea.SubmissionDate)
                .Skip((Math.Max(page, 1) - 1) * pageSize)
                .Take(pageSize)
                .Select(idea => new IdeaListItemViewModel
                {
                    Id = idea.Id,
                    Title = idea.Title,
                    CategoryName = idea.Category != null ? idea.Category.Name : "Uncategorized",
                    Stage = idea.CurrentStage.ToString(),
                    Status = idea.IsRetracted ? "Retracted" : idea.CurrentStatus.ToString(),
                    SubmissionDate = idea.SubmissionDate,
                    IsRetracted = idea.IsRetracted
                })
                .ToListAsync()
        };

        return View(model);
    }

    [Authorize(Roles = RoleConstants.InnovationTeam)]
    public async Task<IActionResult> Pipeline(
        string? searchTerm,
        string? statusFilter,
        string? categoryFilter)
    {
        var query = context.InnovationIdeas
            .AsNoTracking()
            .Where(idea => !idea.IsDeleted && !idea.IsRetracted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim();
            query = query.Where(idea =>
                idea.Title.Contains(normalizedSearch) ||
                idea.ReferenceNumber.Contains(normalizedSearch) ||
                idea.Submitter.FullName.Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) &&
            Enum.TryParse<IdeaStatus>(statusFilter, true, out var parsedStatus))
        {
            query = query.Where(idea => idea.CurrentStatus == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            query = query.Where(idea =>
                idea.Category != null && idea.Category.Name == categoryFilter);
        }

        var ideas = await query
            .OrderBy(idea => idea.CurrentStage)
            .ThenBy(idea => idea.Timeline
                .Where(entry => entry.ActualCompletionDate == null)
                .Select(entry => (DateTime?)entry.DeadlineDate)
                .FirstOrDefault())
            .ThenByDescending(idea => idea.SubmissionDate)
            .Select(idea => new IdeaPipelineCardViewModel
            {
                Id = idea.Id,
                ReferenceNumber = idea.ReferenceNumber,
                Title = idea.Title,
                Submitter = idea.Submitter.FullName,
                Department = idea.Submitter.BusinessUnit,
                CategoryName = idea.Category != null ? idea.Category.Name : "Uncategorized",
                Stage = idea.CurrentStage,
                Status = idea.CurrentStatus,
                SubmissionDate = idea.SubmissionDate,
                StageStartDate = idea.Timeline
                    .Where(entry =>
                        entry.Stage == idea.CurrentStage &&
                        entry.ActualCompletionDate == null)
                    .OrderByDescending(entry => entry.StartDate)
                    .Select(entry => (DateTime?)entry.StartDate)
                    .FirstOrDefault(),
                DeadlineDate = idea.Timeline
                    .Where(entry =>
                        entry.Stage == idea.CurrentStage &&
                        entry.ActualCompletionDate == null)
                    .OrderBy(entry => entry.DeadlineDate)
                    .Select(entry => (DateTime?)entry.DeadlineDate)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var model = new IdeaPipelineModel
        {
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            CategoryFilter = categoryFilter,
            CategoryOptions = await context.Categories
                .AsNoTracking()
                .Where(category => category.IsActive)
                .OrderBy(category => category.Name)
                .Select(category => category.Name)
                .ToListAsync(),
            TotalIdeas = ideas.Count,
            PendingReviewIdeas = ideas.Count(idea => idea.Status == IdeaStatus.UnderReview),
            ApprovedIdeas = ideas.Count(idea => idea.Status == IdeaStatus.Approved),
            OverdueIdeas = ideas.Count(idea => idea.IsOverdue),
            Columns = Enum.GetValues<IdeaStage>()
                .Select(stage => new IdeaPipelineColumnViewModel
                {
                    Stage = stage,
                    Name = StageName(stage),
                    Description = StageDescription(stage),
                    Ideas = ideas.Where(idea => idea.Stage == stage).ToList()
                })
                .ToList()
        };

        return View(model);
    }

    [Authorize(Roles = RoleConstants.InnovationTeam)]
    public async Task<IActionResult> AllIdeas(
        string? searchTerm,
        string? statusFilter,
        string? categoryFilter,
        string? departmentFilter,
        DateTime? dateFrom,
        int page = 1)
    {
        const int pageSize = 10;
        var query = context.InnovationIdeas
            .AsNoTracking()
            .Where(idea => !idea.IsDeleted && !idea.IsRetracted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(idea =>
                idea.Title.Contains(searchTerm) ||
                idea.ReferenceNumber.Contains(searchTerm));
        }
        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            if (Enum.TryParse<IdeaStatus>(statusFilter, true, out var parsedStatus))
                query = query.Where(idea => idea.CurrentStatus == parsedStatus);
        }
        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            query = query.Where(idea =>
                idea.Category != null && idea.Category.Name == categoryFilter);
        }
        if (!string.IsNullOrWhiteSpace(departmentFilter))
        {
            query = query.Where(idea => idea.Submitter.BusinessUnit == departmentFilter);
        }
        if (dateFrom.HasValue)
        {
            query = query.Where(idea => idea.SubmissionDate >= dateFrom.Value);
        }

        var count = await query.CountAsync();
        var model = new SubmittedIdeasModel
        {
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            CategoryFilter = categoryFilter,
            DepartmentFilter = departmentFilter,
            DateFrom = dateFrom,
            CurrentPage = Math.Max(page, 1),
            TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)),
            Ideas = await query
                .OrderBy(idea => idea.SubmissionDate)
                .Skip((Math.Max(page, 1) - 1) * pageSize)
                .Take(pageSize)
                .Select(idea => new SubmittedIdeaItemViewModel
                {
                    Id = idea.Id,
                    Title = idea.Title,
                    Submitter = idea.Submitter.FullName,
                    Department = idea.Submitter.BusinessUnit,
                    CategoryName = idea.Category != null ? idea.Category.Name : "Uncategorized",
                    Stage = idea.CurrentStage.ToString(),
                    Status = idea.CurrentStatus.ToString(),
                    SubmissionDate = idea.SubmissionDate,
                    NeedsReview = idea.CurrentStatus == IdeaStatus.UnderReview
                })
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var isInnovationTeam = User.IsInRole(RoleConstants.InnovationTeam);
        var idea = await context.InnovationIdeas
            .AsNoTracking()
            .Where(item =>
                item.Id == id &&
                !item.IsDeleted &&
                (isInnovationTeam || item.SubmitterId == userId))
            .Select(item => new IdeaDetailsModel
            {
                Idea = new IdeaDetailViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    Submitter = item.Submitter.FullName,
                    SubmissionDate = item.SubmissionDate,
                    SummaryDescription = item.SummaryDescription,
                    ProblemStatement = item.ProblemStatement,
                    ProposedSolution = item.ProposedSolution,
                    Stage = item.CurrentStage.ToString(),
                    Status = item.IsRetracted ? "Retracted" : item.CurrentStatus.ToString(),
                    Attachments = item.Attachments.Select(file => new AttachmentViewModel
                    {
                        Id = file.Id,
                        FileName = file.FileName,
                        FilePath = file.StorageName,
                        Icon = "paperclip"
                    }).ToList()
                },
                TimelineEntries = item.Timeline
                    .OrderBy(entry => entry.StartDate)
                    .Select(entry => new TimelineEntryViewModel
                    {
                        StageName = entry.Stage.ToString(),
                        Date = entry.StartDate
                    }).ToList(),
                Comments = item.Comments
                    .Where(comment => !comment.IsDeleted && !comment.IsInternal)
                    .OrderBy(comment => comment.CreatedDate)
                    .Select(comment => new CommentDetailViewModel
                    {
                        Avatar = comment.User.FullName.Substring(0, 1),
                        Author = comment.User.FullName,
                        Role = "Participant",
                        TimeAgo = comment.CreatedDate.ToString("dd MMM yyyy HH:mm"),
                        Text = comment.CommentText
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        return idea == null ? NotFound() : View(idea);
    }

    private bool TryGetCurrentUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return null;
        }

        return await context.Users.FirstOrDefaultAsync(user => user.Id == userId);
    }

    private static string StageName(IdeaStage stage) => stage switch
    {
        IdeaStage.ConceptDevelopment => "Concept Development",
        _ => stage.ToString()
    };

    private static string StageDescription(IdeaStage stage) => stage switch
    {
        IdeaStage.Submitted => "New ideas awaiting initial review",
        IdeaStage.ConceptDevelopment => "Ideas being shaped and assessed",
        IdeaStage.Experimentation => "Concepts being tested and validated",
        IdeaStage.Deployment => "Approved ideas moving into operation",
        IdeaStage.Closed => "Ideas with a completed workflow",
        _ => string.Empty
    };
}

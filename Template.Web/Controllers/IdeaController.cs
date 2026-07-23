using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Models.Idea;

namespace Template.Web.Controllers;

[Authorize]
public class IdeaController(ApplicationDbContext context) : Controller
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
            CurrentStage = nameof(IdeaStage.Submitted),
            CurrentStatus = nameof(IdeaStatus.UnderReview),
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

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Idea {idea.ReferenceNumber} submitted successfully.";
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
            query = query.Where(idea => idea.CurrentStatus == statusFilter);
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
                    Stage = idea.CurrentStage,
                    Status = idea.IsRetracted ? "Retracted" : idea.CurrentStatus,
                    SubmissionDate = idea.SubmissionDate
                })
                .ToListAsync()
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
            query = query.Where(idea => idea.CurrentStatus == statusFilter);
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
                    Stage = idea.CurrentStage,
                    Status = idea.CurrentStatus,
                    SubmissionDate = idea.SubmissionDate,
                    NeedsReview = idea.CurrentStatus == nameof(IdeaStatus.UnderReview)
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
                    Stage = item.CurrentStage,
                    Status = item.IsRetracted ? "Retracted" : item.CurrentStatus,
                    Attachments = item.Attachments.Select(file => new AttachmentViewModel
                    {
                        FileName = file.FileName,
                        FilePath = file.FilePath,
                        Icon = "paperclip"
                    }).ToList()
                },
                TimelineEntries = item.Timeline
                    .OrderBy(entry => entry.StartDate)
                    .Select(entry => new TimelineEntryViewModel
                    {
                        StageName = entry.Stage.ToString(),
                        Date = entry.StartDate
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
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Models.Idea;
using Microsoft.AspNetCore.SignalR;
using Template.Web.Hubs;
using Template.Core.Services.Workflow;

namespace Template.Web.Controllers;

[Authorize(Roles = RoleConstants.InnovationTeam)]
[Route("Idea")]
public class IdeaWorkflowController(ApplicationDbContext context, IHubContext<ImtsHub> hub,
    IIdeaWorkflowService workflow) : Controller
{
    [HttpGet("Review/{id:guid}")]
    public async Task<IActionResult> Review(Guid id)
    {
        var model = await context.InnovationIdeas
            .AsNoTracking()
            .Where(idea => idea.Id == id && !idea.IsDeleted && !idea.IsRetracted)
            .Select(idea => new ReviewIdeaModel
            {
                Idea = new IdeaReviewViewModel
                {
                    Id = idea.Id,
                    Title = idea.Title,
                    Submitter = idea.Submitter.FullName,
                    SubmitterBusinessUnit = idea.Submitter.BusinessUnit,
                    SubmissionDate = idea.SubmissionDate,
                    SummaryDescription = idea.SummaryDescription,
                    ProblemStatement = idea.ProblemStatement,
                    ProposedSolution = idea.ProposedSolution,
                    Attachments = idea.Attachments
                        .Where(file => !file.IsDeleted)
                        .Select(file => new AttachmentReviewViewModel
                        {
                            FileName = file.FileName,
                            Icon = "paperclip"
                        }).ToList()
                },
                Review = new ReviewFormViewModel
                {
                    RowVersion = Convert.ToBase64String(idea.RowVersion),
                    Status = idea.CurrentStatus.ToString(),
                    Stage = idea.CurrentStage.ToString(),
                    AssignedReviewerId = idea.AssignedReviewerId.HasValue
                        ? idea.AssignedReviewerId.ToString()
                        : null,
                    TimelineDate = idea.Timeline
                        .Where(entry => entry.ActualCompletionDate == null)
                        .OrderBy(entry => entry.DeadlineDate)
                        .Select(entry => (DateTime?)entry.DeadlineDate)
                        .FirstOrDefault()
                },
                Comments = idea.Comments
                    .Where(comment => !comment.IsDeleted)
                    .OrderBy(comment => comment.CreatedDate)
                    .Select(comment => new CommentReviewViewModel
                    {
                        Avatar = comment.User.FullName.Substring(0, 1),
                        Author = comment.User.FullName,
                        Role = comment.IsInternal ? "Innovation Team" : "Staff",
                        TimeAgo = comment.CreatedDate.ToString("dd MMM yyyy HH:mm"),
                        Text = comment.CommentText
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (model == null)
        {
            return NotFound();
        }

        model.Reviewers = await context.Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .OrderBy(user => user.FullName)
            .Select(user => new ReviewerOptionViewModel
            {
                Id = user.Id.ToString(),
                FullName = user.FullName
            }).ToListAsync();

        model.WorkflowSteps = Enum.GetNames<IdeaStage>()
            .Select(stage => new WorkflowStepViewModel
            {
                Name = stage,
                IsComplete = StageOrder(stage) < StageOrder(model.Review.Stage),
                IsPending = stage == model.Review.Stage,
                StatusClass = stage == model.Review.Stage ? "active" : string.Empty
            }).ToList();

        return View("~/Views/Idea/Review.cshtml", model);
    }

    [HttpPost("Review/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReview(Guid id, ReviewFormViewModel review)
    {
        if (!Enum.TryParse<IdeaStage>(review.Stage, out var stage) ||
            !Enum.TryParse<IdeaStatus>(review.Status, out var status))
        {
            TempData["ErrorMessage"] = "Select a valid stage and status.";
            return RedirectToAction(nameof(Review), new { id });
        }

        var reviewerId = GetCurrentUserId();
        var idea = await context.InnovationIdeas
            .Include(item => item.Timeline)
            .Include(item => item.Submitter)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted && !item.IsRetracted);
        if (idea == null)
        {
            return NotFound();
        }

        var previousStage = idea.CurrentStage;
        var previousStatus = idea.CurrentStatus;
        if (!workflow.CanTransition(previousStage, previousStatus, stage, status))
        {
            TempData["ErrorMessage"] = "That workflow transition is not permitted.";
            return RedirectToAction(nameof(Review), new { id });
        }
        if (string.IsNullOrWhiteSpace(review.RowVersion))
        {
            TempData["ErrorMessage"] = "The review version is missing. Reload and try again.";
            return RedirectToAction(nameof(Review), new { id });
        }
        context.Entry(idea).Property(item => item.RowVersion).OriginalValue =
            Convert.FromBase64String(review.RowVersion);
        idea.CurrentStage = stage;
        idea.CurrentStatus = status;
        idea.AssignedReviewerId = Guid.TryParse(review.AssignedReviewerId, out var assigned)
            ? assigned
            : null;
        idea.ModifiedDate = DateTime.UtcNow;
        idea.ModifiedBy = reviewerId.ToString();

        context.StageHistories.Add(new StageHistory
        {
            IdeaId = idea.Id,
            Idea = idea,
            PreviousStage = previousStage,
            NewStage = idea.CurrentStage,
            PreviousStatus = previousStatus,
            NewStatus = idea.CurrentStatus,
            ChangedById = reviewerId,
            ChangedBy = await context.Users.FindAsync(reviewerId)
                ?? throw new InvalidOperationException("Reviewer account not found."),
            ChangeReason = review.Notes,
            ChangedAt = DateTime.UtcNow
        });

        var openTimeline = idea.Timeline.FirstOrDefault(item =>
            item.ActualCompletionDate == null);
        if (openTimeline != null && openTimeline.Stage != stage)
        {
            openTimeline.ActualCompletionDate = DateTime.UtcNow;
        }

        if (openTimeline == null || openTimeline.Stage != stage)
        {
            context.IdeaTimelines.Add(new IdeaTimeline
            {
                Id = Guid.NewGuid(),
                IdeaId = idea.Id,
                Idea = idea,
                StageId = (int)stage,
                Stage = stage,
                StartDate = DateTime.UtcNow,
                DeadlineDate = review.TimelineDate ?? DefaultDeadline(stage),
                ApprovedById = reviewerId,
                ApprovedAt = DateTime.UtcNow
            });
        }
        else if (review.TimelineDate.HasValue)
        {
            openTimeline.DeadlineDate = review.TimelineDate.Value;
        }

        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = idea.SubmitterId,
            IdeaId = idea.Id,
            Idea = idea,
            Type = previousStage != idea.CurrentStage
                ? NotificationType.StageChanged
                : NotificationType.StatusChanged,
            Subject = "Idea review updated",
            Message = $"{idea.ReferenceNumber} is now {idea.CurrentStage} / {idea.CurrentStatus}.",
            CreatedDate = DateTime.UtcNow
        });
        if (!string.IsNullOrWhiteSpace(idea.Submitter.Email))
            context.EmailOutbox.Add(new EmailOutbox
            {
                Id = Guid.NewGuid(), IdempotencyKey = $"review:{idea.Id}:{DateTime.UtcNow.Ticks}",
                Recipient = idea.Submitter.Email, Subject = "Innovation idea updated",
                Body = $"{idea.ReferenceNumber} is now {idea.CurrentStage} / {idea.CurrentStatus}.",
                CreatedAtUtc = DateTime.UtcNow
            });

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["ErrorMessage"] = "Another reviewer changed this idea. Reload and review the latest values.";
            return RedirectToAction(nameof(Review), new { id });
        }
        await hub.Clients.Group($"user:{idea.SubmitterId}").SendAsync(
            "ideaChanged", new { ideaId = idea.Id, stage = idea.CurrentStage.ToString(), status = idea.CurrentStatus.ToString() });
        TempData["SuccessMessage"] = "Review saved and the submitter was notified.";
        return RedirectToAction(nameof(Review), new { id });
    }

    [HttpPost("Review/{id:guid}/Comment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Guid id, string newComment)
    {
        if (string.IsNullOrWhiteSpace(newComment))
        {
            return RedirectToAction(nameof(Review), new { id });
        }

        var reviewerId = GetCurrentUserId();
        var idea = await context.InnovationIdeas
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted && !item.IsRetracted);
        var reviewer = await context.Users.FindAsync(reviewerId);
        if (idea == null || reviewer == null)
        {
            return NotFound();
        }

        context.Comments.Add(new Comment
        {
            Id = Guid.NewGuid(),
            IdeaId = idea.Id,
            Idea = idea,
            UserId = reviewer.Id,
            User = reviewer,
            CommentText = newComment.Trim(),
            IsInternal = false
        });
        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = idea.SubmitterId,
            IdeaId = idea.Id,
            Idea = idea,
            Type = NotificationType.CommentAdded,
            Subject = "New review comment",
            Message = $"A reviewer commented on {idea.ReferenceNumber}.",
            CreatedDate = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Review), new { id });
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static int StageOrder(string? stage) =>
        Enum.TryParse<IdeaStage>(stage, out var value) ? (int)value : -1;

    private static DateTime DefaultDeadline(IdeaStage stage) =>
        DateTime.UtcNow.AddDays(stage switch
        {
            IdeaStage.Submitted => 30,
            IdeaStage.ConceptDevelopment => 60,
            IdeaStage.Experimentation => 90,
            IdeaStage.Deployment => 30,
            _ => 0
        });
}

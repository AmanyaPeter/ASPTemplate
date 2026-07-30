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
                    ExpectedBenefits = idea.ExpectedBenefits,
                    KeyEnablers = idea.KeyEnablers,
                    ImplementationApproach = idea.ImplementationApproach,
                    ImpactIndicators = idea.ImpactIndicators,
                    StrategicObjective = idea.StrategicObjective,
                    Category = idea.Category != null ? idea.Category.Name : "Uncategorised",
                    Attachments = idea.Attachments
                        .Where(file => !file.IsDeleted)
                        .Select(file => new AttachmentReviewViewModel
                        {
                            Id = file.Id,
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
                        Role = comment.UserId == idea.SubmitterId ? "Staff" : "Innovation Team",
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
            LinkUrl = $"/Idea/Details/{idea.Id}",
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
        await hub.Clients.Group($"user:{idea.SubmitterId}").SendAsync(
            "notificationChanged",
            new
            {
                ideaId = idea.Id,
                type = previousStage != idea.CurrentStage
                    ? NotificationType.StageChanged.ToString()
                    : NotificationType.StatusChanged.ToString(),
                subject = "Idea review updated"
            });
        TempData["SuccessMessage"] = "Review saved and the submitter was notified.";
        return RedirectToAction(nameof(Review), new { id });
    }

    [HttpPost("Pipeline/Advance")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdvanceFromPipeline(
        Guid ideaId,
        IdeaStage targetStage,
        string? reason,
        CancellationToken cancellationToken)
    {
        if (targetStage == IdeaStage.Submitted)
        {
            TempData["ErrorMessage"] = "New submissions enter the pipeline from the staff submission form.";
            return RedirectToAction("Pipeline", "Idea");
        }

        var snapshot = await context.InnovationIdeas
            .AsNoTracking()
            .Where(idea => idea.Id == ideaId && !idea.IsDeleted && !idea.IsRetracted)
            .Select(idea => new
            {
                idea.Id,
                idea.ReferenceNumber,
                idea.SubmitterId,
                SubmitterEmail = idea.Submitter.Email,
                idea.CurrentStage,
                idea.CurrentStatus,
                idea.RowVersion
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (snapshot == null)
        {
            TempData["ErrorMessage"] = "The selected idea is no longer available.";
            return RedirectToAction("Pipeline", "Idea");
        }

        if (!workflow.CanTransition(
                snapshot.CurrentStage,
                snapshot.CurrentStatus,
                targetStage,
                IdeaStatus.Approved))
        {
            TempData["ErrorMessage"] =
                $"Move the idea through the next pipeline stage first. It is currently in {snapshot.CurrentStage}.";
            return RedirectToAction("Pipeline", "Idea");
        }

        try
        {
            await workflow.TransitionAsync(
                snapshot.Id,
                targetStage,
                IdeaStatus.Approved,
                GetCurrentUserId(),
                string.IsNullOrWhiteSpace(reason) ? $"Moved to {targetStage} from the idea pipeline." : reason.Trim(),
                null,
                snapshot.RowVersion,
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["ErrorMessage"] = "Another reviewer changed this idea. Reload the pipeline and try again.";
            return RedirectToAction("Pipeline", "Idea");
        }
        catch (InvalidOperationException)
        {
            TempData["ErrorMessage"] = "The idea can no longer be moved to that stage. Reload the pipeline and review its latest status.";
            return RedirectToAction("Pipeline", "Idea");
        }

        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = snapshot.SubmitterId,
            IdeaId = snapshot.Id,
            Type = NotificationType.StageChanged,
            Subject = "Idea moved to a new stage",
            Message = $"{snapshot.ReferenceNumber} moved to {targetStage}.",
            LinkUrl = $"/Idea/Details/{snapshot.Id}",
            CreatedDate = DateTime.UtcNow
        });
        if (!string.IsNullOrWhiteSpace(snapshot.SubmitterEmail))
        {
            context.EmailOutbox.Add(new EmailOutbox
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = $"pipeline:{snapshot.Id}:{targetStage}:{DateTime.UtcNow.Ticks}",
                Recipient = snapshot.SubmitterEmail,
                Subject = $"Your idea moved to {targetStage}",
                Body = $"{snapshot.ReferenceNumber} moved to the {targetStage} stage.",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        await hub.Clients.Group($"user:{snapshot.SubmitterId}").SendAsync(
            "notificationChanged",
            new
            {
                ideaId = snapshot.Id,
                type = NotificationType.StageChanged.ToString(),
                subject = "Idea moved to a new stage"
            },
            cancellationToken);

        TempData["SuccessMessage"] = $"{snapshot.ReferenceNumber} was moved to {targetStage}.";
        return RedirectToAction("Pipeline", "Idea");
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
            .Include(item => item.Submitter)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted && !item.IsRetracted);
        var reviewer = await context.Users.FindAsync(reviewerId);
        if (idea == null || reviewer == null)
        {
            return NotFound();
        }

        var commentId = Guid.NewGuid();
        context.Comments.Add(new Comment
        {
            Id = commentId,
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
            LinkUrl = $"/Idea/Details/{idea.Id}",
            CreatedDate = DateTime.UtcNow
        });
        if (!string.IsNullOrWhiteSpace(idea.Submitter.Email))
        {
            context.EmailOutbox.Add(new EmailOutbox
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = $"review-comment:{commentId}",
                Recipient = idea.Submitter.Email,
                Subject = $"New comment on {idea.ReferenceNumber}",
                Body = $"The Innovation Team commented on your idea \"{idea.Title}\":\n\n{newComment.Trim()}\n\nSign in to the Innovation Management System to respond.",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        await hub.Clients.Group($"user:{idea.SubmitterId}").SendAsync(
            "notificationChanged",
            new
            {
                ideaId = idea.Id,
                type = NotificationType.CommentAdded.ToString(),
                subject = "New review comment"
            },
            HttpContext.RequestAborted);
        TempData["CommentSentSuccess"] =
            $"Your comment was sent to {idea.Submitter.FullName}. The submitter has been notified.";
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

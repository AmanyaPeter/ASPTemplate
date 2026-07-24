#nullable enable
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Core.Services.Workflow;

public sealed class IdeaWorkflowService(ApplicationDbContext context) : IIdeaWorkflowService
{
    public bool CanTransition(IdeaStage currentStage, IdeaStatus currentStatus, IdeaStage nextStage, IdeaStatus nextStatus)
        => WorkflowRules.CanTransition(currentStage, currentStatus, nextStage, nextStatus);

    public int DefaultDurationDays(IdeaStage stage) => WorkflowRules.DefaultDurationDays(stage);

    public async Task<InnovationIdea> TransitionAsync(Guid ideaId, IdeaStage stage, IdeaStatus status,
        Guid actorId, string? reason, DateTime? dueAtUtc, byte[] rowVersion, CancellationToken cancellationToken)
    {
        var idea = await context.InnovationIdeas.Include(x => x.Timeline)
            .SingleOrDefaultAsync(x => x.Id == ideaId && !x.IsDeleted && !x.IsRetracted, cancellationToken)
            ?? throw new KeyNotFoundException("Idea was not found.");
        context.Entry(idea).Property(x => x.RowVersion).OriginalValue = rowVersion;
        if (!CanTransition(idea.CurrentStage, idea.CurrentStatus, stage, status))
            throw new InvalidOperationException("The requested workflow transition is not permitted.");

        var actor = await context.Users.SingleAsync(x => x.Id == actorId, cancellationToken);
        var previousStage = idea.CurrentStage;
        var previousStatus = idea.CurrentStatus;
        idea.CurrentStage = stage;
        idea.CurrentStatus = status;
        idea.DecisionDate = status is IdeaStatus.Approved or IdeaStatus.Declined ? DateTime.UtcNow : null;
        idea.DecisionReason = reason;

        context.StageHistories.Add(new StageHistory
        {
            IdeaId = idea.Id, Idea = idea, PreviousStage = previousStage, NewStage = stage,
            PreviousStatus = previousStatus, NewStatus = status, ChangedById = actorId,
            ChangedBy = actor, ChangeReason = reason, ChangedAt = DateTime.UtcNow
        });

        if (stage != previousStage)
        {
            foreach (var open in idea.Timeline.Where(x => x.ActualCompletionDate == null))
                open.ActualCompletionDate = DateTime.UtcNow;
            context.IdeaTimelines.Add(new IdeaTimeline
            {
                Id = Guid.NewGuid(), IdeaId = idea.Id, Idea = idea, Stage = stage, StageId = (int)stage,
                StartDate = DateTime.UtcNow,
                DeadlineDate = dueAtUtc ?? DateTime.UtcNow.AddDays(DefaultDurationDays(stage)),
                ApprovedById = actorId, ApprovedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return idea;
    }
}

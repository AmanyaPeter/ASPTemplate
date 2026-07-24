#nullable enable
using Template.Common.Enums;
using Template.Data.Entities;

namespace Template.Core.Services.Workflow;

public interface IIdeaWorkflowService
{
    bool CanTransition(IdeaStage currentStage, IdeaStatus currentStatus, IdeaStage nextStage, IdeaStatus nextStatus);
    int DefaultDurationDays(IdeaStage stage);
    Task<InnovationIdea> TransitionAsync(Guid ideaId, IdeaStage stage, IdeaStatus status,
        Guid actorId, string? reason, DateTime? dueAtUtc, byte[] rowVersion, CancellationToken cancellationToken);
}

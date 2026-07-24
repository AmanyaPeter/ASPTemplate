using Template.Common.Enums;

namespace Template.Core.Services.Workflow;

public static class WorkflowRules
{
    public static bool CanTransition(IdeaStage currentStage, IdeaStatus currentStatus,
        IdeaStage nextStage, IdeaStatus nextStatus)
    {
        if (currentStage == IdeaStage.Closed) return false;
        if (nextStatus == IdeaStatus.Declined) return true;
        if (nextStage == currentStage) return true;
        return (int)nextStage == (int)currentStage + 1 && nextStatus == IdeaStatus.Approved;
    }

    public static int DefaultDurationDays(IdeaStage stage) => stage switch
    {
        IdeaStage.Submitted => 30,
        IdeaStage.ConceptDevelopment => 60,
        IdeaStage.Experimentation => 90,
        IdeaStage.Deployment => 30,
        _ => 0
    };
}

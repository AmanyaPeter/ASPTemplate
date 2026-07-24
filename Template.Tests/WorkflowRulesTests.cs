using Template.Common.Enums;
using Template.Core.Services.Workflow;
using Xunit;

namespace Template.Tests;

public sealed class WorkflowRulesTests
{
    [Theory]
    [InlineData(IdeaStage.Submitted, 30)]
    [InlineData(IdeaStage.ConceptDevelopment, 60)]
    [InlineData(IdeaStage.Experimentation, 90)]
    [InlineData(IdeaStage.Deployment, 30)]
    public void Default_duration_matches_srs(IdeaStage stage, int days) =>
        Assert.Equal(days, WorkflowRules.DefaultDurationDays(stage));

    [Fact]
    public void Cannot_skip_a_stage() =>
        Assert.False(WorkflowRules.CanTransition(IdeaStage.Submitted, IdeaStatus.UnderReview,
            IdeaStage.Deployment, IdeaStatus.Approved));

    [Fact]
    public void Closed_idea_is_terminal() =>
        Assert.False(WorkflowRules.CanTransition(IdeaStage.Closed, IdeaStatus.Approved,
            IdeaStage.Closed, IdeaStatus.Declined));
}

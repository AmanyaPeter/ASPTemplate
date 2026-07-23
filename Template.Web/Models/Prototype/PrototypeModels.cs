namespace Template.Web.Models.Prototype;

public sealed class PrototypePageViewModel
{
    public string ActiveStage { get; set; } = "Review";
    public int CompletionPercent { get; set; } = 45;
    public IReadOnlyList<PrototypeStageViewModel> Stages { get; init; } =
    [
        new("Submission", "Completed", true),
        new("Review", "In progress", true),
        new("Concept", "Not started", false),
        new("Experiment", "Not started", false),
        new("Deployment", "Not started", false)
    ];
}

public sealed record PrototypeStageViewModel(string Name, string State, bool IsReached);

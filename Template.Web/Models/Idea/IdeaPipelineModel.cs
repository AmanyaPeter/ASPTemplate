using Template.Common.Enums;

namespace Template.Web.Models.Idea;

public sealed class IdeaPipelineModel
{
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
    public string? CategoryFilter { get; set; }
    public List<string> CategoryOptions { get; set; } = [];
    public List<IdeaPipelineColumnViewModel> Columns { get; set; } = [];
    public int TotalIdeas { get; set; }
    public int PendingReviewIdeas { get; set; }
    public int ApprovedIdeas { get; set; }
    public int OverdueIdeas { get; set; }
}

public sealed class IdeaPipelineColumnViewModel
{
    public IdeaStage Stage { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<IdeaPipelineCardViewModel> Ideas { get; set; } = [];
    public List<PipelineMoveCandidateViewModel> MoveCandidates { get; set; } = [];
}

public sealed class PipelineMoveCandidateViewModel
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public sealed class IdeaPipelineCardViewModel
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Submitter { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string CategoryName { get; set; } = "Uncategorized";
    public IdeaStage Stage { get; set; }
    public IdeaStatus Status { get; set; }
    public DateTime SubmissionDate { get; set; }
    public DateTime? StageStartDate { get; set; }
    public DateTime? DeadlineDate { get; set; }

    public bool IsOverdue =>
        Stage != IdeaStage.Closed &&
        DeadlineDate.HasValue &&
        DeadlineDate.Value.Date < DateTime.UtcNow.Date;

    public string DeadlineLabel
    {
        get
        {
            if (Stage == IdeaStage.Closed)
            {
                return "Workflow complete";
            }

            if (!DeadlineDate.HasValue)
            {
                return "No deadline set";
            }

            var days = (DeadlineDate.Value.Date - DateTime.UtcNow.Date).Days;
            return days switch
            {
                < 0 => $"{Math.Abs(days)} day{(Math.Abs(days) == 1 ? string.Empty : "s")} overdue",
                0 => "Due today",
                _ => $"{days} day{(days == 1 ? string.Empty : "s")} left"
            };
        }
    }

    public int DaysInStage =>
        Math.Max(0, ((Stage == IdeaStage.Closed ? DeadlineDate : DateTime.UtcNow) -
            (StageStartDate ?? SubmissionDate)).GetValueOrDefault().Days);
}

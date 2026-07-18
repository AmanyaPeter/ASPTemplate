namespace Template.Web.Models.Idea
{
    public class ReviewIdeaModel
    {
        public IdeaReviewViewModel Idea { get; set; } = new();
        public List<CommentReviewViewModel> Comments { get; set; } = new();
        public string? NewComment { get; set; }
        public ReviewFormViewModel Review { get; set; } = new();
        public List<ReviewerOptionViewModel> Reviewers { get; set; } = new();
        public List<WorkflowStepViewModel> WorkflowSteps { get; set; } = new();
    }

    public class IdeaReviewViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Submitter { get; set; }
        public string? SubmitterBusinessUnit { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string? SummaryDescription { get; set; }
        public string? ProblemStatement { get; set; }
        public string? ProposedSolution { get; set; }
        public List<AttachmentReviewViewModel> Attachments { get; set; } = new();
        public string? StrategicObjective { get; set; }
        public string? SDGContribution { get; set; }
    }

    public class AttachmentReviewViewModel
    {
        public string? FileName { get; set; }
        public string? Icon { get; set; }
    }

    public class CommentReviewViewModel
    {
        public string? Avatar { get; set; }
        public string? Author { get; set; }
        public string? Role { get; set; }
        public string? TimeAgo { get; set; }
        public string? Text { get; set; }
    }

    public class ReviewFormViewModel
    {
        public string? Status { get; set; }
        public string? Stage { get; set; }
        public DateTime? TimelineDate { get; set; }
        public string? AssignedReviewerId { get; set; }
        public string? Notes { get; set; }
    }

    public class ReviewerOptionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }

    public class WorkflowStepViewModel
    {
        public string? Name { get; set; }
        public string? StatusClass { get; set; }
        public bool IsComplete { get; set; }
        public bool IsPending { get; set; }
    }
}

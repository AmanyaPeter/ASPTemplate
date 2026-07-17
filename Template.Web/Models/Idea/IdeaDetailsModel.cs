namespace Template.Web.Models.Idea
{
    public class IdeaDetailsModel
    {
        public IdeaDetailViewModel Idea { get; set; } = new();
        public List<CommentDetailViewModel> Comments { get; set; } = new();
        public string? NewComment { get; set; }
        public List<TimelineEntryViewModel> TimelineEntries { get; set; } = new();
    }

    public class IdeaDetailViewModel
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Submitter { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string? SummaryDescription { get; set; }
        public string? ProblemStatement { get; set; }
        public string? ProposedSolution { get; set; }
        public string? Stage { get; set; }
        public string? Status { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; } = new();
    }

    public class AttachmentViewModel
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? Icon { get; set; }
    }

    public class CommentDetailViewModel
    {
        public string? Avatar { get; set; }
        public string? Author { get; set; }
        public string? Role { get; set; }
        public string? TimeAgo { get; set; }
        public string? Text { get; set; }
    }

    public class TimelineEntryViewModel
    {
        public string? StageName { get; set; }
        public DateTime Date { get; set; }
    }
}

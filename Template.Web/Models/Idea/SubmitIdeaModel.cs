namespace Template.Web.Models.Idea
{
    public class SubmitIdeaModel
    {
        public InnovatorViewModel Innovator { get; set; } = new();
        public string SubmissionType { get; set; } = "individual";
        public IdeaFormViewModel Idea { get; set; } = new();
        public List<IFormFile>? Attachments { get; set; }
    }

    public class InnovatorViewModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? BusinessUnit { get; set; }
        public string? DutyStation { get; set; }
        public string? Age { get; set; }
        public string? Gender { get; set; }
        public string? Rank { get; set; }
    }

    public class IdeaFormViewModel
    {
        public string? Title { get; set; }
        public string? SummaryDescription { get; set; }
        public string? ProblemStatement { get; set; }
        public string? ProposedSolution { get; set; }
        public string? ExpectedBenefits { get; set; }
        public string? KeyEnablers { get; set; }
        public string? ImplementationApproach { get; set; }
        public string? ImpactIndicators { get; set; }
        public string? StrategicObjective { get; set; }
        public string? Category { get; set; }
    }
}

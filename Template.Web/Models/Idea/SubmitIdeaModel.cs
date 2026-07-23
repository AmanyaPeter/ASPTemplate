using System.ComponentModel.DataAnnotations;

namespace Template.Web.Models.Idea
{
    public class SubmitIdeaModel
    {
        public Guid? Id { get; set; }
        public InnovatorViewModel Innovator { get; set; } = new();
        [Required]
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
        [StringLength(2000)]
        public string? TeamMemberNames { get; set; }
    }

    public class IdeaFormViewModel
    {
        [Required, StringLength(200)]
        public string? Title { get; set; }
        [Required, StringLength(2000)]
        public string? SummaryDescription { get; set; }
        [Required, StringLength(4000)]
        public string? ProblemStatement { get; set; }
        [Required, StringLength(4000)]
        public string? ProposedSolution { get; set; }
        [Required, StringLength(4000)]
        public string? ExpectedBenefits { get; set; }
        [Required, StringLength(4000)]
        public string? KeyEnablers { get; set; }
        [Required, StringLength(4000)]
        public string? ImplementationApproach { get; set; }
        [Required, StringLength(4000)]
        public string? ImpactIndicators { get; set; }
        [Required, StringLength(1000)]
        public string? StrategicObjective { get; set; }
        [Required]
        public string? Category { get; set; }
    }
}

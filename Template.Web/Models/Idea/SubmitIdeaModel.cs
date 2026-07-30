using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public List<SelectListItem> CategoryOptions { get; set; } = new();
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
        [Phone, StringLength(50)]
        public string? PhoneNumber { get; set; }

        public List<SelectListItem> BusinessUnitOptions { get; set; } = new();
        public List<SelectListItem> DutyStationOptions { get; set; } = new();
        public List<SelectListItem> AgeOptions { get; set; } = new();
        public List<SelectListItem> GenderOptions { get; set; } = new();
        public List<SelectListItem> RankOptions { get; set; } = new();
    }

    public class IdeaFormViewModel
    {
        [Required, StringLength(100)]
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
        [StringLength(1000)]
        public string? StrategicObjective { get; set; }
        public string? Category { get; set; }

        // New form fields
        public List<string>? TypeOfInnovation { get; set; }
        public List<string>? StrategicAlignment { get; set; }
        public string? OtherStrategicAlignment { get; set; }
        public string? InnovationPriorityArea { get; set; }
        public string? ExpectedTimeline { get; set; }
        public string? EstimatedBudgetRange { get; set; }
        [StringLength(4000)]
        public string? AdditionalComments { get; set; }
        public bool DeclarationAccurate { get; set; }
        public bool DeclarationReview { get; set; }
        public bool DeclarationParticipate { get; set; }

        public List<SelectListItem> TypeOfInnovationOptions { get; set; } = new();
        public List<Template.Data.Entities.TypeOfInnovation> InnovationTypeDetails { get; set; } = new();
        public List<SelectListItem> StrategicAlignmentOptions { get; set; } = new();
        public List<SelectListItem> InnovationPriorityAreaOptions { get; set; } = new();
        public List<SelectListItem> ExpectedTimelineOptions { get; set; } = new();
        public List<SelectListItem> EstimatedBudgetRangeOptions { get; set; } = new();
    }

    public class IdeaSuccessViewModel
    {
        public string ReferenceNumber { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime SubmissionDate { get; set; }
        public string Category { get; set; } = "";
        public string SummaryDescription { get; set; } = "";
    }
}

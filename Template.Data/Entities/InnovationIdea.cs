using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
public class InnovationIdea : AuditableEntity
{
    public Guid Id { get; set; }

    public string ReferenceNumber { get; set; }

    // Submission information
    public string SubmissionType { get; set; }
    public DateTime SubmissionDate { get; set; }

    public Guid SubmitterId { get; set; }
    public ApplicationUser Submitter { get; set; }

    // Idea details
    public string Title { get; set; }
    public string SummaryDescription { get; set; }
    public string ProblemStatement { get; set; }
    public string ProposedSolution { get; set; }
    public string? ExpectedBenefits { get; set; }
    public string? KeyEnablers { get; set; }
    public string? ImplementationApproach { get; set; }
    public string? ImpactIndicators { get; set; }
    public string? StrategicObjective { get; set; }
    public string? TeamMemberNames { get; set; }
    public string? TeamCompositionJson { get; set; }

    // Classification
    public int? CategoryId { get; set; }
    public Category Category { get; set; }

    // Snapshot information
    public int? SubmitterBusinessUnitId { get; set; }
    public int? SubmitterStationId { get; set; }
    public string SubmitterAgeBracket { get; set; }

    // Workflow
    public string CurrentStage { get; set; }
    public string CurrentStatus { get; set; }

    // Review
    public Guid? AssignedReviewerId { get; set; }

    // Decision
    public DateTime? DecisionDate { get; set; }
    public string? DecisionReason { get; set; }

    // Controls
    public bool IsLocked { get; set; }
    public bool IsRetracted { get; set; }
    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; }

    // Collections
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<IdeaAttachment> Attachments { get; set; } = new List<IdeaAttachment>();
    public ICollection<IdeaTimeline> Timeline { get; set; } = new List<IdeaTimeline>();
}
}

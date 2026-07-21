using Template.Common.AuditColumn;
using Template.Common.Enums;

namespace Template.Data.Entities
{
public class InnovationIdea : AuditableEntity
{
    public Guid Id { get; set; }

    public required string ReferenceNumber { get; set; }

    // Submission information
    public required string SubmissionType { get; set; }
    public DateTime SubmissionDate { get; set; }

    public Guid SubmitterId { get; set; }
    public required ApplicationUser Submitter { get; set; }

    // Idea details
    public required string Title { get; set; }
    public required string SummaryDescription { get; set; }
    public required string ProblemStatement { get; set; }
    public required string ProposedSolution { get; set; }

    // Classification
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // Snapshot information
    public int? SubmitterBusinessUnitId { get; set; }
    public int? SubmitterStationId { get; set; }
    public required string SubmitterAgeBracket { get; set; }

    // Workflow
    // Enums make invalid workflow values impossible to persist. EF stores their names
    // (configured in ApplicationDbContext) so existing data remains understandable.
    public IdeaStage CurrentStage { get; set; } = IdeaStage.Submitted;
    public IdeaStatus CurrentStatus { get; set; } = IdeaStatus.UnderReview;

    // Review
    public Guid? AssignedReviewerId { get; set; }

    // Decision
    public DateTime? DecisionDate { get; set; }
    public string? DecisionReason { get; set; }

    // Controls
    public bool IsLocked { get; set; }
    public bool IsRetracted { get; set; }
    public bool IsDeleted { get; set; }

    // SQL Server updates this token automatically; controllers use it to detect
    // when two reviewers attempt to update the same idea concurrently.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Collections
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<IdeaAttachment> Attachments { get; set; } = new List<IdeaAttachment>();
    public ICollection<IdeaTimeline> Timeline { get; set; } = new List<IdeaTimeline>();
}
}

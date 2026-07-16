using Template.Common.AuditColumn;
using Template.Common.Enums;

namespace Template.Data.Entities
{
public class IdeaTimeline : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid IdeaId { get; set; }
    public required InnovationIdea Idea { get; set; }

    public int StageId { get; set; }
    public IdeaStage Stage { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime DeadlineDate { get; set; }

    public DateTime? ActualCompletionDate { get; set; }

    public string? OverrideReason { get; set; }

    public Guid? ApprovedById { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
}}
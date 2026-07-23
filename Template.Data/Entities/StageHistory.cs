using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class StageHistory : AuditableEntity
    {
        public long Id { get; set; }

        public Guid IdeaId { get; set; }
        public InnovationIdea Idea { get; set; } = null!;

        public string PreviousStage { get; set; } = string.Empty;

        public string NewStage { get; set; } = string.Empty;

        public string PreviousStatus { get; set; } = string.Empty;

        public string NewStatus { get; set; } = string.Empty;

        public Guid ChangedById { get; set; }
        public ApplicationUser ChangedBy { get; set; } = null!;

        public string? ChangeReason { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}

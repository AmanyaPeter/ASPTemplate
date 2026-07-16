using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class StageHistory : AuditableEntity
    {
        public long Id { get; set; }

        public Guid IdeaId { get; set; }
        public InnovationIdea Idea { get; set; }

        public string PreviousStage { get; set; }

        public string NewStage { get; set; }

        public string PreviousStatus { get; set; }

        public string NewStatus { get; set; }

        public Guid ChangedById { get; set; }
        public ApplicationUser ChangedBy { get; set; }

        public string? ChangeReason { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}
using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class StageHistory : AuditableEntity
    {
        public long Id { get; set; }

        public Guid IdeaId { get; set; }
        public InnovationIdea Idea { get; set; } = null!;

        public Template.Common.Enums.IdeaStage PreviousStage { get; set; }
        public Template.Common.Enums.IdeaStage NewStage { get; set; }
        public Template.Common.Enums.IdeaStatus PreviousStatus { get; set; }
        public Template.Common.Enums.IdeaStatus NewStatus { get; set; }

        public Guid ChangedById { get; set; }
        public ApplicationUser ChangedBy { get; set; } = null!;

        public string? ChangeReason { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}

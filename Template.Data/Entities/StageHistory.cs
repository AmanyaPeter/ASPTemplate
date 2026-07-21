using Template.Common.AuditColumn;
using Template.Common.Enums;

namespace Template.Data.Entities
{
    public class StageHistory : AuditableEntity
    {
        public long Id { get; set; }

        public Guid IdeaId { get; set; }
        public required InnovationIdea Idea { get; set; }

        public IdeaStage PreviousStage { get; set; }

        public IdeaStage NewStage { get; set; }

        public IdeaStatus PreviousStatus { get; set; }

        public IdeaStatus NewStatus { get; set; }

        public Guid ChangedById { get; set; }
        public required ApplicationUser ChangedBy { get; set; }

        public string? ChangeReason { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}

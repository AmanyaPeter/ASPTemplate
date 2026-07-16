namespace Template.Data.Entities
{
    public class SurveyResponse : AuditableEntity
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public Guid? IdeaId { get; set; }
        public InnovationIdea? Idea { get; set; }

        public required string SurveyType { get; set; }

        public required string ResponseData { get; set; } // JSON format

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
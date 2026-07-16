namespace Template.Data.Entities
{
    public class SurveyResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public Guid? IdeaId { get; set; }
        public string SurveyType { get; set; }
        public string ResponseData { get; set; } // JSON format
        
        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual InnovationIdea Idea { get; set; }
        
        // Audit Fields
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
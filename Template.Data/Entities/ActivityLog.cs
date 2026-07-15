namespace Template.Data.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ActionType { get; set; }
        public string? ActionDetails { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? SessionId { get; set; }
        
        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
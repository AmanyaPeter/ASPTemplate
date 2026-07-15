namespace Template.Data.Entities
{
    public class UserSession
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceName { get; set; }
        public string? Browser { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastActivityTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        
        
    }
}
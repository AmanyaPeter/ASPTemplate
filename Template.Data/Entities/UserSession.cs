namespace Template.Data.Entities
{
    public class UserSession
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public required string SessionId { get; set; }

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

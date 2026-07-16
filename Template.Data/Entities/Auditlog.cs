using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class AuditLog : IAuditableEntity 
    {
        public int Id { get; set; }
        public string LogEntryId { get; set; } // AUD-YYYYMMDD-XXXXX
        public string UserId { get; set; }
        public User User { get; set; }

        public string Username { get; set; } // Denormalized for performance
        public string EventType { get; set; } // Login, Logout, Create, Edit, Delete, View
        public string OperationPerformed { get; set; }
        public string? SourceIP { get; set; }
        public string? DestinationIP { get; set; }
        public string? SourceName { get; set; }
        public string? DestinationName { get; set; }
        public string? AffectedEntityType { get; set; } // Idea, User, Category
        public string? AffectedEntityId { get; set; }
        public string? OldValues { get; set; } // JSON format
        public string? NewValues { get; set; } // JSON format
        public string Status { get; set; } // Success, Failed
        public string? ErrorMessage { get; set; }
        public string? RequestData { get; set; } // JSON format
        public string? ResponseData { get; set; } // JSON format
        public string? SessionId { get; set; }  
        public string? UserAgent { get; set; }
        public string? ActionDetails { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
        public string ModifiedBy { get; set; } = "system";
    }
}

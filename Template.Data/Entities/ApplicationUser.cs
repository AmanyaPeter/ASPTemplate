using Microsoft.AspNetCore.Identity;
using static Template.Common.Static.SystemPermissions;

namespace Template.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Personal Information
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; } // Job Title/Rank

        // BOU Specific Fields
        public string BusinessUnit { get; set; } // Department/Business Unit
        public string? StationLocation { get; set; } // Where stationed
        public string? AgeBracket { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName => $"{FirstName} {LastName}";

        // Account Status
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public string? LockReason { get; set; }
        public DateTime? LockedAt { get; set; }
        public DateTime? DisableDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }

        // Session Tracking
        public bool IsLoggedIn { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;

        // Password Reset
        public bool PasswordResetRequired { get; set; } = false;
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // Role (Custom Role ID - not using IdentityRole directly)
        public int? RoleId { get; set; }
        /*
        public virtual Role Role { get; set; }
        
        // Navigation Properties
        public virtual ICollection<InnovationIdea> SubmittedIdeas { get; set; }
        public virtual ICollection<InnovationIdea> ReviewedIdeas { get; set; }
        public virtual ICollection<InnovationIdea> ApprovedIdeas { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
        public virtual ICollection<InnovationDraft> Drafts { get; set; }
        public virtual ICollection<StageHistory> StageHistories { get; set; }
        public virtual ICollection<IdeaTimeline> IdeaTimelines { get; set; }
        public virtual ICollection<Resource> Resources { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
        public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; }
        */
        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
using Template.Common.AuditColumn;
using Template.Common.Enums;

namespace Template.Data.Entities
{
    public class NotificationPreference : AuditableEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Email Notification Preferences
        public bool IdeaSubmissionConfirmations { get; set; } = true;
        public bool StatusChangeUpdates { get; set; } = true;
        public bool CommentsOnMyIdeas { get; set; } = true;
        public bool MentionsInComments { get; set; } = true;
        public bool WeeklyDigest { get; set; } = false;
        public bool SystemAnnouncements { get; set; } = true;

        // In-App Notification Preferences
        public bool AllInAppNotifications { get; set; } = true;
        public bool SoundAlerts { get; set; } = true;
        public bool DesktopNotifications { get; set; } = false;

        // Timing Preferences
        public DigestFrequency? DigestFrequency { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }
}

namespace Template.Data.Entities
{
    public class NotificationPreference
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        
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
        public string? DigestFrequency { get; set; } // Daily, Weekly, Monthly
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        
        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
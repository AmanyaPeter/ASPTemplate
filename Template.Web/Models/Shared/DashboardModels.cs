using Template.Web.Models.Notification;

namespace Template.Web.Models.Shared
{
    public class AdminDashboardModel
    {
        public AdminStatsViewModel Stats { get; set; } = new();
        public List<RecentUserViewModel> RecentUsers { get; set; } = new();
    }

    public class AdminStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedAccounts { get; set; }
        public int RecentActivities { get; set; }
    }

    public class RecentUserViewModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsOnline { get; set; }
    }

    public class InnovationDashboardModel
    {
        public InnovationStatsViewModel Stats { get; set; } = new();
        public List<RecentIdeaViewModel> RecentIdeas { get; set; } = new();
        public List<ReviewQueueItemViewModel> ReviewQueue { get; set; } = new();
        public List<SLADeadlineViewModel> SLADeadlines { get; set; } = new();
    }

    public class InnovationStatsViewModel
    {
        public int TotalIdeas { get; set; }
        public int PendingReviews { get; set; }
        public int ConceptDevelopment { get; set; }
        public int Experimentation { get; set; }
        public int Deployment { get; set; }
    }

    public class RecentIdeaViewModel
    {
        public string? Title { get; set; }
        public string? Submitter { get; set; }
        public string? Status { get; set; }
        public DateTime Date { get; set; }
    }

    public class ReviewQueueItemViewModel
    {
        public string? Title { get; set; }
        public string? StatusClass { get; set; }
        public string? StatusText { get; set; }
    }

    public class SLADeadlineViewModel
    {
        public string? Idea { get; set; }
        public string? Reviewer { get; set; }
        public DateTime Deadline { get; set; }
        public string? Status { get; set; }
        public string? StatusClass { get; set; }
    }

    public class StaffDashboardModel
    {
        public int TotalIdeas { get; set; }
        public int UnderReview { get; set; }
        public int Approved { get; set; }
        public int Completed { get; set; }
        public List<RecentSubmissionViewModel> RecentSubmissions { get; set; } = new();
        public List<NotificationItemViewModel> RecentNotifications { get; set; } = new();
    }

    public class RecentSubmissionViewModel
    {
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public DateTime Date { get; set; }
    }
}

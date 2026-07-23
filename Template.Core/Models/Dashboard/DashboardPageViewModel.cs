using Template.Common.Enums;

namespace Template.Core.Models.Dashboard;

public class DashboardPageViewModel
{
    public StaffDashboardViewModel Staff { get; set; } = new();
    public InnovationTeamDashboardViewModel InnovationTeam { get; set; } = new();
    public ItAdminDashboardViewModel ItAdmin { get; set; } = new();
}

public class StaffDashboardViewModel
{
    public int TotalIdeas { get; set; }
    public int IdeasUnderReview { get; set; }
    public int ApprovedIdeas { get; set; }
    public int CompletedIdeas { get; set; }
    public List<DashboardIdeaViewModel> RecentIdeas { get; set; } = [];
    public List<DashboardNotificationViewModel> RecentNotifications { get; set; } = [];
}

public class InnovationTeamDashboardViewModel
{
    public int TotalActiveIdeas { get; set; }
    public int PendingReviewIdeas { get; set; }
    public int ConceptDevelopmentIdeas { get; set; }
    public int ExperimentationIdeas { get; set; }
    public int DeploymentIdeas { get; set; }
    public List<DashboardIdeaViewModel> ReviewQueue { get; set; } = [];
    public List<StageDeadlineViewModel> StageDeadlines { get; set; } = [];
}

public class ItAdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedAccounts { get; set; }
    public int DisabledAccounts { get; set; }
    public List<RecentUserViewModel> RecentUsers { get; set; } = [];
    public List<RecentAuditActivityViewModel> RecentAuditActivity { get; set; } = [];
}

public class DashboardIdeaViewModel
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Uncategorized";
    public string Stage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public bool IsRetracted { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public string SubmitterDepartment { get; set; } = string.Empty;
}

public class DashboardNotificationViewModel
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class StageDeadlineViewModel
{
    public Guid IdeaId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string IdeaTitle { get; set; } = string.Empty;
    public IdeaStage Stage { get; set; }
    public DateTime DeadlineDate { get; set; }
    public bool IsOverdue { get; set; }
}

public class RecentUserViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastAccessTime { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class RecentAuditActivityViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public AuditEventType EventType { get; set; }
    public string OperationPerformed { get; set; } = string.Empty;
    public AuditStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}

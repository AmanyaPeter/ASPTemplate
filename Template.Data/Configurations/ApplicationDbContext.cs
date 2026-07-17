using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Template.Data.Configurations;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
    {
    }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<IdeaTimeline> IdeaTimelines { get; set; }
    public DbSet<InnovationIdea> InnovationIdeas { get; set; }
    public DbSet<StageHistory> StageHistories { get; set; }
    public DbSet<InnovationDraft> InnovationDrafts { get; set; }
    public DbSet<IdeaAttachment> IdeaAttachments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<TimelineSetting> TimelineSettings { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
}
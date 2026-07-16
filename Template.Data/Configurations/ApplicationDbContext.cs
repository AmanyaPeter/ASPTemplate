using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Template.Data.Configurations;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
    {
    }
    public DbSet<Role> Roles { get; set; }
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
    public DbSet<BackupHistory> BackupHistories { get; set; }
    // Decide first: do you actually want the mine/ entities (Category, Comment,
    // IdeaTimeline, InnovationIdea, Role, StageHistory, User) too? If yes, add
    // them here — but first delete the duplicate Report/Resource from one folder.

}




  


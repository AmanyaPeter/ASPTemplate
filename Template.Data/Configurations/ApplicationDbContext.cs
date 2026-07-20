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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Fix multiple cascade paths by disabling cascade delete on these relationships
        builder.Entity<Comment>()
            .HasOne(c => c.Idea)
            .WithMany(i => i.Comments)
            .HasForeignKey(c => c.IdeaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<InnovationIdea>()
            .HasOne(i => i.Submitter)
            .WithMany(u => u.Ideas)
            .HasForeignKey(i => i.SubmitterId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<InnovationIdea>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Ideas)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<IdeaAttachment>()
            .HasOne(a => a.Idea)
            .WithMany(i => i.Attachments)
            .HasForeignKey(a => a.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<IdeaAttachment>()
            .HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
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
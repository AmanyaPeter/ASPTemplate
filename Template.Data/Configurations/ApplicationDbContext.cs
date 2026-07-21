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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity is the only role system. This avoids the former custom Roles table
        // shadowing IdentityDbContext.Roles and producing conflicting assignments.
        ConfigureApplicationUser(builder);
        ConfigureInnovationIdea(builder);
        ConfigureSupportingEntities(builder);
    }

    private static void ConfigureApplicationUser(ModelBuilder builder)
    {
        var user = builder.Entity<ApplicationUser>();
        user.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        user.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        user.Property(x => x.MiddleName).HasMaxLength(100);
        user.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        user.Property(x => x.Title).IsRequired().HasMaxLength(100);
        user.Property(x => x.BusinessUnit).IsRequired().HasMaxLength(150);
        user.Property(x => x.JobTitle).IsRequired().HasMaxLength(150);
        user.Property(x => x.Station).IsRequired().HasMaxLength(150);
        user.Property(x => x.AgeBracket).IsRequired().HasMaxLength(30);
        user.Property(x => x.Gender).IsRequired().HasMaxLength(30);
        user.Property(x => x.LockReason).HasMaxLength(500);
    }

    private static void ConfigureInnovationIdea(ModelBuilder builder)
    {
        var idea = builder.Entity<InnovationIdea>();
        idea.HasIndex(x => x.ReferenceNumber).IsUnique();
        idea.Property(x => x.ReferenceNumber).HasMaxLength(40);
        idea.Property(x => x.SubmissionType).HasMaxLength(30);
        idea.Property(x => x.Title).HasMaxLength(250);
        idea.Property(x => x.SummaryDescription).HasMaxLength(4000);
        idea.Property(x => x.ProblemStatement).HasMaxLength(4000);
        idea.Property(x => x.ProposedSolution).HasMaxLength(4000);
        idea.Property(x => x.SubmitterAgeBracket).HasMaxLength(30);
        idea.Property(x => x.CurrentStage).HasConversion<string>().HasMaxLength(40);
        idea.Property(x => x.CurrentStatus).HasConversion<string>().HasMaxLength(40);
        idea.Property(x => x.DecisionReason).HasMaxLength(2000);
        idea.Property(x => x.RowVersion).IsRowVersion();

        // Restrict user deletion because ideas are permanent business records.
        idea.HasOne(x => x.Submitter)
            .WithMany(x => x.Ideas)
            .HasForeignKey(x => x.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict);

        idea.HasOne(x => x.Category)
            .WithMany(x => x.Ideas)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureSupportingEntities(ModelBuilder builder)
    {
        ConfigureAuditableColumns(builder);

        var category = builder.Entity<Category>();
        category.HasIndex(x => x.Name).IsUnique();
        category.Property(x => x.Name).HasMaxLength(150);
        category.Property(x => x.Description).HasMaxLength(1000);

        // NoAction prevents SQL Server's multiple-cascade-path error and preserves
        // comments for the audit trail when a user account is disabled or removed.
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
        builder.Entity<Comment>().Property(x => x.CommentText).HasMaxLength(4000);
        builder.Entity<Comment>()
            .HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);

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
        builder.Entity<IdeaAttachment>().Property(x => x.FileName).HasMaxLength(255);
        builder.Entity<IdeaAttachment>().Property(x => x.FilePath).HasMaxLength(1000);
        builder.Entity<IdeaAttachment>().Property(x => x.FileType).HasMaxLength(50);
        builder.Entity<IdeaAttachment>().Property(x => x.MimeType).HasMaxLength(150);

        builder.Entity<StageHistory>().Property(x => x.PreviousStage).HasConversion<string>().HasMaxLength(40);
        builder.Entity<StageHistory>().Property(x => x.NewStage).HasConversion<string>().HasMaxLength(40);
        builder.Entity<StageHistory>().Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(40);
        builder.Entity<StageHistory>().Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(40);
        builder.Entity<StageHistory>().Property(x => x.ChangeReason).HasMaxLength(2000);

        builder.Entity<Notification>().Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Entity<Notification>().Property(x => x.Subject).HasMaxLength(250);
        builder.Entity<Notification>().Property(x => x.Message).HasMaxLength(4000);
        builder.Entity<Notification>().Property(x => x.LinkUrl).HasMaxLength(1000);

        builder.Entity<Resource>().Property(x => x.Category).HasConversion<string>().HasMaxLength(50);
        builder.Entity<Resource>().Property(x => x.ResourceTitle).HasMaxLength(250);
        builder.Entity<Resource>().Property(x => x.FileName).HasMaxLength(255);
        builder.Entity<Resource>().Property(x => x.FilePath).HasMaxLength(1000);
        builder.Entity<Resource>().Property(x => x.FileType).HasMaxLength(50);
        builder.Entity<Resource>().Property(x => x.MimeType).HasMaxLength(150);
        builder.Entity<Resource>().Property(x => x.Tags).HasMaxLength(500);

        builder.Entity<Report>().Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Entity<Report>().Property(x => x.Format).HasConversion<string>().HasMaxLength(20);
        builder.Entity<Report>().Property(x => x.Frequency).HasConversion<string>().HasMaxLength(20);
        builder.Entity<Report>().Property(x => x.ReportName).HasMaxLength(250);
        builder.Entity<Report>().Property(x => x.FilePath).HasMaxLength(1000);

        builder.Entity<SystemSetting>().HasIndex(x => x.SettingKey).IsUnique();
        builder.Entity<SystemSetting>().Property(x => x.SettingKey).HasMaxLength(150);
        builder.Entity<SystemSetting>().Property(x => x.SettingType).HasMaxLength(30);
        builder.Entity<TimelineSetting>().HasIndex(x => x.StageName).IsUnique();
        builder.Entity<TimelineSetting>().Property(x => x.StageName).HasMaxLength(100);
        builder.Entity<UserSession>().HasIndex(x => x.SessionId).IsUnique();
        builder.Entity<UserSession>().Property(x => x.SessionId).HasMaxLength(200);
    }

    private static void ConfigureAuditableColumns(ModelBuilder builder)
    {
        // Apply common limits once to every AuditableEntity-derived model.
        foreach (var entityType in builder.Model.GetEntityTypes()
                     .Where(x => typeof(Template.Common.AuditColumn.IAuditableEntity)
                         .IsAssignableFrom(x.ClrType)))
        {
            builder.Entity(entityType.ClrType).Property(nameof(Template.Common.AuditColumn.IAuditableEntity.CreatedBy)).HasMaxLength(256);
            builder.Entity(entityType.ClrType).Property(nameof(Template.Common.AuditColumn.IAuditableEntity.ModifiedBy)).HasMaxLength(256);
        }
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

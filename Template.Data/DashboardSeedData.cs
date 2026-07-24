using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Data;

public static class DashboardSeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var staff = await userManager.FindByNameAsync("staff");
        var admin = await userManager.FindByNameAsync("admin");
        if (staff == null || admin == null)
        {
            return;
        }
        var category = await context.Categories
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync();
        if (category == null)
        {
            category = new Category
            {
                Name = "Process Improvement",
                Description = "Workflow and operational improvements",
                IsActive = true
            };
            context.Categories.Add(category);
        await context.SaveChangesAsync();
        }
        if (!await context.InnovationIdeas.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var ideas = new[]
            {
                CreateIdea(staff, category, "IMTS-DEMO-001",
                    "Digital document approval workflow",
                    IdeaStage.Submitted, IdeaStatus.UnderReview, now.AddDays(-5)),
                CreateIdea(staff, category, "IMTS-DEMO-002",
                    "Automated stakeholder feedback analysis",
                    IdeaStage.ConceptDevelopment, IdeaStatus.Approved, now.AddDays(-25)),
                CreateIdea(staff, category, "IMTS-DEMO-003",
                    "Branch service queue optimization",
                    IdeaStage.Experimentation, IdeaStatus.Approved, now.AddDays(-55)),
                CreateIdea(staff, category, "IMTS-DEMO-004",
                    "Paperless internal request tracking",
                    IdeaStage.Closed, IdeaStatus.Approved, now.AddDays(-100))
            };

            context.InnovationIdeas.AddRange(ideas);
            context.IdeaTimelines.AddRange(
                CreateTimeline(ideas[0], IdeaStage.Submitted, 30),
                CreateTimeline(ideas[1], IdeaStage.ConceptDevelopment, 60),
                CreateTimeline(ideas[2], IdeaStage.Experimentation, 90),
                CreateTimeline(ideas[3], IdeaStage.Closed, 0, now.AddDays(-20)));

            context.Notifications.AddRange(
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = staff.Id,
                    IdeaId = ideas[0].Id,
                    Type = NotificationType.IdeaSubmitted,
                    Subject = "Idea received",
                    Message = $"{ideas[0].ReferenceNumber} is awaiting review.",
                    CreatedDate = now.AddDays(-5)
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = staff.Id,
                    IdeaId = ideas[1].Id,
                    Type = NotificationType.ApprovalDecision,
                    Subject = "Idea approved",
                    Message = $"{ideas[1].ReferenceNumber} was approved for concept development.",
                    CreatedDate = now.AddDays(-2)
                });
        }
        if (!await context.AuditLogs.AnyAsync())
        {
            context.AuditLogs.AddRange(
                CreateAudit(admin, AuditEventType.LoginSuccess,
                    "Administrator signed in", AuditStatus.Success),
                CreateAudit(admin, AuditEventType.AccountCreated,
                    "Development dashboard accounts initialized", AuditStatus.Success));
        }
        await context.SaveChangesAsync();
    }

    private static InnovationIdea CreateIdea(
        ApplicationUser staff,
        Category category,
        string reference,
        string title,
        IdeaStage stage,
        IdeaStatus status,
        DateTime submittedAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = reference,
            SubmissionType = "individual",
            SubmissionDate = submittedAt,
            SubmitterId = staff.Id,
            Title = title,
            SummaryDescription = $"Demonstration data for {title}.",
            ProblemStatement = "The current process is manual and difficult to track.",
            ProposedSolution = "Introduce a measurable digital workflow with clear ownership.",
            CategoryId = category.Id,
            SubmitterAgeBracket = staff.AgeBracket,
            CurrentStage = stage,
            CurrentStatus = status,
            RowVersion = new byte[8],
            CreatedDate = submittedAt,
            CreatedBy = staff.Id.ToString()
        };

    private static IdeaTimeline CreateTimeline(
        InnovationIdea idea,
        IdeaStage stage,
        int durationDays,
        DateTime? completedAt = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            IdeaId = idea.Id,
            Idea = idea,
            StageId = (int)stage,
            Stage = stage,
            StartDate = idea.SubmissionDate,
            DeadlineDate = idea.SubmissionDate.AddDays(durationDays),
            ActualCompletionDate = completedAt,
            CreatedDate = idea.SubmissionDate,
            CreatedBy = idea.SubmitterId.ToString()
        };

    private static AuditLog CreateAudit(
        ApplicationUser user,
        AuditEventType eventType,
        string operation,
        AuditStatus status) =>
        new()
        {
            LogEntryId = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            Username = user.UserName ?? "admin",
            EventType = eventType,
            OperationPerformed = operation,
            SourceName = Environment.MachineName,
            Status = status,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.Id.ToString()
        };
}

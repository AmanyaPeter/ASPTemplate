using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Template.Common.Enums;
using Template.Core.Configuration;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Web.Services;

public sealed class DeadlineReminderWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ReminderOptions> options,
    ILogger<DeadlineReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await CreateRemindersAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Deadline reminder scan failed."); }
            await Task.Delay(TimeSpan.FromMinutes(options.Value.PollIntervalMinutes), stoppingToken);
        }
    }

    private async Task CreateRemindersAsync(CancellationToken token)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var boundary = DateTime.UtcNow.AddDays(options.Value.ApproachingDueDays);
        var timelines = await db.IdeaTimelines.Include(x => x.Idea).ThenInclude(x => x.Submitter)
            .Where(x => x.ActualCompletionDate == null && x.DeadlineDate <= boundary).ToListAsync(token);
        foreach (var timeline in timelines)
        {
            var kind = timeline.DeadlineDate < DateTime.UtcNow ? "Overdue" : "ApproachingDue";
            var due = DateOnly.FromDateTime(timeline.DeadlineDate);
            if (await db.ReminderExecutions.AnyAsync(x => x.IdeaTimelineId == timeline.Id &&
                x.ReminderKind == kind && x.DueDate == due, token)) continue;
            db.ReminderExecutions.Add(new ReminderExecution
                { Id = Guid.NewGuid(), IdeaTimelineId = timeline.Id, ReminderKind = kind, DueDate = due, ExecutedAtUtc = DateTime.UtcNow });
            db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(), UserId = timeline.Idea.SubmitterId, IdeaId = timeline.IdeaId,
                Type = NotificationType.DeadlineReminder, Subject = $"Idea deadline {kind}",
                Message = $"{timeline.Idea.ReferenceNumber} is due {timeline.DeadlineDate:dd MMM yyyy}.",
                CreatedDate = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync(token);
    }
}

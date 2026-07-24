using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Template.Core.Configuration;
using Template.Data.Configurations;

namespace Template.Web.Services;

public sealed class EmailOutboxWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<SmtpOptions> options,
    ILogger<EmailOutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await DispatchAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Email outbox dispatch failed."); }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task DispatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;
        var messages = await db.EmailOutbox.Where(x => x.SentAtUtc == null &&
            (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.CreatedAtUtc).Take(20).ToListAsync(cancellationToken);
        foreach (var item in messages)
        {
            try
            {
                var settings = options.Value;
                using var client = new SmtpClient(settings.Host, settings.Port)
                {
                    EnableSsl = settings.EnableSsl,
                    Credentials = string.IsNullOrWhiteSpace(settings.UserName)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(settings.UserName, settings.Password)
                };
                using var message = new MailMessage(settings.FromAddress, item.Recipient, item.Subject, item.Body);
                await client.SendMailAsync(message, cancellationToken);
                item.SentAtUtc = DateTime.UtcNow;
                item.LastError = null;
            }
            catch (Exception ex)
            {
                item.AttemptCount++;
                item.LastError = ex.Message[..Math.Min(ex.Message.Length, 2000)];
                item.NextAttemptAtUtc = DateTime.UtcNow.AddMinutes(Math.Min(60, Math.Pow(2, item.AttemptCount)));
            }
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}

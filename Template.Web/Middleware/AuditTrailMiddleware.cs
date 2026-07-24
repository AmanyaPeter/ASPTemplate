using System.Security.Claims;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Web.Middleware;

public sealed class AuditTrailMiddleware(RequestDelegate next, ILogger<AuditTrailMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var started = DateTime.UtcNow;
        Exception? failure = null;
        try { await next(context); }
        catch (Exception ex) { failure = ex; throw; }
        finally
        {
            if (context.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE")
            {
                try
                {
                    var idText = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userId = Guid.TryParse(idText, out var parsed) ? parsed : (Guid?)null;
                    db.AuditLogs.Add(new AuditLog
                    {
                        LogEntryId = Guid.NewGuid().ToString("N"),
                        UserId = userId,
                        Username = context.User.Identity?.Name ??
                            (context.Request.HasFormContentType ? context.Request.Form["username"].ToString() : "anonymous"),
                        EventType = ResolveEventType(context),
                        OperationPerformed = $"{context.Request.Method} {context.Request.Path}",
                        SourceIP = context.Connection.RemoteIpAddress?.ToString(),
                        DestinationIP = context.Connection.LocalIpAddress?.ToString(),
                        DestinationName = Environment.MachineName,
                        Status = failure == null && context.Response.StatusCode < 400
                            ? AuditStatus.Success : AuditStatus.Failure,
                        ErrorMessage = failure?.GetType().Name,
                        SessionId = context.Session.Id,
                        UserAgent = context.Request.Headers.UserAgent.ToString(),
                        ActionDetails = $"HTTP {context.Response.StatusCode}; {DateTime.UtcNow.Subtract(started).TotalMilliseconds:F0} ms",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId?.ToString() ?? "anonymous"
                    });
                    await db.SaveChangesAsync(CancellationToken.None);
                }
                catch (Exception ex) { logger.LogError(ex, "Unable to persist request audit event."); }
            }
        }
    }

    private static AuditEventType ResolveEventType(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase))
            return context.Response.StatusCode is >= 300 and < 400
                ? AuditEventType.LoginSuccess : AuditEventType.LoginFailure;
        if (path.Contains("/Account/Logout", StringComparison.OrdinalIgnoreCase)) return AuditEventType.Logout;
        if (path.Contains("/Comment", StringComparison.OrdinalIgnoreCase)) return AuditEventType.CommentAdded;
        if (path.Contains("/Category", StringComparison.OrdinalIgnoreCase)) return AuditEventType.CategoryEdited;
        if (path.Contains("/Report", StringComparison.OrdinalIgnoreCase)) return AuditEventType.ReportGenerated;
        if (path.Contains("/Resource", StringComparison.OrdinalIgnoreCase)) return AuditEventType.ResourceUploaded;
        if (path.Contains("/Review", StringComparison.OrdinalIgnoreCase)) return AuditEventType.IdeaStatusChanged;
        return AuditEventType.IdeaEdited;
    }
}

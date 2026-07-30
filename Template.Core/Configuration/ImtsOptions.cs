#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Template.Core.Configuration;

public sealed class ActiveDirectoryOptions
{
    public const string SectionName = "ActiveDirectory";
    [Required] public string Domain { get; set; } = string.Empty;
    public string? Container { get; set; }
    public bool Enabled { get; set; } = true;
}

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    [Required] public string Host { get; set; } = string.Empty;
    [Range(1, 65535)] public int Port { get; set; } = 25;
    [Required, EmailAddress] public string FromAddress { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

public sealed class AttachmentOptions
{
    public const string SectionName = "Attachments";
    [Range(1, 10)] public int MaximumSizeMb { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } =
        [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"];
}

public sealed class ReminderOptions
{
    public const string SectionName = "Reminders";
    [Range(1, 1440)] public int PollIntervalMinutes { get; set; } = 15;
    public int ApproachingDueDays { get; set; } = 3;
}

public sealed class BusinessOptions
{
    public const string SectionName = "Business";
    public string TimeZoneId { get; set; } = "E. Africa Standard Time";
}

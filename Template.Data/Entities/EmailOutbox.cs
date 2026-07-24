namespace Template.Data.Entities;

public sealed class EmailOutbox
{
    public Guid Id { get; set; }
    public required string IdempotencyKey { get; set; }
    public required string Recipient { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}

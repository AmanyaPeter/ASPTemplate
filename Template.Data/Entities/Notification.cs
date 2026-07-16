namespace Template.Data.Entities
{
public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }

    public Guid? IdeaId { get; set; }
    public InnovationIdea? Idea { get; set; }

    public NotificationType Type { get; set; }

    public required string Subject { get; set; }

    public required string Message { get; set; }

    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool IsEmailSent { get; set; }

    public DateTime? EmailSentAt { get; set; }

    public int EmailRetryCount { get; set; }

    public DateTime CreatedDate { get; set; }
}}
namespace Template.Data.Entities;

public sealed class ReminderExecution
{
    public Guid Id { get; set; }
    public Guid IdeaTimelineId { get; set; }
    public required string ReminderKind { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime ExecutedAtUtc { get; set; }
}

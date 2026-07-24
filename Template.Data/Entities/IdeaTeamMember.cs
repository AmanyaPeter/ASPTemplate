namespace Template.Data.Entities;

public sealed class IdeaTeamMember
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public InnovationIdea Idea { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? BusinessUnit { get; set; }
}

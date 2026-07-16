namespace Template.Data.Entities
{
public class InnovationDraft
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }

    public string? Title { get; set; }

    public required string DraftJson { get; set; }

    public int CurrentPage { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastSavedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }
}
}
//this does not require me to add the auditable entity
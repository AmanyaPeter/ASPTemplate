namespace Template.Data.Entities
{
public class InnovationDraft
	{
		public Guid Id { get; set; }
		public string UserId { get; set; }
		public string DraftData { get; set; }
		public int CurrentPage { get; set; }
		public DateTime LastSavedAt { get; set; }
	}
}
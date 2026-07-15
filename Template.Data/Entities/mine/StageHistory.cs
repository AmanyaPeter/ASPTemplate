	namespace ASPTemplate.Template.Data.Entities
    {
    public class StageHistory
	{
		public long Id { get; set; }
		public Guid IdeaId { get; set; }
		public string PreviousStage { get; set; }
		public string NewStage { get; set; }
		public string PreviousStatus { get; set; }
		public string NewStatus { get; set; }
		public Guid ChangedBy { get; set; }
		public string ChangeReason { get; set; }
		public DateTime ChangedAt { get; set; }
	}
    }
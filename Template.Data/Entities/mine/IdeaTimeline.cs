namespace ASPTemplate.Template.Data.Entities
{
    public class IdeaTimeline
	{
		public Guid Id { get; set; }
		public Guid IdeaId { get; set; }
		public string StageName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime DeadlineDate { get; set; }
		public DateTime? ActualCompletionDate { get; set; }
		public bool IsOverdue { get; set; }
		public int DaysOverdue { get; set; }
		public string OverrideReason { get; set; }
		public Guid? ApprovedBy { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public DateTime CreatedDate { get; set; }
		public Guid CreatedBy { get; set; }
	}
}
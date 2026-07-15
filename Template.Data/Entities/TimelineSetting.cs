namespace ASPTemplate.Template.Data.Entities
{   
public class TimelineSetting
	{
		public int Id { get; set; }
		public string StageName { get; set; }
		public int DefaultDays { get; set; }
		public int CurrentDays { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
		public bool AllowOverride { get; set; }
		public bool OverrideRequiresApproval { get; set; }
		public DateTime CreatedDate { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public Guid? UpdatedBy { get; set; }
	}
}
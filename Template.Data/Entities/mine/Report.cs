namespace ASPTemplate.Template.Data.Entities
{
public class Report
	{
		public Guid Id { get; set; }
		public string ReportName { get; set; }
		public string ReportType { get; set; }
		public string ReportFormat { get; set; }
		public string FilePath { get; set; }
		public long FileSize { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string FiltersUsed { get; set; }
		public bool IsScheduled { get; set; }
		public string ScheduleFrequency { get; set; }
		public DateTime GeneratedDate { get; set; }
		public Guid GeneratedBy { get; set; }
		public int DownloadCount { get; set; }
		public bool IsDeleted { get; set; }
	}
}
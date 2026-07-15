namespace ASPTemplate.Template.Data.Entities
{
    public class BackupHistory
	{
		public Guid Id { get; set; }
		public string BackupName { get; set; }
		public string BackupType { get; set; }
		public string BackupFilePath { get; set; }
		public long BackupSize { get; set; }
		public DateTime BackupStartTime { get; set; }
		public DateTime? BackupEndTime { get; set; }
		public string BackupStatus { get; set; }
		public string ErrorMessage { get; set; }
		public Guid CreatedBy { get; set; }
	}
}
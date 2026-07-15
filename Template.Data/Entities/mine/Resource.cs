namespace ASPTemplate.Template.Data.Entities
{
public class Resource
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Category { get; set; }
		public string Description { get; set; }
		public string FileName { get; set; }
		public string FilePath { get; set; }
		public long FileSize { get; set; }
		public string FileType { get; set; }
		public string MimeType { get; set; }
		public string Tags { get; set; }
		public int DownloadCount { get; set; }
		public bool IsActive { get; set; }
		public DateTime UploadedDate { get; set; }
		public Guid UploadedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public Guid? UpdatedBy { get; set; }
	}
}
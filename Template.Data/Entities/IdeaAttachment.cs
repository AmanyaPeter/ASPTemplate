namespace Template.Data.Entities
{
    public class IdeaAttachment
	{
		public Guid Id { get; set; }
		public Guid IdeaId { get; set; }
		public string FileName { get; set; }
		public string FilePath { get; set; }
		public long FileSize { get; set; }
		public string FileType { get; set; }
		public string MimeType { get; set; }
		public DateTime UploadedDate { get; set; }
		public Guid UploadedBy { get; set; }
		public int DownloadCount { get; set; }
		public bool IsDeleted { get; set; }
	}

}
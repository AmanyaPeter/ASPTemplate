namespace Template.Data.Entities
{
    public class Resource
    {
        public int Id { get; set; }
        public string ResourceTitle { get; set; }
        public string Category { get; set; } // Innovation Strategy, Policy, Templates, etc.
        public string? Description { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileType { get; set; }
        public string? MimeType { get; set; }
        public string? Tags { get; set; } // Comma-separated keywords
        public int DownloadCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        
        // Audit Fields
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
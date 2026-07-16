namespace Template.Data.Entities
{
    public class Resource : AuditableEntity
    {
        public int Id { get; set; }

        public required string ResourceTitle { get; set; }

        public ResourceCategory Category { get; set; }

        public string? Description { get; set; }

        public required string FileName { get; set; }

        public required string FilePath { get; set; }

        public long FileSizeBytes { get; set; }

        public required string FileType { get; set; }

        public string? MimeType { get; set; }

        public string? Tags { get; set; }

        public int DownloadCount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Guid UploadedById { get; set; }

        public ApplicationUser UploadedBy { get; set; }
    }
}
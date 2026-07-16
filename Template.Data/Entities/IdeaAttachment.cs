namespace Template.Data.Entities
{
    public class IdeaAttachment : AuditableEntity
    {
        public Guid Id { get; set; }

        public Guid IdeaId { get; set; }
        public InnovationIdea Idea { get; set; }

        public required string FileName { get; set; }

        public required string FilePath { get; set; }

        public long FileSize { get; set; }

        public required string FileType { get; set; }

        public required string MimeType { get; set; }

        public Guid UploadedById { get; set; }
        public ApplicationUser UploadedBy { get; set; }

        public int DownloadCount { get; set; }

        public bool IsDeleted { get; set; }
    }
}
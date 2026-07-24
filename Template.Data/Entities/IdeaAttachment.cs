using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class IdeaAttachment : AuditableEntity
    {
        public Guid Id { get; set; }

        public Guid IdeaId { get; set; }
        public required InnovationIdea Idea { get; set; }

        public required string FileName { get; set; }

        public required string StorageName { get; set; }
        public byte[] Content { get; set; } = [];
        public required string Sha256 { get; set; }

        public long FileSize { get; set; }

        public required string FileType { get; set; }

        public required string MimeType { get; set; }

        public Guid UploadedById { get; set; }
        public required ApplicationUser UploadedBy { get; set; }

        public int DownloadCount { get; set; }

        public bool IsDeleted { get; set; }
    }
}

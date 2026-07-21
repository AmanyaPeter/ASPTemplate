using Template.Common.AuditColumn;
using Template.Common.Enums;

namespace Template.Data.Entities
{
    public class Report : AuditableEntity
    {
        public int Id { get; set; }

        public required string ReportName { get; set; }

        public ReportType Type { get; set; }

        public ReportFormat Format { get; set; }

        public string? FilePath { get; set; }

        public long FileSizeBytes { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? FiltersUsed { get; set; }

        public bool IsScheduled { get; set; }

        public ScheduleFrequency? Frequency { get; set; }

        public int DownloadCount { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime GeneratedAt { get; set; }

        public Guid GeneratedById { get; set; }
        public required ApplicationUser GeneratedBy { get; set; }
    }
}

namespace Template.Data.Entities
{
    public class Report
    {
        public int Id { get; set; }
        public string ReportName { get; set; }
        public string ReportType { get; set; } // KPI, Department, Category, Timeline
        public string ReportFormat { get; set; } // PDF, Excel, CSV
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? FiltersUsed { get; set; } // JSON format
        public bool IsScheduled { get; set; } = false;
        public string? ScheduleFrequency { get; set; } // Daily, Weekly, Monthly
        public int DownloadCount { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
        
        // Audit Fields
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; }
    }
}
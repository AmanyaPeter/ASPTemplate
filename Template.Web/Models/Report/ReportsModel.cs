namespace Template.Web.Models.Report
{
    public class ReportsModel
    {
        public ReportFilterViewModel Filters { get; set; } = new();
        public ReportSummaryViewModel ReportSummary { get; set; } = new();
        public List<ReportIdeaItemViewModel> ReportIdeas { get; set; } = new();
        public List<string?> Departments { get; set; } = new();
        public List<string?> Categories { get; set; } = new();
        public List<string?> CategoryChartLabels { get; set; } = new();
        public List<int> CategoryChartData { get; set; } = new();
        public List<string?> TrendChartLabels { get; set; } = new();
        public List<int> TrendChartData { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class ReportFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Department { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
    }

    public class ReportSummaryViewModel
    {
        public int TotalIdeas { get; set; }
        public int Approved { get; set; }
        public int UnderReview { get; set; }
        public int Declined { get; set; }
    }

    public class ReportIdeaItemViewModel
    {
        public string? Title { get; set; }
        public string? Submitter { get; set; }
        public string? Department { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public DateTime Date { get; set; }
    }
}

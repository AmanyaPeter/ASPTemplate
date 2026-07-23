using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Web.Models.Report;

namespace Template.Web.Controllers;

[Authorize(Roles = "Admin,InnovationTeam")]
public class ReportController(ApplicationDbContext db) : Controller
{
    private const int PageSize = 25;

    [HttpGet]
    public Task<IActionResult> Index(int page = 1) => BuildReport(new ReportFilterViewModel(), page);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Index(ReportsModel model, int page = 1) => BuildReport(model.Filters, page);

    [HttpGet]
    public async Task<IActionResult> Export(string format = "csv")
    {
        var rows = await Query(new ReportFilterViewModel()).OrderByDescending(i => i.SubmissionDate)
            .Select(i => new { i.Title, Submitter = i.Submitter.FullName, Department = i.Submitter.BusinessUnit,
                Category = i.Category != null ? i.Category.Name : "", Status = i.CurrentStatus, i.SubmissionDate })
            .ToListAsync();
        var text = new StringBuilder("Idea Title,Submitter,Department,Category,Status,Date\r\n");
        foreach (var row in rows)
            text.AppendLine(string.Join(",", new[] { row.Title, row.Submitter, row.Department, row.Category, row.Status,
                row.SubmissionDate.ToString("yyyy-MM-dd") }.Select(Csv)));
        var mime = format.Equals("excel", StringComparison.OrdinalIgnoreCase)
            ? "application/vnd.ms-excel" : "text/csv";
        var extension = format.Equals("excel", StringComparison.OrdinalIgnoreCase) ? "xls" : "csv";
        return File(Encoding.UTF8.GetBytes(text.ToString()), mime, $"innovation-report-{DateTime.UtcNow:yyyyMMdd}.{extension}");
    }

    private async Task<IActionResult> BuildReport(ReportFilterViewModel filters, int page)
    {
        page = Math.Max(page, 1);
        var query = Query(filters);
        var all = await query.OrderByDescending(i => i.SubmissionDate).ToListAsync();
        var paged = all.Skip((page - 1) * PageSize).Take(PageSize).Select(i => new ReportIdeaItemViewModel
        {
            Title = i.Title, Submitter = i.Submitter.FullName, Department = i.Submitter.BusinessUnit,
            Category = i.Category?.Name, Status = i.CurrentStatus, Date = i.SubmissionDate
        }).ToList();
        var byCategory = all.GroupBy(i => i.Category?.Name ?? "Uncategorised").OrderBy(g => g.Key).ToList();
        var byMonth = all.GroupBy(i => new DateTime(i.SubmissionDate.Year, i.SubmissionDate.Month, 1))
            .OrderBy(g => g.Key).ToList();
        return View(new ReportsModel
        {
            Filters = filters, ReportIdeas = paged,
            Departments = await db.ApplicationUsers.Select(u => u.BusinessUnit).Distinct().OrderBy(x => x).ToListAsync(),
            Categories = await db.Categories.Where(c => c.IsActive).Select(c => c.Name).OrderBy(x => x).ToListAsync(),
            ReportSummary = new ReportSummaryViewModel
            {
                TotalIdeas = all.Count, Approved = all.Count(i => i.CurrentStatus == "Approved"),
                UnderReview = all.Count(i => i.CurrentStatus == "Under Review"),
                Declined = all.Count(i => i.CurrentStatus == "Declined")
            },
            CategoryChartLabels = byCategory.Select(g => (string?)g.Key).ToList(),
            CategoryChartData = byCategory.Select(g => g.Count()).ToList(),
            TrendChartLabels = byMonth.Select(g => (string?)g.Key.ToString("MMM yyyy")).ToList(),
            TrendChartData = byMonth.Select(g => g.Count()).ToList(),
            CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(all.Count / (double)PageSize))
        });
    }

    private IQueryable<InnovationIdea> Query(ReportFilterViewModel filters)
    {
        var query = db.InnovationIdeas.AsNoTracking().Include(i => i.Submitter).Include(i => i.Category)
            .Where(i => !i.IsDeleted && !i.IsRetracted);
        if (filters.StartDate.HasValue) query = query.Where(i => i.SubmissionDate >= filters.StartDate);
        if (filters.EndDate.HasValue) query = query.Where(i => i.SubmissionDate < filters.EndDate.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(filters.Department))
            query = query.Where(i => i.Submitter.BusinessUnit == filters.Department);
        if (!string.IsNullOrWhiteSpace(filters.Category))
            query = query.Where(i => i.Category != null && i.Category.Name == filters.Category);
        if (!string.IsNullOrWhiteSpace(filters.Status)) query = query.Where(i => i.CurrentStatus == filters.Status);
        return query;
    }

    private static string Csv(string? value) => $"\"{(value ?? "").Replace("\"", "\"\"")}\"";
}

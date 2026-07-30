using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Web.Models.Report;
using Template.Web.Services.Reports;

namespace Template.Web.Controllers;

[Authorize(Roles = $"{RoleConstants.ItAdmin},{RoleConstants.InnovationTeam}")]
public class ReportController(ApplicationDbContext db) : Controller
{
    private const int PageSize = 25;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ReportFilterViewModel filters, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = Query(filters);
        var total = await query.CountAsync();
        var approved = await query.CountAsync(i => i.CurrentStatus == nameof(IdeaStatus.Approved));
        var underReview = await query.CountAsync(i => i.CurrentStatus == nameof(IdeaStatus.UnderReview));
        var declined = await query.CountAsync(i => i.CurrentStatus == nameof(IdeaStatus.Declined));

        var categoryGroups = await query
            .GroupBy(i => i.Category != null ? i.Category.Name : "Uncategorised")
            .Select(group => new { Name = group.Key, Count = group.Count() })
            .OrderBy(group => group.Name)
            .ToListAsync();
        var monthGroups = await query
            .GroupBy(i => new { i.SubmissionDate.Year, i.SubmissionDate.Month })
            .Select(group => new { group.Key.Year, group.Key.Month, Count = group.Count() })
            .OrderBy(group => group.Year).ThenBy(group => group.Month)
            .ToListAsync();

        var rows = await query
            .OrderByDescending(i => i.SubmissionDate)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(i => new ReportIdeaItemViewModel
            {
                Title = i.Title,
                Submitter = i.Submitter.FullName,
                Department = i.Submitter.BusinessUnit,
                Category = i.Category != null ? i.Category.Name : "Uncategorised",
                Status = i.CurrentStatus,
                Date = i.SubmissionDate
            }).ToListAsync();

        var statuses = await db.IdeaStatusOptions.AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new SelectListItem { Value = s.Name, Text = s.Name })
            .ToListAsync();

        return View(new ReportsModel
        {
            Filters = filters,
            Statuses = statuses,
            ReportIdeas = rows,
            Departments = await db.ApplicationUsers.AsNoTracking()
                .Where(user => user.BusinessUnit != null && user.BusinessUnit != "")
                .Select(user => user.BusinessUnit).Distinct().OrderBy(value => value).ToListAsync(),
            Categories = await db.Categories.AsNoTracking().Where(category => category.IsActive)
                .Select(category => category.Name).OrderBy(value => value).ToListAsync(),
            ReportSummary = new ReportSummaryViewModel
            {
                TotalIdeas = total,
                Approved = approved,
                UnderReview = underReview,
                Declined = declined
            },
            CategoryChartLabels = categoryGroups.Select(group => group.Name).ToList(),
            CategoryChartData = categoryGroups.Select(group => group.Count).ToList(),
            TrendChartLabels = monthGroups.Select(group => new DateTime(group.Year, group.Month, 1).ToString("MMM yyyy")).ToList(),
            TrendChartData = monthGroups.Select(group => group.Count).ToList(),
            CurrentPage = page,
            TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize))
        });
    }

    [HttpGet]
    public async Task<IActionResult> Export([FromQuery] ReportFilterViewModel filters, string format = "csv")
    {
        var data = await Query(filters)
            .OrderByDescending(i => i.SubmissionDate)
            .Select(i => new
            {
                i.Title,
                Submitter = i.Submitter.FullName,
                Department = i.Submitter.BusinessUnit,
                Category = i.Category != null ? i.Category.Name : "Uncategorised",
                Status = i.CurrentStatus,
                i.SubmissionDate
            }).ToListAsync();
        var rows = data.Select(i => new[]
        {
            i.Title,
            i.Submitter,
            i.Department,
            i.Category,
            i.Status,
            i.SubmissionDate.ToString("yyyy-MM-dd")
        }).ToList();
        rows.Insert(0, new[] { "Idea Title", "Submitter", "Department", "Category", "Status", "Date" });

        if (format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return File(SimpleXlsxWriter.Create(rows),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"innovation-report-{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }

        var csv = new StringBuilder();
        foreach (var row in rows)
            csv.AppendLine(string.Join(",", row.Select(Csv)));
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv",
            $"innovation-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private IQueryable<Template.Data.Entities.InnovationIdea> Query(ReportFilterViewModel filters)
    {
        var query = db.InnovationIdeas.AsNoTracking()
            .Where(idea => !idea.IsDeleted && !idea.IsRetracted);
        if (filters.StartDate.HasValue) query = query.Where(idea => idea.SubmissionDate >= filters.StartDate.Value);
        if (filters.EndDate.HasValue) query = query.Where(idea => idea.SubmissionDate < filters.EndDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(filters.Department))
            query = query.Where(idea => idea.Submitter.BusinessUnit == filters.Department);
        if (!string.IsNullOrWhiteSpace(filters.Category))
            query = query.Where(idea => idea.Category != null && idea.Category.Name == filters.Category);
        if (!string.IsNullOrWhiteSpace(filters.Status))
            query = query.Where(idea => idea.CurrentStatus == filters.Status);
        return query;
    }

    private static string Csv(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}


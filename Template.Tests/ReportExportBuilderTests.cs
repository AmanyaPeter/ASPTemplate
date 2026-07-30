using System;
using System.Linq;
using System.Text;
using Template.Web.Services;
using Xunit;

namespace Template.Tests;

public sealed class ReportExportBuilderTests
{
    [Fact]
    public void Pdf_export_includes_every_report_page()
    {
        var rows = Enumerable.Range(1, 100)
            .Select(index => new ReportExportRow(
                $"Idea {index}",
                "Staff User",
                "Operations",
                "Process",
                "UnderReview",
                new DateTime(2026, 1, 1).AddDays(index)))
            .ToList();

        var pdf = Encoding.ASCII.GetString(ReportExportBuilder.BuildPdf(rows));

        Assert.StartsWith("%PDF-1.4", pdf);
        Assert.Contains("/Count 3", pdf);
        Assert.EndsWith("%%EOF", pdf);
        Assert.Contains("Idea 100", pdf);
    }

    [Fact]
    public void Excel_export_contains_the_filtered_rows()
    {
        var rows = new[]
        {
            new ReportExportRow(
                "Paperless approvals",
                "Staff User",
                "Operations",
                "Process",
                "Approved",
                new DateTime(2026, 7, 30))
        };

        var workbook = Encoding.UTF8.GetString(ReportExportBuilder.BuildExcel(rows));

        Assert.Contains("Paperless approvals", workbook);
        Assert.Contains("2026-07-30", workbook);
    }
}

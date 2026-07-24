using System.Globalization;
using System.Text;
using System.Xml;

namespace Template.Web.Services;

public sealed record ReportExportRow(string Title, string Submitter, string Department,
    string Category, string Status, DateTime SubmissionDate);

public static class ReportExportBuilder
{
    public static byte[] BuildExcel(IEnumerable<ReportExportRow> rows)
    {
        var builder = new StringBuilder();
        using var writer = XmlWriter.Create(builder, new XmlWriterSettings
            { Encoding = Encoding.UTF8, Indent = true, OmitXmlDeclaration = false });
        writer.WriteStartElement("Workbook", "urn:schemas-microsoft-com:office:spreadsheet");
        writer.WriteAttributeString("xmlns", "ss", null, "urn:schemas-microsoft-com:office:spreadsheet");
        writer.WriteStartElement("Worksheet");
        writer.WriteAttributeString("ss", "Name", null, "Innovation Ideas");
        writer.WriteStartElement("Table");
        WriteExcelRow(writer, ["Idea Title", "Submitter", "Department", "Category", "Status", "Submission Date"]);
        foreach (var row in rows)
            WriteExcelRow(writer, [row.Title, row.Submitter, row.Department, row.Category, row.Status,
                row.SubmissionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)]);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.Flush();
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public static byte[] BuildPdf(IEnumerable<ReportExportRow> rows)
    {
        var lines = new List<string> { "BANK OF UGANDA", "Innovation Management System - Ideas Report",
            $"Generated (UTC): {DateTime.UtcNow:dd MMM yyyy HH:mm}", "" };
        lines.AddRange(rows.Take(42).Select(x =>
            $"{x.SubmissionDate:yyyy-MM-dd} | {x.Status} | {x.Title} | {x.Department}"));
        var content = new StringBuilder("BT /F1 10 Tf 45 790 Td 13 TL ");
        foreach (var line in lines)
            content.Append('(').Append(EscapePdf(line)).Append(") Tj T* ");
        content.Append("ET");
        var stream = Encoding.ASCII.GetBytes(content.ToString());
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {stream.Length} >>\nstream\n{Encoding.ASCII.GetString(stream)}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };
        using var output = new MemoryStream();
        using var writer = new StreamWriter(output, Encoding.ASCII, 1024, true) { NewLine = "\n" };
        writer.Write("%PDF-1.4\n");
        writer.Flush();
        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(output.Position);
            writer.Write($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
            writer.Flush();
        }
        var xref = output.Position;
        writer.Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) writer.Write($"{offset:0000000000} 00000 n \n");
        writer.Write($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        writer.Flush();
        return output.ToArray();
    }

    private static void WriteExcelRow(XmlWriter writer, IEnumerable<string> values)
    {
        writer.WriteStartElement("Row");
        foreach (var value in values)
        {
            writer.WriteStartElement("Cell");
            writer.WriteStartElement("Data");
            writer.WriteAttributeString("ss", "Type", null, "String");
            writer.WriteString(value ?? string.Empty);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static string EscapePdf(string value) => value
        .Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")
        .Select(character => character <= 127 ? character : '?')
        .Aggregate(new StringBuilder(), (builder, character) => builder.Append(character)).ToString();
}

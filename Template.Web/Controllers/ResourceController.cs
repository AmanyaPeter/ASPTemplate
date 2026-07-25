using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Web.Models.Resource;

namespace Template.Web.Controllers;

[Authorize]
public class ResourceController(ApplicationDbContext db, IWebHostEnvironment environment) : Controller
{
    private const int PageSize = 12;

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, ResourceCategory? categoryFilter, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = db.Resources.AsNoTracking().Where(r => r.IsActive);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(r => r.ResourceTitle.Contains(searchTerm) ||
                (r.Description != null && r.Description.Contains(searchTerm)) ||
                (r.Tags != null && r.Tags.Contains(searchTerm)));
        if (categoryFilter.HasValue) query = query.Where(r => r.Category == categoryFilter);
        var count = await query.CountAsync();
        var items = await query.OrderByDescending(r => r.UploadedAt)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(r => new ResourceItemViewModel
            {
                Id = r.Id, ResourceTitle = r.ResourceTitle, Description = r.Description,
                UploadedAt = r.UploadedAt, Icon = "file-text"
            }).ToListAsync();
        return View(new ResourcesModel
        {
            SearchTerm = searchTerm, CategoryFilter = categoryFilter, Resources = items,
            CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize))
        });
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, bool inline = false)
    {
        var resource = await db.Resources.FindAsync(id);
        if (resource == null || !resource.IsActive) return NotFound();
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "App_Data", "Resources"));
        var relativePath = resource.FilePath.Replace('\\', '/').TrimStart('/');
        const string legacyPrefix = "uploads/resources/";
        if (relativePath.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
            relativePath = relativePath[legacyPrefix.Length..];
        var path = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            !System.IO.File.Exists(path)) return NotFound();
        resource.DownloadCount++;
        await db.SaveChangesAsync();
        return inline
            ? PhysicalFile(path, resource.MimeType ?? "application/octet-stream")
            : PhysicalFile(path, resource.MimeType ?? "application/octet-stream", resource.FileName);
    }
}



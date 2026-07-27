using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Data.Configurations;
using Template.Web.Models.Resource;
using Template.Core.Services.Files;
using Template.Common.Static;
using System.Security.Claims;
using Template.Data.Entities;

namespace Template.Web.Controllers;

[Authorize]
public class ResourceController(ApplicationDbContext db, IDatabaseFileService fileService) : Controller
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
                Category = r.Category, FileName = r.FileName, FileSizeBytes = r.FileSizeBytes,
                DownloadCount = r.DownloadCount, UploadedAt = r.UploadedAt, Icon = "file-text"
            }).ToListAsync();
        return View(new ResourcesModel
        {
            SearchTerm = searchTerm, CategoryFilter = categoryFilter, Resources = items,
            CurrentPage = page, TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize))
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.ItAdmin + "," + RoleConstants.InnovationTeam)]
    public async Task<IActionResult> Upload(ResourceUploadViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || model.File == null) return RedirectToAction(nameof(Index));
        try
        {
            await using var stream = model.File.OpenReadStream();
            var file = await fileService.ValidateAsync(model.File.FileName, model.File.ContentType,
                stream, model.File.Length, cancellationToken);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await db.Users.SingleAsync(x => x.Id == userId, cancellationToken);
            db.Resources.Add(new Resource
            {
                ResourceTitle = model.ResourceTitle.Trim(), Category = model.Category,
                Description = model.Description?.Trim(), FileName = file.OriginalName,
                StorageName = file.StorageName, Content = file.Content, Sha256 = file.Sha256,
                FileSizeBytes = file.Size, FileType = Path.GetExtension(file.OriginalName).TrimStart('.').ToUpperInvariant(),
                MimeType = file.MimeType, UploadedById = userId, UploadedBy = user,
                CreatedDate = DateTime.UtcNow, CreatedBy = userId.ToString()
            });
            await db.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Resource uploaded.";
        }
        catch (InvalidDataException ex) { TempData["ErrorMessage"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.ItAdmin + "," + RoleConstants.InnovationTeam)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var resource = await db.Resources.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resource == null) return NotFound();
        resource.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        TempData["SuccessMessage"] = $"{resource.ResourceTitle} was deleted from the resource library.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, bool inline = false)
    {
        var resource = await db.Resources.FindAsync(id);
        if (resource == null || !resource.IsActive) return NotFound();
        resource.DownloadCount++;
        await db.SaveChangesAsync();
        return inline
            ? File(resource.Content, resource.MimeType ?? "application/octet-stream")
            : File(resource.Content, resource.MimeType ?? "application/octet-stream", resource.FileName);
    }
}

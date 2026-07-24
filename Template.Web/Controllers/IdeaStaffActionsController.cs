using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Enums;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Web.Controllers;

[Authorize]
[Route("Idea")]
public class IdeaStaffActionsController(
    ApplicationDbContext context) : Controller
{
    [Authorize(Roles = RoleConstants.Staff)]
    [HttpPost("Retract/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retract(Guid id)
    {
        var userId = GetCurrentUserId();
        var idea = await context.InnovationIdeas.FirstOrDefaultAsync(item =>
            item.Id == id &&
            item.SubmitterId == userId &&
            !item.IsDeleted &&
            !item.IsRetracted);

        if (idea == null)
        {
            return NotFound();
        }

        if (idea.CurrentStatus == IdeaStatus.Approved ||
            idea.CurrentStage == IdeaStage.Closed)
        {
            TempData["ErrorMessage"] = "Approved or closed ideas cannot be retracted.";
            return RedirectToAction("MyIdeas", "Idea");
        }

        idea.IsRetracted = true;
        idea.ModifiedDate = DateTime.UtcNow;
        idea.ModifiedBy = userId.ToString();
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{idea.ReferenceNumber} was retracted.";
        return RedirectToAction("MyIdeas", "Idea");
    }

    [Authorize(Roles = RoleConstants.Staff)]
    [HttpPost("Cancel/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();
        var idea = await context.InnovationIdeas.FirstOrDefaultAsync(item =>
            item.Id == id &&
            item.SubmitterId == userId &&
            !item.IsDeleted);

        if (idea == null)
        {
            return NotFound();
        }

        if (!idea.IsRetracted)
        {
            TempData["ErrorMessage"] = "Retract the idea before cancelling it.";
            return RedirectToAction("MyIdeas", "Idea");
        }

        idea.IsDeleted = true;
        idea.ModifiedDate = DateTime.UtcNow;
        idea.ModifiedBy = userId.ToString();
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{idea.ReferenceNumber} was cancelled.";
        return RedirectToAction("MyIdeas", "Idea");
    }

    [HttpGet("Attachment/{id:guid}")]
    public async Task<IActionResult> DownloadAttachment(Guid id)
    {
        var userId = GetCurrentUserId();
        var canReview = User.IsInRole(RoleConstants.InnovationTeam);
        var attachment = await context.IdeaAttachments
            .Include(item => item.Idea)
            .FirstOrDefaultAsync(item =>
                item.Id == id &&
                !item.IsDeleted &&
                !item.Idea.IsDeleted &&
                (canReview || item.Idea.SubmitterId == userId));

        if (attachment == null)
        {
            return NotFound();
        }

        attachment.DownloadCount++;
        await context.SaveChangesAsync();
        return File(attachment.Content, attachment.MimeType, attachment.FileName);
    }

    [Authorize(Roles = RoleConstants.Staff)]
    [HttpPost("{id:guid}/Comment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(Guid id, string? newComment)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(newComment))
        {
            TempData["ErrorMessage"] = "Enter a comment before sending.";
            return RedirectToAction("Details", "Idea", new { id });
        }

        var idea = await context.InnovationIdeas.FirstOrDefaultAsync(item =>
            item.Id == id && item.SubmitterId == userId && !item.IsDeleted);
        if (idea == null) return NotFound();

        context.Comments.Add(new Comment
        {
            Id = Guid.NewGuid(), IdeaId = idea.Id, Idea = idea,
            UserId = userId,
            User = await context.Users.SingleAsync(user => user.Id == userId),
            CommentText = newComment.Trim(), IsInternal = false,
            CreatedDate = DateTime.UtcNow, CreatedBy = userId.ToString()
        });
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Comment added.";
        return RedirectToAction("Details", "Idea", new { id });
    }
    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}



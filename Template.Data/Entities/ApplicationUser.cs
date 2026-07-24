using Microsoft.AspNetCore.Identity;
using static Template.Common.Static.SystemPermissions;

namespace Template.Data.Entities
{
  public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? DisableDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsLoggedIn { get; set; }
    public DateTime LastActivity { get; set; }
    public string BusinessUnit { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string AgeBracket { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    /// <summary>True only for tightly controlled local emergency accounts.</summary>
    public bool IsBreakGlassAccount { get; set; }
    public string? LockReason { get; set; }

    public DateTime? LastLoginDate { get; set; }
    public bool PasswordResetRequired { get; set; }

    public DateTime CreatedDate { get; set; }
    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];

  public ICollection<InnovationIdea> Ideas { get; set; } = new List<InnovationIdea>();
}
}

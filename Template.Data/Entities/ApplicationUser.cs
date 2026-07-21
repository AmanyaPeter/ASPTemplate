using Microsoft.AspNetCore.Identity;
namespace Template.Data.Entities
{
public class ApplicationUser : IdentityUser<Guid>
{
    // Identity constructs users through new(), so scalar defaults are used here while
    // EF configuration still marks these columns as required in the database.
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

    // Roles intentionally come only from ASP.NET Identity's AspNetUserRoles table.
    // Keeping a second RoleId here previously allowed the two role systems to disagree.

    public bool IsActive { get; set; } = true;
    public string? LockReason { get; set; }

    public DateTime? LastLoginDate { get; set; }
    public bool PasswordResetRequired { get; set; }

    public DateTime CreatedDate { get; set; }
    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<InnovationIdea> Ideas { get; set; } = new List<InnovationIdea>();
}
}

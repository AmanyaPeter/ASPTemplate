
namespace Template.Data.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } // Staff, InnovationTeam, ITAdmin
        public string? RoleDescription { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

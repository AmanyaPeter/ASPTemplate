namespace Template.Data.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string PermissionName { get; set; }
        public string? PermissionDescription { get; set; }
        public string? Category { get; set; } // Ideas, Users, Reports, Categories, etc.

        // Navigation Properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; }

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}
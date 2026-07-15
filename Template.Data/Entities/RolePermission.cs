namespace Template.Data.Entities
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public string? GrantedBy { get; set; }

        // Navigation Properties
        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }
    }
}

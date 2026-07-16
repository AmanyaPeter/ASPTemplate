namespace Template.Data.Entities
{
public class RolePermission
	{
		public int RoleId { get; set; }
		public int PermissionId { get; set; }
		public DateTime GrantedDate { get; set; }
		public Guid GrantedBy { get; set; }
	}
}
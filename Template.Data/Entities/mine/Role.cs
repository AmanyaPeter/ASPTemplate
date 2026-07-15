namespace Template.Data.Entities{
public class Role
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedDate { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public Guid? UpdatedBy { get; set; }
	}
}
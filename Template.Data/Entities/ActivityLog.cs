namespace Template.Data.Entities
{
public class ActivityLog
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string ActionType { get; set; }
		public string ActionDetails { get; set; }
		public string IpAddress { get; set; }
		public string UserAgent { get; set; }
		public string SessionId { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}
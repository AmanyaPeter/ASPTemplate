namespace ASPTemplate.Template.Data.Entities
{
public class AuditLog
	{
		public long Id { get; set; }
		public string LogEntryIdentifier { get; set; }
		public Guid? UserId { get; set; }
		public string Username { get; set; }
		public string EventType { get; set; }
		public string OperationPerformed { get; set; }
		public string SourceIpAddress { get; set; }
		public string DestinationIpAddress { get; set; }
		public string SourceName { get; set; }
		public string DestinationName { get; set; }
		public string AffectedEntityType { get; set; }
		public string AffectedEntityId { get; set; }
		public string OldValues { get; set; }
		public string NewValues { get; set; }
		public string Status { get; set; }
		public string ErrorMessage { get; set; }
		public string RequestData { get; set; }
		public string ResponseData { get; set; }
		public string SessionId { get; set; }
		public string UserAgent { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}
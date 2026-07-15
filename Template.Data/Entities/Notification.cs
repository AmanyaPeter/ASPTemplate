namespace ASPTemplate.Template.Data.Entities
{
public class Notification
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid? IdeaId { get; set; }
		public string NotificationType { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
		public string LinkUrl { get; set; }
		public bool IsRead { get; set; }
		public bool IsEmailSent { get; set; }
		public DateTime? EmailSentAt { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}
namespace ASPTemplate.Template.Data.Entities
{
public class Comment
	{
		public Guid Id { get; set; }
		public Guid IdeaId { get; set; }
		public Guid UserId { get; set; }
		public string CommentText { get; set; }
		public Guid? ParentCommentId { get; set; }
		public bool IsInternal { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? UpdatedDate { get; set; }
	}
}
namespace Template.Data.Entities
{

    public class Comment : AuditableEntity
    {
        public Guid Id { get; set; }

        public Guid IdeaId { get; set; }
        public InnovationIdea Idea { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public required string CommentText { get; set; }

        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } 
            = new List<Comment>();

        public bool IsInternal { get; set; }

        public bool IsDeleted { get; set; }
    }
}

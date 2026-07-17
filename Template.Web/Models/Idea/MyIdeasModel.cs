namespace Template.Web.Models.Idea
{
    public class MyIdeasModel
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public List<IdeaListItemViewModel> Ideas { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class IdeaListItemViewModel
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? CategoryName { get; set; }
        public string? Stage { get; set; }
        public string? Status { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}

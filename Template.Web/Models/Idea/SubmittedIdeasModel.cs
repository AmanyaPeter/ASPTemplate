using Microsoft.AspNetCore.Mvc.Rendering;

namespace Template.Web.Models.Idea
{
    public class SubmittedIdeasModel
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
        public string? DepartmentFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public List<SubmittedIdeaItemViewModel> Ideas { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class SubmittedIdeaItemViewModel
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Submitter { get; set; }
        public string? Department { get; set; }
        public string? CategoryName { get; set; }
        public string? Stage { get; set; }
        public string? Status { get; set; }
        public DateTime SubmissionDate { get; set; }
        public bool NeedsReview { get; set; }
    }
}

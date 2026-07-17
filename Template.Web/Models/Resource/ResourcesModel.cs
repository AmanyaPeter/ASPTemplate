using Template.Common.Enums;

namespace Template.Web.Models.Resource
{
    public class ResourcesModel
    {
        public string? SearchTerm { get; set; }
        public ResourceCategory? CategoryFilter { get; set; }
        public List<ResourceItemViewModel> Resources { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class ResourceItemViewModel
    {
        public int Id { get; set; }
        public required string ResourceTitle { get; set; }
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Icon { get; set; }
    }
}

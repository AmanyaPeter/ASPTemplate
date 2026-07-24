using Template.Common.Enums;
using System.ComponentModel.DataAnnotations;

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

    public sealed class ResourceUploadViewModel
    {
        [Required, StringLength(200)]
        public string ResourceTitle { get; set; } = string.Empty;
        public ResourceCategory Category { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
        [Required]
        public IFormFile? File { get; set; }
    }
}

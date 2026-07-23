using System.ComponentModel.DataAnnotations;

namespace Template.Web.Models.Category
{
    public class CategoriesModel
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public List<CategoryItemViewModel> Categories { get; set; } = new();
        public CategoryFormViewModel Category { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class CategoryItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}

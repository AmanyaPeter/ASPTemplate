namespace ASPTemplate.Template.Data.Entities
{
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ColorCode { get; set; }
    public string IconName { get; set; }
    public bool IsActive { get; set; }
    public int IdeasCount { get; set; }  // Denormalized for quick stats
    
    public DateTime CreatedDate { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    // Navigation
    //public virtual ICollection<InnovationIdea> Ideas { get; set; }
//    public virtual ICollection<InnovationIdea> Ideas { get; set; } = new List<InnovationIdea>();
}
}
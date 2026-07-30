using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class TypeOfInnovation : AuditableEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

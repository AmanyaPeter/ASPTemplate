using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class BusinessUnit : AuditableEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

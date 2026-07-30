using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Station : AuditableEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

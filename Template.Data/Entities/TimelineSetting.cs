using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class TimelineSetting : AuditableEntity
    {
        public int Id { get; set; }

        public required string StageName { get; set; }

        public int DefaultDays { get; set; }

        public int CurrentDays { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool AllowOverride { get; set; }

        public bool OverrideRequiresApproval { get; set; }
    }
}
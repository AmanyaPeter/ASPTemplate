namespace Template.Data.Entities
{
    public class SystemSetting : AuditableEntity
    {
        public int Id { get; set; }

        public required string SettingKey { get; set; }

        public required string SettingValue { get; set; }

        public string? SettingType { get; set; } // String, Int, Boolean, JSON

        public string? Description { get; set; }

        public bool IsSystemOnly { get; set; } = false;
    }
}
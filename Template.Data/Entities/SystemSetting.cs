namespace Template.Data.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string? SettingType { get; set; } // String, Int, Boolean, JSON
        public string? Description { get; set; }
        public bool IsSystemOnly { get; set; } = false;
        
        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
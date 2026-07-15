namespace Template.Data.Entities
{
    public class BackupHistory
    {
        public int Id { get; set; }
        public string BackupName { get; set; }
        public string BackupType { get; set; } // Full, Differential, TransactionLog
        public string BackupFilePath { get; set; }
        public long BackupSizeBytes { get; set; }
        public DateTime BackupStartTime { get; set; }
        public DateTime BackupEndTime { get; set; }
        public string BackupStatus { get; set; } // Success, Failed
        public string? ErrorMessage { get; set; }
        
        // Audit Fields
        public string CreatedBy { get; set; }
    }
}
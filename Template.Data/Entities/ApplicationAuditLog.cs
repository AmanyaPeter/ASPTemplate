
namespace Template.Data.Entities
{
	public class ApplicationAuditLog
	{
		public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
		public string Level { get; set; }
		public string UserName { get; set; }
        public string Message { get; set; }
        public string MachineName { get; set; }
        public string Logger { get; set; }
	}
}

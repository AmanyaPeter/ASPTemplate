
using System.ComponentModel.DataAnnotations;

namespace Template.Core.Models.AuditLogs
{
	public class ApplicationAuditLogViewModel
	{
		public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:dddd, MMMM dd, yyyy h:mm tt}")]
        public DateTime TimeStamp { get; set; }
		public string Level { get; set; }
		public string UserName { get; set; }
        public string Message { get; set; }
        public string MachineName { get; set; }
        public string Logger { get; set; }
	}
}

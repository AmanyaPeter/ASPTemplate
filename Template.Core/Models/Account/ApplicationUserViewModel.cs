using System.ComponentModel.DataAnnotations;
using Template.Data.Entities;

namespace Template.Core.Models.Account
{
	public class ApplicationUserViewModel
	{
		public string Id  { get; set; }
		public string UserName  { get; set; }
		public string FirstName  { get; set; }
		public string MiddleName  { get; set; }
		public string LastName  { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
		//public string PhoneNumber { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime? DisableDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

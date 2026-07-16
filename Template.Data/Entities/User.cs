namespace Template.Data.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string PasswordSalt { get; set; }
		public string FullName { get; set; }
		public string BusinessUnit { get; set; }
		public string JobTitle { get; set; }
		public string Station { get; set; }
		public string AgeBracket { get; set; }
		public string Gender { get; set; }
		public string PhoneNumber { get; set; }
		public int RoleId { get; set; }
		public bool IsActive { get; set; }
		public bool IsLocked { get; set; }
		public string LockReason { get; set; }
		public DateTime? LastLoginDate { get; set; }
		public int FailedLoginAttempts { get; set; }
		public bool PasswordResetRequired { get; set; }
		public string ResetToken { get; set; }
		public DateTime? ResetTokenExpiryDate { get; set; }
		public DateTime CreatedDate { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public Guid? UpdatedBy { get; set; }

		public ICollection<AuditLog>? AuditLogs { get; set; }
	}
}
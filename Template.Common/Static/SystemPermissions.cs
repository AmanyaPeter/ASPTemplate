namespace Template.Common.Static
{
    public static class SystemPermissions
    {
        public static class Roles // RoleController Actions
        {
            public const string CreateRole = "RolesController Create";
            public const string ViewRoles = "RolesController Index";
            public const string EditRole = "RolesController Edit";
            public const string DeleteRole = "RolesController Delete";
        }
        
        public static class Account
        {
            public const string ViewApplicationUsers = "AccountController Index";
            public const string CreateApplicationUser = "AccountController Create";
            public const string EditApplicationUser = "AccountController Edit";
        }

        public static class AuditLog // AuditLogController Actions
        {
            public const string ViewAuditLogs = "AuditLogController Index";
        }

        public static class Ideas
        {
            public const string Review = "Ideas Review";
            public const string ManageWorkflow = "Ideas ManageWorkflow";
        }

        public static class Governance
        {
            public const string ViewReports = "Governance ViewReports";
            public const string AssignRoles = "Governance AssignRoles";
        }
	}
}

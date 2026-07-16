namespace Template.Common.Enums
{
    // Covers IMTS-REQ-255 through IMTS-REQ-290 (audit log event categories).
    public enum AuditEventType
    {
        LoginSuccess,
        LoginFailure,
        Logout,
        SessionTimeout,
        AccountCreated,
        AccountLocked,
        AccountUnlocked,
        AccountDisabled,
        AccountEnabled,
        PasswordChanged,
        PasswordReset,
        RoleAssigned,
        RoleUnassigned,
        IdeaSubmitted,
        IdeaEdited,
        IdeaRetracted,
        IdeaStageChanged,
        IdeaStatusChanged,
        CommentAdded,
        CategoryCreated,
        CategoryEdited,
        CategoryDeleted,
        ReportGenerated,
        ResourceUploaded,
        ResourceDeleted
    }
}

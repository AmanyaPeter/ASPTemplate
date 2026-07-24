IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [MiddleName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [DisableDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [IsLoggedIn] bit NOT NULL,
        [LastActivity] datetime2 NOT NULL,
        [BusinessUnit] nvarchar(max) NOT NULL,
        [JobTitle] nvarchar(max) NOT NULL,
        [Station] nvarchar(max) NOT NULL,
        [AgeBracket] nvarchar(max) NOT NULL,
        [Gender] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [LockReason] nvarchar(max) NULL,
        [LastLoginDate] datetime2 NULL,
        [PasswordResetRequired] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] int NOT NULL IDENTITY,
        [SettingKey] nvarchar(max) NOT NULL,
        [SettingValue] nvarchar(max) NOT NULL,
        [SettingType] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [IsSystemOnly] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [TimelineSettings] (
        [Id] int NOT NULL IDENTITY,
        [StageName] nvarchar(max) NOT NULL,
        [DefaultDays] int NOT NULL,
        [CurrentDays] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [AllowOverride] bit NOT NULL,
        [OverrideRequiresApproval] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TimelineSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [LogEntryId] nvarchar(max) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Username] nvarchar(max) NOT NULL,
        [EventType] int NOT NULL,
        [OperationPerformed] nvarchar(max) NOT NULL,
        [SourceIP] nvarchar(max) NULL,
        [DestinationIP] nvarchar(max) NULL,
        [SourceName] nvarchar(max) NULL,
        [DestinationName] nvarchar(max) NULL,
        [AffectedEntityType] nvarchar(max) NULL,
        [AffectedEntityId] nvarchar(max) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [RequestData] nvarchar(max) NULL,
        [ResponseData] nvarchar(max) NULL,
        [SessionId] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [ActionDetails] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [InnovationDrafts] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NULL,
        [DraftJson] nvarchar(max) NOT NULL,
        [CurrentPage] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [LastSavedAt] datetime2 NOT NULL,
        [SubmittedAt] datetime2 NULL,
        CONSTRAINT [PK_InnovationDrafts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InnovationDrafts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [NotificationPreferences] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [IdeaSubmissionConfirmations] bit NOT NULL,
        [StatusChangeUpdates] bit NOT NULL,
        [CommentsOnMyIdeas] bit NOT NULL,
        [MentionsInComments] bit NOT NULL,
        [WeeklyDigest] bit NOT NULL,
        [SystemAnnouncements] bit NOT NULL,
        [AllInAppNotifications] bit NOT NULL,
        [SoundAlerts] bit NOT NULL,
        [DesktopNotifications] bit NOT NULL,
        [DigestFrequency] int NULL,
        [QuietHoursStart] time NULL,
        [QuietHoursEnd] time NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NotificationPreferences_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [Reports] (
        [Id] int NOT NULL IDENTITY,
        [ReportName] nvarchar(max) NOT NULL,
        [Type] int NOT NULL,
        [Format] int NOT NULL,
        [FilePath] nvarchar(max) NULL,
        [FileSizeBytes] bigint NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [FiltersUsed] nvarchar(max) NULL,
        [IsScheduled] bit NOT NULL,
        [Frequency] int NULL,
        [DownloadCount] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        [GeneratedAt] datetime2 NOT NULL,
        [GeneratedById] uniqueidentifier NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reports_AspNetUsers_GeneratedById] FOREIGN KEY ([GeneratedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [Resources] (
        [Id] int NOT NULL IDENTITY,
        [ResourceTitle] nvarchar(max) NOT NULL,
        [Category] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileSizeBytes] bigint NOT NULL,
        [FileType] nvarchar(max) NOT NULL,
        [MimeType] nvarchar(max) NULL,
        [Tags] nvarchar(max) NULL,
        [DownloadCount] int NOT NULL,
        [IsActive] bit NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [UploadedById] uniqueidentifier NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Resources] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Resources_AspNetUsers_UploadedById] FOREIGN KEY ([UploadedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [UserSessions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [SessionId] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [DeviceName] nvarchar(max) NULL,
        [Browser] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [LoginTime] datetime2 NOT NULL,
        [LastActivityTime] datetime2 NULL,
        [LogoutTime] datetime2 NULL,
        CONSTRAINT [PK_UserSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserSessions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [InnovationIdeas] (
        [Id] uniqueidentifier NOT NULL,
        [ReferenceNumber] nvarchar(max) NOT NULL,
        [SubmissionType] nvarchar(max) NOT NULL,
        [SubmissionDate] datetime2 NOT NULL,
        [SubmitterId] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [SummaryDescription] nvarchar(max) NOT NULL,
        [ProblemStatement] nvarchar(max) NOT NULL,
        [ProposedSolution] nvarchar(max) NOT NULL,
        [CategoryId] int NULL,
        [SubmitterBusinessUnitId] int NULL,
        [SubmitterStationId] int NULL,
        [SubmitterAgeBracket] nvarchar(max) NOT NULL,
        [CurrentStage] nvarchar(max) NOT NULL,
        [CurrentStatus] nvarchar(max) NOT NULL,
        [AssignedReviewerId] uniqueidentifier NULL,
        [DecisionDate] datetime2 NULL,
        [DecisionReason] nvarchar(max) NULL,
        [IsLocked] bit NOT NULL,
        [IsRetracted] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [RowVersion] varbinary(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_InnovationIdeas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InnovationIdeas_AspNetUsers_SubmitterId] FOREIGN KEY ([SubmitterId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_InnovationIdeas_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [Comments] (
        [Id] uniqueidentifier NOT NULL,
        [IdeaId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CommentText] nvarchar(max) NOT NULL,
        [ParentCommentId] uniqueidentifier NULL,
        [IsInternal] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Comments_Comments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [Comments] ([Id]),
        CONSTRAINT [FK_Comments_InnovationIdeas_IdeaId] FOREIGN KEY ([IdeaId]) REFERENCES [InnovationIdeas] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [IdeaAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [IdeaId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileSize] bigint NOT NULL,
        [FileType] nvarchar(max) NOT NULL,
        [MimeType] nvarchar(max) NOT NULL,
        [UploadedById] uniqueidentifier NOT NULL,
        [DownloadCount] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_IdeaAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IdeaAttachments_AspNetUsers_UploadedById] FOREIGN KEY ([UploadedById]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_IdeaAttachments_InnovationIdeas_IdeaId] FOREIGN KEY ([IdeaId]) REFERENCES [InnovationIdeas] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [IdeaTimelines] (
        [Id] uniqueidentifier NOT NULL,
        [IdeaId] uniqueidentifier NOT NULL,
        [StageId] int NOT NULL,
        [Stage] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [DeadlineDate] datetime2 NOT NULL,
        [ActualCompletionDate] datetime2 NULL,
        [OverrideReason] nvarchar(max) NULL,
        [ApprovedById] uniqueidentifier NULL,
        [ApprovedAt] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_IdeaTimelines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IdeaTimelines_AspNetUsers_ApprovedById] FOREIGN KEY ([ApprovedById]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_IdeaTimelines_InnovationIdeas_IdeaId] FOREIGN KEY ([IdeaId]) REFERENCES [InnovationIdeas] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [IdeaId] uniqueidentifier NULL,
        [Type] int NOT NULL,
        [Subject] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [LinkUrl] nvarchar(max) NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [IsEmailSent] bit NOT NULL,
        [EmailSentAt] datetime2 NULL,
        [EmailRetryCount] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Notifications_InnovationIdeas_IdeaId] FOREIGN KEY ([IdeaId]) REFERENCES [InnovationIdeas] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [StageHistories] (
        [Id] bigint NOT NULL IDENTITY,
        [IdeaId] uniqueidentifier NOT NULL,
        [PreviousStage] nvarchar(max) NOT NULL,
        [NewStage] nvarchar(max) NOT NULL,
        [PreviousStatus] nvarchar(max) NOT NULL,
        [NewStatus] nvarchar(max) NOT NULL,
        [ChangedById] uniqueidentifier NOT NULL,
        [ChangeReason] nvarchar(max) NULL,
        [ChangedAt] datetime2 NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_StageHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StageHistories_AspNetUsers_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StageHistories_InnovationIdeas_IdeaId] FOREIGN KEY ([IdeaId]) REFERENCES [InnovationIdeas] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE TABLE [SurveyResponses] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [IdeaId] uniqueidentifier NULL,
        [SurveyType] nvarchar(max) NOT NULL,
        [ResponseData] nvarchar(max) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SurveyResponses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SurveyResponses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SurveyResponses_InnovationIdeas_IdeaId] FOREIGN KEY ([IdeaId]) REFERENCES [InnovationIdeas] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_Comments_IdeaId] ON [Comments] ([IdeaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_Comments_ParentCommentId] ON [Comments] ([ParentCommentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_IdeaAttachments_IdeaId] ON [IdeaAttachments] ([IdeaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_IdeaAttachments_UploadedById] ON [IdeaAttachments] ([UploadedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_IdeaTimelines_ApprovedById] ON [IdeaTimelines] ([ApprovedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_IdeaTimelines_IdeaId] ON [IdeaTimelines] ([IdeaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_InnovationDrafts_UserId] ON [InnovationDrafts] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_InnovationIdeas_CategoryId] ON [InnovationIdeas] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_InnovationIdeas_SubmitterId] ON [InnovationIdeas] ([SubmitterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_NotificationPreferences_UserId] ON [NotificationPreferences] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_Notifications_IdeaId] ON [Notifications] ([IdeaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_Reports_GeneratedById] ON [Reports] ([GeneratedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_Resources_UploadedById] ON [Resources] ([UploadedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_StageHistories_ChangedById] ON [StageHistories] ([ChangedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_StageHistories_IdeaId] ON [StageHistories] ([IdeaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_SurveyResponses_IdeaId] ON [SurveyResponses] ([IdeaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_SurveyResponses_UserId] ON [SurveyResponses] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    CREATE INDEX [IX_UserSessions_UserId] ON [UserSessions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720095329_InnovationDb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720095329_InnovationDb', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720212701_AddRoleEntity'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [RoleId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720212701_AddRoleEntity'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720212701_AddRoleEntity'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_RoleId] ON [AspNetUsers] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720212701_AddRoleEntity'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260720212701_AddRoleEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260720212701_AddRoleEntity', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    ALTER TABLE [InnovationIdeas] ADD [ExpectedBenefits] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    ALTER TABLE [InnovationIdeas] ADD [ImpactIndicators] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    ALTER TABLE [InnovationIdeas] ADD [ImplementationApproach] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    ALTER TABLE [InnovationIdeas] ADD [KeyEnablers] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    ALTER TABLE [InnovationIdeas] ADD [StrategicObjective] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    ALTER TABLE [InnovationIdeas] ADD [TeamCompositionJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    ALTER TABLE [InnovationIdeas] ADD [TeamMemberNames] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723093526_AddSubmissionFormDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723093526_AddSubmissionFormDetails', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Roles_RoleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DROP TABLE [Roles];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DROP INDEX [IX_AspNetUsers_RoleId] ON [AspNetUsers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'RoleId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [AspNetUsers] DROP COLUMN [RoleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    EXEC sp_rename N'[Resources].[FilePath]', N'StorageName', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    EXEC sp_rename N'[IdeaAttachments].[FilePath]', N'StorageName', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    UPDATE InnovationIdeas SET CurrentStage = CASE REPLACE(CurrentStage, ' ', '')
        WHEN 'Submitted' THEN '0' WHEN 'ConceptDevelopment' THEN '1'
        WHEN 'Experimentation' THEN '2' WHEN 'Experimentation/Research' THEN '2'
        WHEN 'Deployment' THEN '3' WHEN 'Closed' THEN '4' ELSE '0' END;
    UPDATE InnovationIdeas SET CurrentStatus = CASE REPLACE(CurrentStatus, ' ', '')
        WHEN 'PendingInformation' THEN '0' WHEN 'UnderReview' THEN '1'
        WHEN 'Approved' THEN '2' WHEN 'Declined' THEN '3' ELSE '1' END;
    UPDATE StageHistories SET
        PreviousStage = CASE REPLACE(PreviousStage, ' ', '') WHEN 'Submitted' THEN '0' WHEN 'ConceptDevelopment' THEN '1' WHEN 'Experimentation' THEN '2' WHEN 'Experimentation/Research' THEN '2' WHEN 'Deployment' THEN '3' WHEN 'Closed' THEN '4' ELSE '0' END,
        NewStage = CASE REPLACE(NewStage, ' ', '') WHEN 'Submitted' THEN '0' WHEN 'ConceptDevelopment' THEN '1' WHEN 'Experimentation' THEN '2' WHEN 'Experimentation/Research' THEN '2' WHEN 'Deployment' THEN '3' WHEN 'Closed' THEN '4' ELSE '0' END,
        PreviousStatus = CASE REPLACE(PreviousStatus, ' ', '') WHEN 'PendingInformation' THEN '0' WHEN 'UnderReview' THEN '1' WHEN 'Approved' THEN '2' WHEN 'Declined' THEN '3' ELSE '1' END,
        NewStatus = CASE REPLACE(NewStatus, ' ', '') WHEN 'PendingInformation' THEN '0' WHEN 'UnderReview' THEN '1' WHEN 'Approved' THEN '2' WHEN 'Declined' THEN '3' ELSE '1' END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StageHistories]') AND [c].[name] = N'PreviousStatus');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [StageHistories] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [StageHistories] ALTER COLUMN [PreviousStatus] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StageHistories]') AND [c].[name] = N'PreviousStage');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [StageHistories] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [StageHistories] ALTER COLUMN [PreviousStage] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StageHistories]') AND [c].[name] = N'NewStatus');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [StageHistories] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [StageHistories] ALTER COLUMN [NewStatus] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StageHistories]') AND [c].[name] = N'NewStage');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [StageHistories] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [StageHistories] ALTER COLUMN [NewStage] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [Resources] ADD [Content] varbinary(max) NOT NULL DEFAULT 0x;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [Resources] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [Resources] ADD [Sha256] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InnovationIdeas]') AND [c].[name] = N'RowVersion');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [InnovationIdeas] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [InnovationIdeas] ALTER COLUMN [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InnovationIdeas]') AND [c].[name] = N'CurrentStatus');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [InnovationIdeas] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [InnovationIdeas] ALTER COLUMN [CurrentStatus] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InnovationIdeas]') AND [c].[name] = N'CurrentStage');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [InnovationIdeas] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [InnovationIdeas] ALTER COLUMN [CurrentStage] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [IdeaAttachments] ADD [Content] varbinary(max) NOT NULL DEFAULT 0x;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [IdeaAttachments] ADD [Sha256] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsBreakGlassAccount] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    CREATE TABLE [EmailOutbox] (
        [Id] uniqueidentifier NOT NULL,
        [IdempotencyKey] nvarchar(450) NOT NULL,
        [Recipient] nvarchar(max) NOT NULL,
        [Subject] nvarchar(max) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [NextAttemptAtUtc] datetime2 NULL,
        [SentAtUtc] datetime2 NULL,
        [AttemptCount] int NOT NULL,
        [LastError] nvarchar(max) NULL,
        CONSTRAINT [PK_EmailOutbox] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    CREATE TABLE [IdeaTeamMembers] (
        [Id] uniqueidentifier NOT NULL,
        [IdeaId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [BusinessUnit] nvarchar(max) NULL,
        CONSTRAINT [PK_IdeaTeamMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IdeaTeamMembers_InnovationIdeas_IdeaId] FOREIGN KEY ([IdeaId]) REFERENCES [InnovationIdeas] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    CREATE TABLE [ReminderExecutions] (
        [Id] uniqueidentifier NOT NULL,
        [IdeaTimelineId] uniqueidentifier NOT NULL,
        [ReminderKind] nvarchar(450) NOT NULL,
        [DueDate] date NOT NULL,
        [ExecutedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_ReminderExecutions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EmailOutbox_IdempotencyKey] ON [EmailOutbox] ([IdempotencyKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    CREATE INDEX [IX_IdeaTeamMembers_IdeaId] ON [IdeaTeamMembers] ([IdeaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReminderExecutions_IdeaTimelineId_ReminderKind_DueDate] ON [ReminderExecutions] ([IdeaTimelineId], [ReminderKind], [DueDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723191723_CompleteAndHardenImts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723191723_CompleteAndHardenImts', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723192816_SynchronizeImtsModel'
)
BEGIN
    ALTER TABLE [AuditLogs] DROP CONSTRAINT [FK_AuditLogs_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723192816_SynchronizeImtsModel'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'UserId');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [UserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723192816_SynchronizeImtsModel'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD CONSTRAINT [FK_AuditLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723192816_SynchronizeImtsModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723192816_SynchronizeImtsModel', N'8.0.8');
END;
GO

COMMIT;
GO


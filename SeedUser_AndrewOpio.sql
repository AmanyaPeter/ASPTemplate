-- ============================================================
-- Seed Dummy User: Andrew Opio (aopio@bou.or.ug)
-- Password: Admin@123
-- ============================================================
-- NOTE: PasswordHash is a real ASP.NET Identity PBKDF2-SHA256
--       hash for "Admin@123". Plain text will NOT work.
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @NewUserId  UNIQUEIDENTIFIER = NEWID();
DECLARE @SecurityStamp  NVARCHAR(MAX) = UPPER(CONVERT(NVARCHAR(36), NEWID()));
DECLARE @ConcurrencyStamp NVARCHAR(MAX) = LOWER(CONVERT(NVARCHAR(36), NEWID()));

-- Step 1: Insert the user row
INSERT INTO AspNetUsers (
    -- === ASP.NET Identity Standard Columns ===
    Id,
    UserName,
    NormalizedUserName,
    Email,
    NormalizedEmail,
    EmailConfirmed,
    PasswordHash,
    SecurityStamp,
    ConcurrencyStamp,
    PhoneNumber,
    PhoneNumberConfirmed,
    TwoFactorEnabled,
    LockoutEnd,
    LockoutEnabled,
    AccessFailedCount,

    -- === ApplicationUser Custom Columns ===
    FullName,
    FirstName,
    MiddleName,
    LastName,
    Title,
    BusinessUnit,
    JobTitle,
    Station,
    AgeBracket,
    Gender,
    RoleId,
    IsActive,
    LockReason,
    IsLoggedIn,
    LastActivity,
    DisableDate,
    EndDate,
    LastLoginDate,
    PasswordResetRequired,
    CreatedDate,
    CreatedBy,
    UpdatedDate,
    UpdatedBy
)
VALUES (
    @NewUserId,
    'aopio@bou.or.ug',
    'AOPIO@BOU.OR.UG',                          -- NormalizedUserName MUST be UPPERCASE
    'aopio@bou.or.ug',
    'AOPIO@BOU.OR.UG',                          -- NormalizedEmail MUST be UPPERCASE
    1,                                          -- EmailConfirmed = true
    -- Real ASP.NET Identity PBKDF2-SHA256 hash for: Admin@123
    'AQAAAAEAACcQAAAAENqZuHAs+SY02TSef8jvhELhHKlPQ4J6SLRrA3oX+B2YcOPOJ4y6ZVHtoLmbuS+UwA==',
    @SecurityStamp,
    @ConcurrencyStamp,
    NULL,                                       -- PhoneNumber
    0,                                          -- PhoneNumberConfirmed
    0,                                          -- TwoFactorEnabled
    NULL,                                       -- LockoutEnd
    1,                                          -- LockoutEnabled
    0,                                          -- AccessFailedCount
    'Andrew Opio',                              -- FullName
    'Andrew',                                   -- FirstName
    NULL,                                       -- MiddleName
    'Opio',                                     -- LastName
    'Mr',                                       -- Title
    'IT',                                       -- BusinessUnit
    'Software Engineer',                        -- JobTitle
    'Head Office',                              -- Station
    '25-34',                                    -- AgeBracket
    'Male',                                     -- Gender
    NULL,                                       -- RoleId (custom Roles table FK, int) - assign below
    1,                                          -- IsActive
    NULL,                                       -- LockReason
    0,                                          -- IsLoggedIn
    GETUTCDATE(),                               -- LastActivity
    NULL,                                       -- DisableDate
    NULL,                                       -- EndDate
    NULL,                                       -- LastLoginDate
    0,                                          -- PasswordResetRequired
    GETUTCDATE(),                               -- CreatedDate
    @NewUserId,                                 -- CreatedBy
    NULL,                                       -- UpdatedDate
    NULL                                        -- UpdatedBy
);

-- Step 2: Assign the user to the 'Staff' AspNetRole (Identity roles table)
-- Run this block ONLY after confirming AspNetRoles contains the role.
-- The DbInitializer seeds: Admin, Staff, InnovationTeam

DECLARE @StaffRoleId UNIQUEIDENTIFIER;
SELECT @StaffRoleId = Id FROM AspNetRoles WHERE NormalizedName = 'STAFF';

IF @StaffRoleId IS NOT NULL
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@NewUserId, @StaffRoleId);
    PRINT 'User assigned to Staff role.';
END
ELSE
BEGIN
    PRINT 'WARNING: Staff role not found. Run the app once so DbInitializer seeds roles, then re-run this block.';
END

-- Step 3: Confirm the insert
SELECT
    Id,
    UserName,
    Email,
    FullName,
    JobTitle,
    Station,
    IsActive
FROM AspNetUsers
WHERE Email = 'aopio@bou.or.ug';

PRINT 'Done. Login with: aopio@bou.or.ug / Admin@123';

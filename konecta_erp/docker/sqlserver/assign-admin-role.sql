-- Assign SuperAdmin role to registered admin user
SET QUOTED_IDENTIFIER ON;
GO

USE Konecta_Auth;
GO

DECLARE @AdminUserId NVARCHAR(450);
DECLARE @SystemAdminRoleId NVARCHAR(450);

-- Get the admin user ID
SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = 'admin@konecta.com';

-- Get the System Admin role ID
SELECT @SystemAdminRoleId = Id FROM AspNetRoles WHERE Name = 'System Admin';

-- Check if both exist
IF @AdminUserId IS NULL
BEGIN
    PRINT 'ERROR: Admin user not found. Please register admin@konecta.com first.';
END
ELSE IF @SystemAdminRoleId IS NULL
BEGIN
    PRINT 'ERROR: System Admin role not found. Please run seed-admin.sql first.';
END
ELSE IF EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @AdminUserId AND RoleId = @SystemAdminRoleId)
BEGIN
    PRINT 'System Admin role already assigned to admin user.';
END
ELSE
BEGIN
    -- Assign System Admin role to admin user
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@AdminUserId, @SystemAdminRoleId);
    
    PRINT 'SUCCESS: System Admin role assigned to admin@konecta.com';
END
GO

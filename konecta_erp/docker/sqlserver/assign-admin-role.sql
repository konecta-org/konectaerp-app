-- Assign SuperAdmin role to registered admin user
SET QUOTED_IDENTIFIER ON;
GO

USE Konecta_Auth;
GO

DECLARE @AdminUserId NVARCHAR(450);
DECLARE @SuperAdminRoleId NVARCHAR(450);

-- Get the admin user ID
SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = 'admin@konecta.com';

-- Get the SuperAdmin role ID
SELECT @SuperAdminRoleId = Id FROM AspNetRoles WHERE Name = 'SuperAdmin';

-- Check if both exist
IF @AdminUserId IS NULL
BEGIN
    PRINT 'ERROR: Admin user not found. Please register admin@konecta.com first.';
END
ELSE IF @SuperAdminRoleId IS NULL
BEGIN
    PRINT 'ERROR: SuperAdmin role not found. Please run seed-admin.sql first.';
END
ELSE IF EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @AdminUserId AND RoleId = @SuperAdminRoleId)
BEGIN
    PRINT 'SuperAdmin role already assigned to admin user.';
END
ELSE
BEGIN
    -- Assign SuperAdmin role to admin user
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@AdminUserId, @SuperAdminRoleId);
    
    PRINT 'SUCCESS: SuperAdmin role assigned to admin@konecta.com';
END
GO

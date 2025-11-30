-- Assign SuperAdmin role to admin user in UserManagement database
SET QUOTED_IDENTIFIER ON;
GO

USE Konecta_UserManagement;
GO

-- Get the Auth user ID for admin
DECLARE @AdminAuthUserId NVARCHAR(64);
SELECT TOP 1 @AdminAuthUserId = Id FROM Konecta_Auth.dbo.AspNetUsers WHERE Email = 'admin@konecta.com';

IF @AdminAuthUserId IS NULL
BEGIN
    PRINT 'ERROR: Admin user not found in Auth database. Please ensure admin@konecta.com is registered.';
END
ELSE
BEGIN
    PRINT 'Auth User ID: ' + @AdminAuthUserId;

    -- Get the System Admin role ID
    DECLARE @SuperAdminRoleId INT;
    SELECT @SuperAdminRoleId = Id FROM Roles WHERE Name = 'System Admin';

    IF @SuperAdminRoleId IS NULL
    BEGIN
        PRINT 'ERROR: System Admin role not found. Please run seed-admin.sql first.';
    END
    ELSE
    BEGIN
        -- Check if admin user exists in UserManagement database
        DECLARE @ExistingUserId NVARCHAR(64);
        SELECT @ExistingUserId = Id FROM Users WHERE Email = 'admin@konecta.com';

        -- Delete old user if exists with different ID
        IF @ExistingUserId IS NOT NULL AND @ExistingUserId != @AdminAuthUserId
        BEGIN
            DELETE FROM UserRoles WHERE UserId = @ExistingUserId;
            DELETE FROM Users WHERE Id = @ExistingUserId;
            PRINT 'Deleted old admin user with ID: ' + @ExistingUserId;
        END

        -- Create user with matching Auth ID
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @AdminAuthUserId)
        BEGIN
            INSERT INTO Users (Id, Email, NormalizedEmail, FullName, Status, IsLocked, IsDeleted, CreatedAt)
            VALUES (
                @AdminAuthUserId,
                'admin@konecta.com',
                'ADMIN@KONECTA.COM',
                'System Administrator',
                'Active',
                0,
                0,
                GETUTCDATE()
            );
            
            PRINT 'Admin user created in UserManagement database with ID: ' + @AdminAuthUserId;
        END
        ELSE
        BEGIN
            PRINT 'Admin user already exists in UserManagement database with ID: ' + @AdminAuthUserId;
        END

        -- Assign SuperAdmin role to admin user
        IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AdminAuthUserId AND RoleId = @SuperAdminRoleId)
        BEGIN
            INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
            VALUES (@AdminAuthUserId, @SuperAdminRoleId, GETUTCDATE());
            PRINT 'SuperAdmin role assigned to admin user in UserManagement';
        END
        ELSE
        BEGIN
            PRINT 'SuperAdmin role already assigned';
        END

        DECLARE @PermissionCount INT;
        SELECT @PermissionCount = COUNT(*) FROM RolePermissions WHERE RoleId = @SuperAdminRoleId;

        PRINT '========================================';
        PRINT 'Admin user permissions configured!';
        PRINT 'User ID: ' + @AdminAuthUserId;
        PRINT 'Role: SuperAdmin (with all ' + CAST(@PermissionCount AS NVARCHAR(10)) + ' permissions)';
        PRINT '========================================';
    END
END
GO

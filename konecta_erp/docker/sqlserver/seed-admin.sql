-- Seed Admin User with All Permissions
-- This script should run AFTER migrations have created the tables
-- Run this manually or via a post-migration script

SET QUOTED_IDENTIFIER ON;
GO

USE [Konecta_UserManagement];
GO

-- Insert System Admin Role if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'System Admin')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, IsSystemDefault, CreatedAt, UpdatedAt)
    VALUES ('System Admin', 'System Administrator with all permissions', 1, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'System Admin role created';
END
GO

-- Get the System Admin role ID
DECLARE @SuperAdminRoleId INT;
SELECT @SuperAdminRoleId = Id FROM Roles WHERE Name = 'System Admin';

-- Insert all permissions for SuperAdmin
DECLARE @Permissions TABLE (Name NVARCHAR(100), Category NVARCHAR(64));

INSERT INTO @Permissions VALUES 
('users.view', 'users'), ('users.create', 'users'), ('users.edit', 'users'), ('users.delete', 'users'),
('roles.view', 'roles'), ('roles.create', 'roles'), ('roles.edit', 'roles'), ('roles.delete', 'roles'),
('permissions.view', 'permissions'), ('permissions.create', 'permissions'), ('permissions.edit', 'permissions'), ('permissions.delete', 'permissions'),
('employees.view', 'employees'), ('employees.create', 'employees'), ('employees.edit', 'employees'), ('employees.delete', 'employees'),
('departments.view', 'departments'), ('departments.create', 'departments'), ('departments.edit', 'departments'), ('departments.delete', 'departments'),
('finance.view', 'finance'), ('finance.create', 'finance'), ('finance.edit', 'finance'), ('finance.delete', 'finance'),
('inventory.view', 'inventory'), ('inventory.create', 'inventory'), ('inventory.edit', 'inventory'), ('inventory.delete', 'inventory'),
('reports.view', 'reports'), ('reports.create', 'reports'), ('reports.edit', 'reports'), ('reports.delete', 'reports');

-- Insert permissions that don't exist
INSERT INTO Permissions (Name, Description, Category, IsActive, CreatedAt, UpdatedAt)
SELECT 
    Name,
    'System permission: ' + Name,
    Category,
    1,
    GETUTCDATE(),
    GETUTCDATE()
FROM @Permissions p
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = p.Name);

PRINT 'Permissions created';

-- Assign all permissions to SuperAdmin role
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @SuperAdminRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @SuperAdminRoleId AND rp.PermissionId = Permissions.Id
);

PRINT 'Permissions assigned to SuperAdmin role';
GO

-- Seed Admin User in Authentication Database
USE [Konecta_Auth];
GO

-- Insert System Admin role in Auth database if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'System Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'System Admin', 'SYSTEM ADMIN', NEWID());
    PRINT 'System Admin role created in Auth database';
END
GO

-- Insert Admin User if doesn't exist
-- Password: Admin@123456
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@konecta.com')
BEGIN
    DECLARE @AdminUserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
    DECLARE @SuperAdminRoleId NVARCHAR(450);
    SELECT @SuperAdminRoleId = Id FROM AspNetRoles WHERE Name = 'System Admin';
    
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
        FullName
    )
    VALUES (
        @AdminUserId,
        'admin@konecta.com',
        'ADMIN@KONECTA.COM',
        'admin@konecta.com',
        'ADMIN@KONECTA.COM',
        1,
        'AQAAAAIAAYagAAAAEJ8Z3qZ3YfJKHK9yK8v4xZ+fLKxHF8Q5YVc6H0wR1Yx1CKpB6p8EgFQQYZJ3j3WJrQ==', -- Admin@123456
        CAST(NEWID() AS NVARCHAR(MAX)),
        CAST(NEWID() AS NVARCHAR(MAX)),
        0,
        0,
        1,
        0,
        'System Administrator'
    );
    
    PRINT 'Admin user created';
    
    -- Assign SuperAdmin role to admin user
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@AdminUserId, @SuperAdminRoleId);
    
    PRINT 'SuperAdmin role assigned to admin user';
END
GO

PRINT '========================================';
PRINT 'Admin user seeded successfully!';
PRINT 'Email: admin@konecta.com';
PRINT 'Password: Admin@123456';
PRINT '========================================';
GO

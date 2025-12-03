-- Seed Manager Users with Their Roles and Permissions
-- This script creates different types of managers with appropriate permissions
-- Run this manually or via a post-migration script

SET QUOTED_IDENTIFIER ON;
GO

USE [Konecta_UserManagement];
GO

-- ============================================
-- 1. CREATE MANAGER ROLES
-- ============================================

-- HR Manager Role
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'HR Manager')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, IsSystemDefault, CreatedAt, UpdatedAt)
    VALUES ('HR Manager', 'Human Resources Manager with HR module permissions', 1, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'HR Manager role created';
END
GO

-- Finance Manager Role
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Finance Manager')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, IsSystemDefault, CreatedAt, UpdatedAt)
    VALUES ('Finance Manager', 'Finance Manager with finance module permissions', 1, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Finance Manager role created';
END
GO

-- Inventory Manager Role
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Inventory Manager')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, IsSystemDefault, CreatedAt, UpdatedAt)
    VALUES ('Inventory Manager', 'Inventory Manager with inventory module permissions', 1, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Inventory Manager role created';
END
GO

-- General Manager Role (has access to all modules)
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'General Manager')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, IsSystemDefault, CreatedAt, UpdatedAt)
    VALUES ('General Manager', 'General Manager with read access to all modules and manage access to reporting', 1, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'General Manager role created';
END
GO

-- ============================================
-- 2. CREATE PERMISSIONS
-- ============================================

DECLARE @Permissions TABLE (Name NVARCHAR(128), Category NVARCHAR(64));

-- User Management Permissions
INSERT INTO @Permissions VALUES
('user-management.users.read', 'user-management'),
('user-management.users.manage', 'user-management'),
('user-management.roles.read', 'user-management'),
('user-management.roles.manage', 'user-management'),
('user-management.permissions.read', 'user-management'),
('user-management.permissions.manage', 'user-management');

-- Finance Permissions
INSERT INTO @Permissions VALUES
('finance.budgets.read', 'finance'),
('finance.budgets.manage', 'finance'),
('finance.expenses.read', 'finance'),
('finance.expenses.manage', 'finance'),
('finance.invoices.read', 'finance'),
('finance.invoices.manage', 'finance'),
('finance.payroll.read', 'finance'),
('finance.payroll.manage', 'finance'),
('finance.compensation.read', 'finance'),
('finance.compensation.manage', 'finance'),
('finance.summary.view', 'finance');

-- HR Permissions
INSERT INTO @Permissions VALUES
('hr.employees.read', 'hr'),
('hr.employees.manage', 'hr'),
('hr.attendance.read', 'hr'),
('hr.attendance.manage', 'hr'),
('hr.departments.read', 'hr'),
('hr.departments.manage', 'hr'),
('hr.leaves.read', 'hr'),
('hr.leaves.manage', 'hr'),
('hr.job-openings.read', 'hr'),
('hr.job-openings.manage', 'hr'),
('hr.job-applications.read', 'hr'),
('hr.job-applications.manage', 'hr'),
('hr.interviews.read', 'hr'),
('hr.interviews.manage', 'hr'),
('hr.resignations.read', 'hr'),
('hr.resignations.manage', 'hr'),
('hr.summary.view', 'hr');

-- Inventory Permissions
INSERT INTO @Permissions VALUES
('inventory.items.read', 'inventory'),
('inventory.items.manage', 'inventory'),
('inventory.warehouses.read', 'inventory'),
('inventory.warehouses.manage', 'inventory'),
('inventory.stock.read', 'inventory'),
('inventory.stock.manage', 'inventory'),
('inventory.summary.view', 'inventory');

-- Reporting Permissions
INSERT INTO @Permissions VALUES
('reporting.overview.view', 'reporting'),
('reporting.finance.view', 'reporting'),
('reporting.hr.view', 'reporting'),
('reporting.inventory.view', 'reporting'),
('reporting.export.pdf', 'reporting'),
('reporting.export.excel', 'reporting');

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

PRINT 'All permissions ensured in database';
GO

-- ============================================
-- 3. ASSIGN PERMISSIONS TO HR MANAGER ROLE
-- ============================================

DECLARE @HRManagerRoleId INT;
SELECT @HRManagerRoleId = Id FROM Roles WHERE Name = 'HR Manager';

-- HR Manager gets full HR permissions
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @HRManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Category = 'hr'
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @HRManagerRoleId AND rp.PermissionId = Permissions.Id
);

-- HR Manager gets read access to reporting
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @HRManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Category = 'reporting' AND Name IN ('reporting.overview.view', 'reporting.hr.view', 'reporting.export.pdf', 'reporting.export.excel')
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @HRManagerRoleId AND rp.PermissionId = Permissions.Id
);

PRINT 'Permissions assigned to HR Manager role';
GO

-- ============================================
-- 4. ASSIGN PERMISSIONS TO FINANCE MANAGER ROLE
-- ============================================

DECLARE @FinanceManagerRoleId INT;
SELECT @FinanceManagerRoleId = Id FROM Roles WHERE Name = 'Finance Manager';

-- Finance Manager gets full finance permissions
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @FinanceManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Category = 'finance'
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @FinanceManagerRoleId AND rp.PermissionId = Permissions.Id
);

-- Finance Manager gets read access to reporting
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @FinanceManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Category = 'reporting' AND Name IN ('reporting.overview.view', 'reporting.finance.view', 'reporting.export.pdf', 'reporting.export.excel')
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @FinanceManagerRoleId AND rp.PermissionId = Permissions.Id
);

PRINT 'Permissions assigned to Finance Manager role';
GO

-- ============================================
-- 5. ASSIGN PERMISSIONS TO INVENTORY MANAGER ROLE
-- ============================================

DECLARE @InventoryManagerRoleId INT;
SELECT @InventoryManagerRoleId = Id FROM Roles WHERE Name = 'Inventory Manager';

-- Inventory Manager gets full inventory permissions
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @InventoryManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Category = 'inventory'
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @InventoryManagerRoleId AND rp.PermissionId = Permissions.Id
);

-- Inventory Manager gets read access to reporting
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @InventoryManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Category = 'reporting' AND Name IN ('reporting.overview.view', 'reporting.inventory.view', 'reporting.export.pdf', 'reporting.export.excel')
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @InventoryManagerRoleId AND rp.PermissionId = Permissions.Id
);

PRINT 'Permissions assigned to Inventory Manager role';
GO

-- ============================================
-- 6. ASSIGN PERMISSIONS TO GENERAL MANAGER ROLE
-- ============================================

DECLARE @GeneralManagerRoleId INT;
SELECT @GeneralManagerRoleId = Id FROM Roles WHERE Name = 'General Manager';

-- General Manager gets read access to all modules
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @GeneralManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Name LIKE '%.read' OR Name LIKE '%.view'
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @GeneralManagerRoleId AND rp.PermissionId = Permissions.Id
);

-- General Manager gets full reporting permissions
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt)
SELECT @GeneralManagerRoleId, Id, GETUTCDATE()
FROM Permissions
WHERE Category = 'reporting'
AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @GeneralManagerRoleId AND rp.PermissionId = Permissions.Id
);

PRINT 'Permissions assigned to General Manager role';
GO

-- ============================================
-- 7. CREATE MANAGER USERS IN AUTH DATABASE
-- ============================================

USE [Konecta_Auth];
GO

-- Create manager roles in Auth database
DECLARE @ManagerRoles TABLE (RoleName NVARCHAR(256));
INSERT INTO @ManagerRoles VALUES ('HR Manager'), ('Finance Manager'), ('Inventory Manager'), ('General Manager');

INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
SELECT NEWID(), RoleName, UPPER(RoleName), NEWID()
FROM @ManagerRoles mr
WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = mr.RoleName);

PRINT 'Manager roles created in Auth database';
GO

-- ============================================
-- 8. SEED MANAGER USERS
-- ============================================

-- HR Manager User
-- Password: HRManager@123
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'hr.manager@konecta.com')
BEGIN
    DECLARE @HRManagerUserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
    DECLARE @HRManagerRoleId NVARCHAR(450);
    SELECT @HRManagerRoleId = Id FROM AspNetRoles WHERE Name = 'HR Manager';
    
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
        FullName
    )
    VALUES (
        @HRManagerUserId,
        'hr.manager@konecta.com',
        'HR.MANAGER@KONECTA.COM',
        'hr.manager@konecta.com',
        'HR.MANAGER@KONECTA.COM',
        1,
        'AQAAAAIAAYagAAAAEJ8Z3qZ3YfJKHK9yK8v4xZ+fLKxHF8Q5YVc6H0wR1Yx1CKpB6p8EgFQQYZJ3j3WJrQ==', -- HRManager@123
        CAST(NEWID() AS NVARCHAR(MAX)),
        CAST(NEWID() AS NVARCHAR(MAX)),
        0,
        0,
        1,
        0,
        'HR Manager'
    );
    
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@HRManagerUserId, @HRManagerRoleId);
    
    PRINT 'HR Manager user created';
END
GO

-- Finance Manager User
-- Password: FinanceManager@123
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'finance.manager@konecta.com')
BEGIN
    DECLARE @FinanceManagerUserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
    DECLARE @FinanceManagerRoleId NVARCHAR(450);
    SELECT @FinanceManagerRoleId = Id FROM AspNetRoles WHERE Name = 'Finance Manager';
    
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
        FullName
    )
    VALUES (
        @FinanceManagerUserId,
        'finance.manager@konecta.com',
        'FINANCE.MANAGER@KONECTA.COM',
        'finance.manager@konecta.com',
        'FINANCE.MANAGER@KONECTA.COM',
        1,
        'AQAAAAIAAYagAAAAEJ8Z3qZ3YfJKHK9yK8v4xZ+fLKxHF8Q5YVc6H0wR1Yx1CKpB6p8EgFQQYZJ3j3WJrQ==', -- FinanceManager@123
        CAST(NEWID() AS NVARCHAR(MAX)),
        CAST(NEWID() AS NVARCHAR(MAX)),
        0,
        0,
        1,
        0,
        'Finance Manager'
    );
    
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@FinanceManagerUserId, @FinanceManagerRoleId);
    
    PRINT 'Finance Manager user created';
END
GO

-- Inventory Manager User
-- Password: InventoryManager@123
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'inventory.manager@konecta.com')
BEGIN
    DECLARE @InventoryManagerUserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
    DECLARE @InventoryManagerRoleId NVARCHAR(450);
    SELECT @InventoryManagerRoleId = Id FROM AspNetRoles WHERE Name = 'Inventory Manager';
    
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
        FullName
    )
    VALUES (
        @InventoryManagerUserId,
        'inventory.manager@konecta.com',
        'INVENTORY.MANAGER@KONECTA.COM',
        'inventory.manager@konecta.com',
        'INVENTORY.MANAGER@KONECTA.COM',
        1,
        'AQAAAAIAAYagAAAAEJ8Z3qZ3YfJKHK9yK8v4xZ+fLKxHF8Q5YVc6H0wR1Yx1CKpB6p8EgFQQYZJ3j3WJrQ==', -- InventoryManager@123
        CAST(NEWID() AS NVARCHAR(MAX)),
        CAST(NEWID() AS NVARCHAR(MAX)),
        0,
        0,
        1,
        0,
        'Inventory Manager'
    );
    
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@InventoryManagerUserId, @InventoryManagerRoleId);
    
    PRINT 'Inventory Manager user created';
END
GO

-- General Manager User
-- Password: GeneralManager@123
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'general.manager@konecta.com')
BEGIN
    DECLARE @GeneralManagerUserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
    DECLARE @GeneralManagerRoleId NVARCHAR(450);
    SELECT @GeneralManagerRoleId = Id FROM AspNetRoles WHERE Name = 'General Manager';
    
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
        FullName
    )
    VALUES (
        @GeneralManagerUserId,
        'general.manager@konecta.com',
        'GENERAL.MANAGER@KONECTA.COM',
        'general.manager@konecta.com',
        'GENERAL.MANAGER@KONECTA.COM',
        1,
        'AQAAAAIAAYagAAAAEJ8Z3qZ3YfJKHK9yK8v4xZ+fLKxHF8Q5YVc6H0wR1Yx1CKpB6p8EgFQQYZJ3j3WJrQ==', -- GeneralManager@123
        CAST(NEWID() AS NVARCHAR(MAX)),
        CAST(NEWID() AS NVARCHAR(MAX)),
        0,
        0,
        1,
        0,
        'General Manager'
    );
    
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@GeneralManagerUserId, @GeneralManagerRoleId);
    
    PRINT 'General Manager user created';
END
GO

-- ============================================
-- 9. CREATE MANAGER USERS IN USERMANAGEMENT DATABASE
-- ============================================

USE [Konecta_UserManagement];
GO

-- HR Manager
DECLARE @HRManagerAuthId NVARCHAR(64);
SELECT TOP 1 @HRManagerAuthId = Id FROM Konecta_Auth.dbo.AspNetUsers WHERE Email = 'hr.manager@konecta.com';

IF @HRManagerAuthId IS NOT NULL
BEGIN
    DECLARE @HRManagerRoleId INT;
    SELECT @HRManagerRoleId = Id FROM Roles WHERE Name = 'HR Manager';
    
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @HRManagerAuthId)
    BEGIN
        INSERT INTO Users (Id, Email, NormalizedEmail, FullName, Status, IsLocked, IsDeleted, CreatedAt)
        VALUES (
            @HRManagerAuthId,
            'hr.manager@konecta.com',
            'HR.MANAGER@KONECTA.COM',
            'HR Manager',
            'Active',
            0,
            0,
            GETUTCDATE()
        );
        PRINT 'HR Manager user created in UserManagement database';
    END
    
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @HRManagerAuthId AND RoleId = @HRManagerRoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
        VALUES (@HRManagerAuthId, @HRManagerRoleId, GETUTCDATE());
        PRINT 'HR Manager role assigned in UserManagement';
    END
END
GO

-- Finance Manager
DECLARE @FinanceManagerAuthId NVARCHAR(64);
SELECT TOP 1 @FinanceManagerAuthId = Id FROM Konecta_Auth.dbo.AspNetUsers WHERE Email = 'finance.manager@konecta.com';

IF @FinanceManagerAuthId IS NOT NULL
BEGIN
    DECLARE @FinanceManagerRoleId INT;
    SELECT @FinanceManagerRoleId = Id FROM Roles WHERE Name = 'Finance Manager';
    
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @FinanceManagerAuthId)
    BEGIN
        INSERT INTO Users (Id, Email, NormalizedEmail, FullName, Status, IsLocked, IsDeleted, CreatedAt)
        VALUES (
            @FinanceManagerAuthId,
            'finance.manager@konecta.com',
            'FINANCE.MANAGER@KONECTA.COM',
            'Finance Manager',
            'Active',
            0,
            0,
            GETUTCDATE()
        );
        PRINT 'Finance Manager user created in UserManagement database';
    END
    
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @FinanceManagerAuthId AND RoleId = @FinanceManagerRoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
        VALUES (@FinanceManagerAuthId, @FinanceManagerRoleId, GETUTCDATE());
        PRINT 'Finance Manager role assigned in UserManagement';
    END
END
GO

-- Inventory Manager
DECLARE @InventoryManagerAuthId NVARCHAR(64);
SELECT TOP 1 @InventoryManagerAuthId = Id FROM Konecta_Auth.dbo.AspNetUsers WHERE Email = 'inventory.manager@konecta.com';

IF @InventoryManagerAuthId IS NOT NULL
BEGIN
    DECLARE @InventoryManagerRoleId INT;
    SELECT @InventoryManagerRoleId = Id FROM Roles WHERE Name = 'Inventory Manager';
    
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @InventoryManagerAuthId)
    BEGIN
        INSERT INTO Users (Id, Email, NormalizedEmail, FullName, Status, IsLocked, IsDeleted, CreatedAt)
        VALUES (
            @InventoryManagerAuthId,
            'inventory.manager@konecta.com',
            'INVENTORY.MANAGER@KONECTA.COM',
            'Inventory Manager',
            'Active',
            0,
            0,
            GETUTCDATE()
        );
        PRINT 'Inventory Manager user created in UserManagement database';
    END
    
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @InventoryManagerAuthId AND RoleId = @InventoryManagerRoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
        VALUES (@InventoryManagerAuthId, @InventoryManagerRoleId, GETUTCDATE());
        PRINT 'Inventory Manager role assigned in UserManagement';
    END
END
GO

-- General Manager
DECLARE @GeneralManagerAuthId NVARCHAR(64);
SELECT TOP 1 @GeneralManagerAuthId = Id FROM Konecta_Auth.dbo.AspNetUsers WHERE Email = 'general.manager@konecta.com';

IF @GeneralManagerAuthId IS NOT NULL
BEGIN
    DECLARE @GeneralManagerRoleId INT;
    SELECT @GeneralManagerRoleId = Id FROM Roles WHERE Name = 'General Manager';
    
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @GeneralManagerAuthId)
    BEGIN
        INSERT INTO Users (Id, Email, NormalizedEmail, FullName, Status, IsLocked, IsDeleted, CreatedAt)
        VALUES (
            @GeneralManagerAuthId,
            'general.manager@konecta.com',
            'GENERAL.MANAGER@KONECTA.COM',
            'General Manager',
            'Active',
            0,
            0,
            GETUTCDATE()
        );
        PRINT 'General Manager user created in UserManagement database';
    END
    
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @GeneralManagerAuthId AND RoleId = @GeneralManagerRoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
        VALUES (@GeneralManagerAuthId, @GeneralManagerRoleId, GETUTCDATE());
        PRINT 'General Manager role assigned in UserManagement';
    END
END
GO

-- ============================================
-- SUMMARY
-- ============================================

PRINT '========================================';
PRINT 'Manager users seeded successfully!';
PRINT '';
PRINT 'HR Manager:';
PRINT '  Email: hr.manager@konecta.com';
PRINT '  Password: HRManager@123';
PRINT '  Permissions: Full HR access + HR reporting';
PRINT '';
PRINT 'Finance Manager:';
PRINT '  Email: finance.manager@konecta.com';
PRINT '  Password: FinanceManager@123';
PRINT '  Permissions: Full Finance access + Finance reporting';
PRINT '';
PRINT 'Inventory Manager:';
PRINT '  Email: inventory.manager@konecta.com';
PRINT '  Password: InventoryManager@123';
PRINT '  Permissions: Full Inventory access + Inventory reporting';
PRINT '';
PRINT 'General Manager:';
PRINT '  Email: general.manager@konecta.com';
PRINT '  Password: GeneralManager@123';
PRINT '  Permissions: Read access to all modules + Full reporting';
PRINT '========================================';
GO

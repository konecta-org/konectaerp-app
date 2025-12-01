-- Test Accounts Creation Script
-- This script creates test users with different roles for testing the application

USE [Konecta_Auth];
GO

-- Create test users if they don't exist
-- Password for all test accounts: Test@123

-- 2. HR Manager
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'hr@konecta.com')
BEGIN
    INSERT INTO AspNetUsers (Id, Email, FullName, PasswordHash, Department, JobTitle, Status, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        NEWID(),
        'hr@konecta.com',
        'HR Manager',
        '$2a$11$8K1p/a0dL3.HNPqAvqfBPOJ5xeTNfANLFFVtNhsjTRKp50C1LBqyW', -- Test@123
        'Human Resources',
        'HR Manager',
        'Active',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'HR Manager created: hr@konecta.com';
END
ELSE
BEGIN
    PRINT 'HR Manager already exists: hr@konecta.com';
END
GO

-- 3. Finance Manager
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'finance@konecta.com')
BEGIN
    INSERT INTO AspNetUsers (Id, Email, FullName, PasswordHash, Department, JobTitle, Status, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        NEWID(),
        'finance@konecta.com',
        'Finance Manager',
        '$2a$11$8K1p/a0dL3.HNPqAvqfBPOJ5xeTNfANLFFVtNhsjTRKp50C1LBqyW', -- Test@123
        'Finance',
        'Finance Manager',
        'Active',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'Finance Manager created: finance@konecta.com';
END
ELSE
BEGIN
    PRINT 'Finance Manager already exists: finance@konecta.com';
END
GO

-- 4. Inventory Manager
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'inventory@konecta.com')
BEGIN
    INSERT INTO AspNetUsers (Id, Email, FullName, PasswordHash, Department, JobTitle, Status, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        NEWID(),
        'inventory@konecta.com',
        'Inventory Manager',
        '$2a$11$8K1p/a0dL3.HNPqAvqfBPOJ5xeTNfANLFFVtNhsjTRKp50C1LBqyW', -- Test@123
        'Warehouse',
        'Inventory Manager',
        'Active',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'Inventory Manager created: inventory@konecta.com';
END
ELSE
BEGIN
    PRINT 'Inventory Manager already exists: inventory@konecta.com';
END
GO

-- 5. Regular Employee
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'employee@konecta.com')
BEGIN
    INSERT INTO AspNetUsers (Id, Email, FullName, PasswordHash, Department, JobTitle, Status, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (
        NEWID(),
        'employee@konecta.com',
        'Regular Employee',
        '$2a$11$8K1p/a0dL3.HNPqAvqfBPOJ5xeTNfANLFFVtNhsjTRKp50C1LBqyW', -- Test@123
        'Sales',
        'Sales Representative',
        'Active',
        GETUTCDATE(),
        GETUTCDATE(),
        0
    );
    PRINT 'Regular Employee created: employee@konecta.com';
END
ELSE
BEGIN
    PRINT 'Regular Employee already exists: employee@konecta.com';
END
GO

-- Display all test accounts
SELECT 
    Email,
    FullName,
    Department,
    JobTitle,
    Status,
    CreatedAt
FROM Users
WHERE Email LIKE '%@konecta.com'
ORDER BY Email;
GO

PRINT '';
PRINT '========================================';
PRINT 'Test Accounts Summary';
PRINT '========================================';
PRINT 'All test accounts use password: Test@123';
PRINT '';
PRINT '1. hr@konecta.com - HR Manager';
PRINT '2. finance@konecta.com - Finance Manager';
PRINT '3. inventory@konecta.com - Inventory Manager';
PRINT '4. employee@konecta.com - Regular Employee';
PRINT '========================================';
GO

-- Delete existing admin user and recreate via registration
SET QUOTED_IDENTIFIER ON;
GO

USE Konecta_Auth;
GO

-- Delete existing admin user
DECLARE @AdminUserId NVARCHAR(450);
SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = 'admin@konecta.com';

IF @AdminUserId IS NOT NULL
BEGIN
    DELETE FROM AspNetUserRoles WHERE UserId = @AdminUserId;
    DELETE FROM AspNetUsers WHERE Id = @AdminUserId;
    PRINT 'Existing admin user deleted. Please register again via Swagger.';
END
ELSE
BEGIN
    PRINT 'No existing admin user found.';
END
GO

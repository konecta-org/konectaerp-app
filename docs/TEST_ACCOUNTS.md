# Test Accounts

This document lists all test accounts available for testing the KonectaERP application.

## Available Test Accounts

All test accounts use the same password: **Test@123**

### 1. System Administrator

- **Email**: admin@test.com
- **Password**: Test@123
- **Department**: IT
- **Job Title**: System Administrator
- **Access**: Full system access

### 2. HR Manager

- **Email**: hr@test.com
- **Password**: Test@123
- **Department**: Human Resources
- **Job Title**: HR Manager
- **Access**: HR module access

### 3. Finance Manager

- **Email**: finance@test.com
- **Password**: Test@123
- **Department**: Finance
- **Job Title**: Finance Manager
- **Access**: Finance module access

### 4. Inventory Manager

- **Email**: inventory@test.com
- **Password**: Test@123
- **Department**: Warehouse
- **Job Title**: Inventory Manager
- **Access**: Inventory module access

### 5. Regular Employee

- **Email**: employee@test.com
- **Password**: Test@123
- **Department**: Sales
- **Job Title**: Sales Representative
- **Access**: Basic employee access

## Creating Test Accounts

### Option 1: Using the Shell Script (Recommended)

```bash
./scripts/create-test-accounts.sh
```

### Option 2: Manual SQL Execution

```bash
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "pa55w0rd!" \
  -i /scripts/create-test-accounts.sql
```

### Option 3: Using Docker Compose Exec

```bash
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "pa55w0rd!" \
  -i /scripts/create-test-accounts.sql
```

## Login Flow

1. Navigate to `http://localhost:4200/auth/login`
2. Enter one of the test account emails
3. Enter password: `Test@123`
4. Click "Sign In"
5. You will be redirected to `/dashboard` upon successful login

## Default Routes

- **Login Page**: `/auth/login`
- **Register Page**: `/auth/register`
- **Dashboard** (after login): `/dashboard`
- **Access Denied**: `/access-denied`

## Testing Different Modules

After logging in, you can test the following routes:

- **Dashboard**: `/dashboard` (accessible to all authenticated users)
- **User Management**: `/users` (requires permissions)
- **HR Employees**: `/hr/employees` (accessible for testing)
- **Finance**: `/finance` (accessible for testing)
- **Inventory**: `/inventory` (accessible for testing)
- **Reporting**: `/reporting` (accessible for testing)

## Notes

- Permission guards have been temporarily removed from most routes for testing purposes
- The dashboard is accessible to all authenticated users
- All test accounts are created with "Active" status
- Test accounts are not deleted by default (IsDeleted = 0)

## Troubleshooting

### Cannot Login

1. Ensure the backend services are running:

   ```bash
   docker compose ps
   ```

2. Check if the test accounts exist:

   ```bash
   docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
     -S localhost \
     -U sa \
     -P "pa55w0rd!" \
     -Q "USE KonectaERP; SELECT Email, FullName, Status FROM Users WHERE Email LIKE '%@test.com';"
   ```

3. Verify the AuthenticationService is running:
   ```bash
   docker compose logs authentication-service
   ```

### Password Not Working

The password hash in the database is: `$2a$11$8K1p/a0dL3.HNPqAvqfBPOJ5xeTNfANLFFVtNhsjTRKp50C1LBqyW`

This corresponds to: `Test@123`

If the password doesn't work, the backend might be using a different hashing algorithm. Check the AuthenticationService logs.

## Security Notice

⚠️ **These are test accounts for development only!**

- Never use these accounts in production
- Change all passwords before deploying to production
- Remove or disable test accounts in production environments

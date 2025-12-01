#!/bin/bash

# Script to create test accounts in the database
# This script connects to SQL Server and runs the test account creation script

echo "Creating test accounts in KonectaERP database..."
echo "=============================================="
echo ""

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
sleep 5

# Run the SQL script
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "pa55w0rd!" \
  -i /scripts/create-test-accounts.sql

echo ""
echo "=============================================="
echo "Test accounts creation completed!"
echo ""
echo "You can now login with any of these accounts:"
echo "  - admin@test.com (System Administrator)"
echo "  - hr@test.com (HR Manager)"
echo "  - finance@test.com (Finance Manager)"
echo "  - inventory@test.com (Inventory Manager)"
echo "  - employee@test.com (Regular Employee)"
echo ""
echo "Password for all accounts: Test@123"
echo "=============================================="

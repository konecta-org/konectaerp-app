-- =============================================
-- Finance Service Seed Data
-- Run this script to populate the Finance database with sample data
-- =============================================

USE Konecta_Finance;
GO

-- =============================================
-- BUDGETS
-- =============================================
PRINT 'Seeding Budgets...';

-- Clear existing data (optional - comment out if you want to append)
DELETE FROM BudgetLines;
DELETE FROM Budgets;

-- Insert Budgets
SET IDENTITY_INSERT Budgets ON;

INSERT INTO Budgets (Id, Name, Department, FiscalYear, StartDate, EndDate, TotalAmount, SpentAmount, Notes)
VALUES 
(1, 'IT Infrastructure Budget', 'IT', 2025, '2025-01-01', '2025-12-31', 500000.00, 187500.00, 'Annual IT infrastructure and equipment budget'),
(2, 'Marketing Campaign Budget', 'Marketing', 2025, '2025-01-01', '2025-12-31', 250000.00, 95000.00, 'Digital and traditional marketing campaigns'),
(3, 'Human Resources Budget', 'HR', 2025, '2025-01-01', '2025-12-31', 150000.00, 62000.00, 'Training, recruitment, and employee programs'),
(4, 'Operations Budget', 'Operations', 2025, '2025-01-01', '2025-12-31', 800000.00, 412000.00, 'Day-to-day operational expenses'),
(5, 'Research & Development', 'R&D', 2025, '2025-01-01', '2025-12-31', 350000.00, 125000.00, 'Product development and innovation'),
(6, 'Sales Department Budget', 'Sales', 2025, '2025-01-01', '2025-12-31', 200000.00, 78000.00, 'Sales team expenses and commissions'),
(7, 'Facilities Management', 'Facilities', 2025, '2025-01-01', '2025-12-31', 175000.00, 98000.00, 'Building maintenance and utilities'),
(8, 'Q1 Special Projects', 'Executive', 2025, '2025-01-01', '2025-03-31', 100000.00, 100000.00, 'Q1 strategic initiatives - COMPLETED');

SET IDENTITY_INSERT Budgets OFF;

-- Insert Budget Lines
SET IDENTITY_INSERT BudgetLines ON;

INSERT INTO BudgetLines (Id, BudgetId, Category, AllocatedAmount, SpentAmount, Notes)
VALUES 
-- IT Infrastructure Budget Lines
(1, 1, 'Hardware', 200000.00, 85000.00, 'Servers, workstations, networking equipment'),
(2, 1, 'Software Licenses', 150000.00, 62500.00, 'Enterprise software and SaaS subscriptions'),
(3, 1, 'Cloud Services', 100000.00, 30000.00, 'AWS, Azure, and GCP costs'),
(4, 1, 'Maintenance & Support', 50000.00, 10000.00, 'Vendor support contracts'),

-- Marketing Campaign Budget Lines
(5, 2, 'Digital Advertising', 100000.00, 45000.00, 'Google Ads, Facebook, LinkedIn'),
(6, 2, 'Content Creation', 50000.00, 22000.00, 'Video production, graphics, copywriting'),
(7, 2, 'Events & Conferences', 60000.00, 18000.00, 'Trade shows and sponsorships'),
(8, 2, 'Print & Traditional', 40000.00, 10000.00, 'Brochures, billboards, radio'),

-- HR Budget Lines
(9, 3, 'Training Programs', 60000.00, 28000.00, 'Employee skill development'),
(10, 3, 'Recruitment', 50000.00, 22000.00, 'Job postings, agencies, referral bonuses'),
(11, 3, 'Employee Wellness', 40000.00, 12000.00, 'Health programs and team building'),

-- Operations Budget Lines
(12, 4, 'Logistics', 350000.00, 185000.00, 'Shipping and transportation'),
(13, 4, 'Inventory Management', 250000.00, 127000.00, 'Warehouse and stock management'),
(14, 4, 'Quality Assurance', 200000.00, 100000.00, 'Testing and compliance');

SET IDENTITY_INSERT BudgetLines OFF;

-- =============================================
-- EXPENSES
-- =============================================
PRINT 'Seeding Expenses...';

DELETE FROM Expenses;

SET IDENTITY_INSERT Expenses ON;

INSERT INTO Expenses (Id, ExpenseNumber, Category, Vendor, Description, IncurredOn, Status, PaymentMethod, Amount, Notes)
VALUES 
(1, 'EXP-2025-0001', 'Technology', 'Dell Technologies', 'Server hardware for data center expansion', '2025-01-15', 'Approved', 'BankTransfer', 45000.00, 'Part of IT infrastructure upgrade'),
(2, 'EXP-2025-0002', 'Marketing', 'Google LLC', 'Q1 Google Ads campaign', '2025-01-20', 'Approved', 'CreditCard', 15000.00, 'PPC advertising for product launch'),
(3, 'EXP-2025-0003', 'Travel', 'United Airlines', 'Executive team travel to NYC conference', '2025-02-05', 'Approved', 'CreditCard', 8500.00, 'Annual industry conference'),
(4, 'EXP-2025-0004', 'Office Supplies', 'Staples', 'Monthly office supplies replenishment', '2025-02-10', 'Approved', 'PurchaseOrder', 1250.00, 'Standard monthly order'),
(5, 'EXP-2025-0005', 'Software', 'Microsoft', 'Microsoft 365 annual subscription', '2025-02-15', 'Approved', 'BankTransfer', 35000.00, 'Enterprise license renewal'),
(6, 'EXP-2025-0006', 'Consulting', 'McKinsey & Company', 'Strategic consulting engagement', '2025-03-01', 'Approved', 'BankTransfer', 75000.00, 'Digital transformation advisory'),
(7, 'EXP-2025-0007', 'Utilities', 'City Power & Light', 'Q1 electricity bill - HQ', '2025-03-10', 'Approved', 'BankTransfer', 12800.00, 'Quarterly utility payment'),
(8, 'EXP-2025-0008', 'Training', 'Coursera Business', 'Employee learning platform subscription', '2025-03-15', 'Approved', 'CreditCard', 8500.00, 'Annual enterprise plan'),
(9, 'EXP-2025-0009', 'Equipment', 'Apple Inc', 'MacBook Pro laptops for development team', '2025-03-20', 'Approved', 'BankTransfer', 28000.00, '10 units for new hires'),
(10, 'EXP-2025-0010', 'Marketing', 'Facebook/Meta', 'Social media advertising campaign', '2025-04-01', 'Approved', 'CreditCard', 12000.00, 'Brand awareness campaign'),
(11, 'EXP-2025-0011', 'Maintenance', 'BuildingCare Inc', 'HVAC system maintenance', '2025-04-10', 'Approved', 'PurchaseOrder', 4500.00, 'Quarterly maintenance contract'),
(12, 'EXP-2025-0012', 'Legal', 'Baker & Associates', 'Contract review and legal consultation', '2025-04-15', 'Approved', 'BankTransfer', 15000.00, 'Vendor contract negotiations'),
(13, 'EXP-2025-0013', 'Cloud Services', 'Amazon Web Services', 'AWS monthly infrastructure costs', '2025-05-01', 'Pending', 'BankTransfer', 22000.00, 'Production environment hosting'),
(14, 'EXP-2025-0014', 'Recruitment', 'LinkedIn', 'LinkedIn Recruiter annual license', '2025-05-10', 'Pending', 'CreditCard', 9600.00, 'Talent acquisition tools'),
(15, 'EXP-2025-0015', 'Insurance', 'Allianz Global', 'Corporate liability insurance premium', '2025-05-15', 'Pending', 'BankTransfer', 45000.00, 'Annual premium payment'),
(16, 'EXP-2025-0016', 'Events', 'Marriott Hotels', 'Company annual retreat venue booking', '2025-06-01', 'Draft', 'PurchaseOrder', 35000.00, 'July 2025 retreat - 150 attendees'),
(17, 'EXP-2025-0017', 'Technology', 'Cisco Systems', 'Network switches and routers', '2025-06-05', 'Draft', 'BankTransfer', 18000.00, 'Network infrastructure upgrade'),
(18, 'EXP-2025-0018', 'Furniture', 'Herman Miller', 'Ergonomic chairs for new office floor', '2025-06-10', 'Rejected', 'PurchaseOrder', 42000.00, 'Postponed to next quarter');

SET IDENTITY_INSERT Expenses OFF;

-- =============================================
-- INVOICES
-- =============================================
PRINT 'Seeding Invoices...';

DELETE FROM InvoiceLines;
DELETE FROM Invoices;

SET IDENTITY_INSERT Invoices ON;

INSERT INTO Invoices (Id, InvoiceNumber, CustomerName, CustomerEmail, CustomerContact, IssueDate, DueDate, Status, Subtotal, TaxAmount, TotalAmount, PaidAmount, Currency, Notes)
VALUES 
(1, 'INV-2025-0001', 'Acme Corporation', 'billing@acme.com', 'John Smith', '2025-01-05', '2025-02-04', 'Paid', 50000.00, 5000.00, 55000.00, 55000.00, 'USD', 'Enterprise software license'),
(2, 'INV-2025-0002', 'Global Tech Solutions', 'accounts@globaltech.com', 'Sarah Johnson', '2025-01-15', '2025-02-14', 'Paid', 75000.00, 7500.00, 82500.00, 82500.00, 'USD', 'Consulting services Q1'),
(3, 'INV-2025-0003', 'Sunrise Industries', 'finance@sunrise.com', 'Mike Davis', '2025-02-01', '2025-03-02', 'Paid', 32000.00, 3200.00, 35200.00, 35200.00, 'USD', 'Custom development project'),
(4, 'INV-2025-0004', 'Pacific Traders Ltd', 'ap@pacifictraders.com', 'Lisa Wong', '2025-02-15', '2025-03-16', 'Paid', 18500.00, 1850.00, 20350.00, 20350.00, 'USD', 'Integration services'),
(5, 'INV-2025-0005', 'Nordic Enterprises', 'invoices@nordic.com', 'Erik Larsson', '2025-03-01', '2025-03-31', 'Paid', 95000.00, 9500.00, 104500.00, 104500.00, 'USD', 'Annual support contract'),
(6, 'INV-2025-0006', 'Metro Systems Inc', 'billing@metrosys.com', 'Amanda Clark', '2025-03-15', '2025-04-14', 'Overdue', 42000.00, 4200.00, 46200.00, 0.00, 'USD', 'Hardware procurement'),
(7, 'INV-2025-0007', 'Quantum Dynamics', 'accounts@quantum.com', 'David Kim', '2025-04-01', '2025-04-30', 'Overdue', 28000.00, 2800.00, 30800.00, 15000.00, 'USD', 'Training program delivery'),
(8, 'INV-2025-0008', 'Atlas Manufacturing', 'finance@atlas.com', 'Robert Brown', '2025-04-15', '2025-05-15', 'Sent', 65000.00, 6500.00, 71500.00, 0.00, 'USD', 'ERP implementation Phase 1'),
(9, 'INV-2025-0009', 'Horizon Healthcare', 'ap@horizon.com', 'Jennifer Lee', '2025-05-01', '2025-05-31', 'Sent', 120000.00, 12000.00, 132000.00, 0.00, 'USD', 'Healthcare compliance module'),
(10, 'INV-2025-0010', 'Stellar Logistics', 'billing@stellar.com', 'Thomas Wilson', '2025-05-15', '2025-06-14', 'Sent', 38000.00, 3800.00, 41800.00, 0.00, 'USD', 'Logistics optimization project'),
(11, 'INV-2025-0011', 'Pinnacle Financial', 'invoices@pinnacle.com', 'Michelle Taylor', '2025-06-01', '2025-06-30', 'Draft', 85000.00, 8500.00, 93500.00, 0.00, 'USD', 'Financial reporting system'),
(12, 'INV-2025-0012', 'Evergreen Solutions', 'accounts@evergreen.com', 'Chris Anderson', '2025-06-10', '2025-07-10', 'Draft', 22000.00, 2200.00, 24200.00, 0.00, 'USD', 'Cloud migration assistance');

SET IDENTITY_INSERT Invoices OFF;

-- Insert Invoice Lines
SET IDENTITY_INSERT InvoiceLines ON;

INSERT INTO InvoiceLines (Id, InvoiceId, ItemCode, Description, Quantity, UnitPrice, LineTotal)
VALUES 
-- Invoice 1 - Acme Corporation
(1, 1, 'LIC-ENT-001', 'Enterprise Software License - Annual', 1.00, 40000.00, 40000.00),
(2, 1, 'SUP-PRE-001', 'Premium Support Package', 1.00, 10000.00, 10000.00),

-- Invoice 2 - Global Tech Solutions
(3, 2, 'CONS-SR-001', 'Senior Consultant - 200 hours', 200.00, 250.00, 50000.00),
(4, 2, 'CONS-JR-001', 'Junior Consultant - 100 hours', 100.00, 150.00, 15000.00),
(5, 2, 'TRV-EXP-001', 'Travel and Expenses', 1.00, 10000.00, 10000.00),

-- Invoice 3 - Sunrise Industries
(6, 3, 'DEV-CUST-001', 'Custom Development - Phase 1', 1.00, 25000.00, 25000.00),
(7, 3, 'TEST-QA-001', 'Quality Assurance Testing', 40.00, 175.00, 7000.00),

-- Invoice 5 - Nordic Enterprises
(8, 5, 'SUP-ANN-001', 'Annual Support Contract - Gold', 1.00, 75000.00, 75000.00),
(9, 5, 'UPG-VER-001', 'Version Upgrades (included)', 1.00, 20000.00, 20000.00),

-- Invoice 8 - Atlas Manufacturing
(10, 8, 'ERP-IMP-001', 'ERP Implementation Services', 1.00, 45000.00, 45000.00),
(11, 8, 'TRN-USR-001', 'End User Training (20 users)', 20.00, 500.00, 10000.00),
(12, 8, 'DOC-TEC-001', 'Technical Documentation', 1.00, 10000.00, 10000.00),

-- Invoice 9 - Horizon Healthcare
(13, 9, 'MOD-HLC-001', 'Healthcare Compliance Module', 1.00, 80000.00, 80000.00),
(14, 9, 'INT-HL7-001', 'HL7 Integration Development', 1.00, 25000.00, 25000.00),
(15, 9, 'CERT-HIP-001', 'HIPAA Certification Support', 1.00, 15000.00, 15000.00);

SET IDENTITY_INSERT InvoiceLines OFF;

-- =============================================
-- PAYROLL RUNS
-- =============================================
PRINT 'Seeding Payroll Runs...';

DELETE FROM PayrollEntries;
DELETE FROM PayrollRuns;

SET IDENTITY_INSERT PayrollRuns ON;

INSERT INTO PayrollRuns (Id, PayrollNumber, PeriodStart, PeriodEnd, PaymentDate, Status, TotalGrossPay, TotalNetPay, Notes)
VALUES 
(1, 'PR-2025-01', '2025-01-01', '2025-01-15', '2025-01-17', 'Completed', 185000.00, 142450.00, 'January 1-15 payroll'),
(2, 'PR-2025-02', '2025-01-16', '2025-01-31', '2025-02-01', 'Completed', 185000.00, 142450.00, 'January 16-31 payroll'),
(3, 'PR-2025-03', '2025-02-01', '2025-02-15', '2025-02-17', 'Completed', 192000.00, 147840.00, 'February 1-15 payroll - includes new hires'),
(4, 'PR-2025-04', '2025-02-16', '2025-02-28', '2025-03-01', 'Completed', 192000.00, 147840.00, 'February 16-28 payroll'),
(5, 'PR-2025-05', '2025-03-01', '2025-03-15', '2025-03-17', 'Completed', 195000.00, 150150.00, 'March 1-15 payroll'),
(6, 'PR-2025-06', '2025-03-16', '2025-03-31', '2025-04-01', 'Completed', 195000.00, 150150.00, 'March 16-31 payroll'),
(7, 'PR-2025-07', '2025-04-01', '2025-04-15', '2025-04-17', 'Completed', 198000.00, 152460.00, 'April 1-15 payroll'),
(8, 'PR-2025-08', '2025-04-16', '2025-04-30', '2025-05-01', 'Completed', 198000.00, 152460.00, 'April 16-30 payroll'),
(9, 'PR-2025-09', '2025-05-01', '2025-05-15', '2025-05-17', 'Completed', 201000.00, 154770.00, 'May 1-15 payroll'),
(10, 'PR-2025-10', '2025-05-16', '2025-05-31', '2025-06-01', 'Processing', 201000.00, 154770.00, 'May 16-31 payroll - in progress'),
(11, 'PR-2025-11', '2025-06-01', '2025-06-15', '2025-06-17', 'Draft', 205000.00, 157850.00, 'June 1-15 payroll - pending approval');

SET IDENTITY_INSERT PayrollRuns OFF;

-- Insert Payroll Entries (sample employees for recent payrolls)
SET IDENTITY_INSERT PayrollEntries ON;

INSERT INTO PayrollEntries (Id, PayrollRunId, EmployeeId, EmployeeName, GrossPay, NetPay, Deductions, Taxes, Notes)
VALUES 
-- Payroll Run 9 (May 1-15)
(1, 9, 'EMP001', 'James Anderson', 12000.00, 9240.00, 1200.00, 1560.00, 'Senior Software Engineer'),
(2, 9, 'EMP002', 'Maria Garcia', 10500.00, 8085.00, 1050.00, 1365.00, 'Product Manager'),
(3, 9, 'EMP003', 'Robert Chen', 11000.00, 8470.00, 1100.00, 1430.00, 'DevOps Engineer'),
(4, 9, 'EMP004', 'Sarah Williams', 9500.00, 7315.00, 950.00, 1235.00, 'UX Designer'),
(5, 9, 'EMP005', 'Michael Johnson', 15000.00, 11550.00, 1500.00, 1950.00, 'Engineering Manager'),
(6, 9, 'EMP006', 'Emily Davis', 8500.00, 6545.00, 850.00, 1105.00, 'Junior Developer'),
(7, 9, 'EMP007', 'Daniel Martinez', 9000.00, 6930.00, 900.00, 1170.00, 'QA Engineer'),
(8, 9, 'EMP008', 'Jessica Brown', 10000.00, 7700.00, 1000.00, 1300.00, 'Data Analyst'),
(9, 9, 'EMP009', 'Christopher Lee', 13500.00, 10395.00, 1350.00, 1755.00, 'Senior Backend Developer'),
(10, 9, 'EMP010', 'Amanda Wilson', 11500.00, 8855.00, 1150.00, 1495.00, 'Frontend Lead'),
(11, 9, 'EMP011', 'Matthew Taylor', 14000.00, 10780.00, 1400.00, 1820.00, 'Solutions Architect'),
(12, 9, 'EMP012', 'Ashley Thomas', 8000.00, 6160.00, 800.00, 1040.00, 'HR Coordinator'),
(13, 9, 'EMP013', 'Joshua Jackson', 9500.00, 7315.00, 950.00, 1235.00, 'Marketing Specialist'),
(14, 9, 'EMP014', 'Stephanie White', 12500.00, 9625.00, 1250.00, 1625.00, 'Finance Manager'),
(15, 9, 'EMP015', 'Andrew Harris', 16000.00, 12320.00, 1600.00, 2080.00, 'VP of Engineering'),

-- Payroll Run 10 (May 16-31) - Processing
(16, 10, 'EMP001', 'James Anderson', 12000.00, 9240.00, 1200.00, 1560.00, 'Senior Software Engineer'),
(17, 10, 'EMP002', 'Maria Garcia', 10500.00, 8085.00, 1050.00, 1365.00, 'Product Manager'),
(18, 10, 'EMP003', 'Robert Chen', 11000.00, 8470.00, 1100.00, 1430.00, 'DevOps Engineer'),
(19, 10, 'EMP004', 'Sarah Williams', 9500.00, 7315.00, 950.00, 1235.00, 'UX Designer'),
(20, 10, 'EMP005', 'Michael Johnson', 15000.00, 11550.00, 1500.00, 1950.00, 'Engineering Manager'),
(21, 10, 'EMP006', 'Emily Davis', 8500.00, 6545.00, 850.00, 1105.00, 'Junior Developer'),
(22, 10, 'EMP007', 'Daniel Martinez', 9000.00, 6930.00, 900.00, 1170.00, 'QA Engineer'),
(23, 10, 'EMP008', 'Jessica Brown', 10000.00, 7700.00, 1000.00, 1300.00, 'Data Analyst'),
(24, 10, 'EMP009', 'Christopher Lee', 13500.00, 10395.00, 1350.00, 1755.00, 'Senior Backend Developer'),
(25, 10, 'EMP010', 'Amanda Wilson', 11500.00, 8855.00, 1150.00, 1495.00, 'Frontend Lead'),
(26, 10, 'EMP011', 'Matthew Taylor', 14000.00, 10780.00, 1400.00, 1820.00, 'Solutions Architect'),
(27, 10, 'EMP012', 'Ashley Thomas', 8000.00, 6160.00, 800.00, 1040.00, 'HR Coordinator'),
(28, 10, 'EMP013', 'Joshua Jackson', 9500.00, 7315.00, 950.00, 1235.00, 'Marketing Specialist'),
(29, 10, 'EMP014', 'Stephanie White', 12500.00, 9625.00, 1250.00, 1625.00, 'Finance Manager'),
(30, 10, 'EMP015', 'Andrew Harris', 16000.00, 12320.00, 1600.00, 2080.00, 'VP of Engineering'),

-- Payroll Run 11 (June 1-15) - Draft
(31, 11, 'EMP001', 'James Anderson', 12000.00, 9240.00, 1200.00, 1560.00, 'Senior Software Engineer'),
(32, 11, 'EMP002', 'Maria Garcia', 10500.00, 8085.00, 1050.00, 1365.00, 'Product Manager'),
(33, 11, 'EMP003', 'Robert Chen', 11000.00, 8470.00, 1100.00, 1430.00, 'DevOps Engineer'),
(34, 11, 'EMP004', 'Sarah Williams', 9500.00, 7315.00, 950.00, 1235.00, 'UX Designer'),
(35, 11, 'EMP005', 'Michael Johnson', 15000.00, 11550.00, 1500.00, 1950.00, 'Engineering Manager'),
(36, 11, 'EMP006', 'Emily Davis', 8500.00, 6545.00, 850.00, 1105.00, 'Junior Developer'),
(37, 11, 'EMP007', 'Daniel Martinez', 9000.00, 6930.00, 900.00, 1170.00, 'QA Engineer'),
(38, 11, 'EMP008', 'Jessica Brown', 10000.00, 7700.00, 1000.00, 1300.00, 'Data Analyst'),
(39, 11, 'EMP009', 'Christopher Lee', 13500.00, 10395.00, 1350.00, 1755.00, 'Senior Backend Developer'),
(40, 11, 'EMP010', 'Amanda Wilson', 11500.00, 8855.00, 1150.00, 1495.00, 'Frontend Lead'),
(41, 11, 'EMP011', 'Matthew Taylor', 14000.00, 10780.00, 1400.00, 1820.00, 'Solutions Architect'),
(42, 11, 'EMP012', 'Ashley Thomas', 8000.00, 6160.00, 800.00, 1040.00, 'HR Coordinator'),
(43, 11, 'EMP013', 'Joshua Jackson', 9500.00, 7315.00, 950.00, 1235.00, 'Marketing Specialist'),
(44, 11, 'EMP014', 'Stephanie White', 12500.00, 9625.00, 1250.00, 1625.00, 'Finance Manager'),
(45, 11, 'EMP015', 'Andrew Harris', 16000.00, 12320.00, 1600.00, 2080.00, 'VP of Engineering'),
(46, 11, 'EMP016', 'Nicole Martin', 7500.00, 5775.00, 750.00, 975.00, 'New Hire - Intern');

SET IDENTITY_INSERT PayrollEntries OFF;

PRINT 'Finance seed data completed successfully!';
PRINT '';
PRINT 'Summary:';
PRINT '  - Budgets: 8 records with 14 budget lines';
PRINT '  - Expenses: 18 records';
PRINT '  - Invoices: 12 records with 15 invoice lines';
PRINT '  - Payroll Runs: 11 records with 46 payroll entries';
GO

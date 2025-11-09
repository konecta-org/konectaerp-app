# Konecta ERP – Local Development Guide

This document walks you through everything required to run Konecta ERP locally: configuring environment variables, starting the Docker stack, seeding the databases, accessing Swagger portals through the API Gateway, logging in as the seeded admin, and onboarding new employees.

---

## 1. Prerequisites

Install the following tools before you start:

1. **Git** – to clone the repository.
2. **Docker Desktop** (Windows/macOS) or **Docker Engine** (Linux) with **Docker Compose** support.
3. **PowerShell** (installed by default on modern Windows versions) – used in the examples below. On macOS/Linux you can translate the commands to bash equivalents.
4. **.NET 9 SDK** *(optional but recommended)* – needed only if you want to run or debug individual services outside Docker.

Make sure Docker Desktop is running and you have at least 8 GB RAM available for containers.

---

## 2. Repository Structure Overview

```
konecta_erp/
├─ backend/
│  ├─ AuthenticationService/
│  ├─ FinanceService/
│  ├─ HrService/
│  ├─ InventoryService/
│  ├─ ReportingService/
│  ├─ UserManagementService/
│  └─ config/                      # Spring Cloud Config server sources
├─ docker/                         # SQL seed scripts
├─ docker-compose.yml              # Main orchestration file
└─ README.md (this guide)
```

---

## 3. First-Time Configuration

1. **Clone the repository** (skip if you already have it locally):
   ```powershell
   git clone https://github.com/<your-org>/Konecta_ERP.git
   cd Konecta_ERP/konecta_erp
   ```


3. **Check Docker resources**:
   - Ensure Docker Desktop has enough CPU/RAM allocated (recommended: ≥ 4 CPUs / 6 GB for the full stack).

---

## 4. Starting the Platform

From the repository root (`konecta_erp` folder), run:

```powershell
docker compose up -d --build
```

This command:

- Builds all microservices and supporting infrastructure.
- Starts SQL Server, Consul, RabbitMQ, MailHog, Spring Cloud Config, API Gateway, and .NET microservices.
- Runs the SQL seed container once to create databases and seed the admin user, roles, and permissions.

> **Tip:** You can monitor startup logs with `docker compose logs -f`.

---

## 5. Database & Admin Seeding

- The `sqlserver` container hosts both `Konecta_Auth` and `Konecta_UserManagement` databases.
- Seed scripts under `docker/sqlserver/` run automatically via the `sqlserver-seed` container.
- Admin account seeded automatically:
  - **Email:** `admin@konecta.com`
  - **Password:** `Admin@123456`
  - **Role:** `System Admin` (full permissions across all services)

If you need to rerun seeds (e.g., after resetting data):

```powershell
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "pa55w0rd!" -C -i /scripts/seed-admin.sql
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "pa55w0rd!" -C -i /scripts/assign-admin-permissions.sql
```

---

## 6. Verifying Service Health

| Service | Purpose | Local URL |
|---------|---------|-----------|
| Consul UI | Service discovery | http://localhost:8500 |
| RabbitMQ UI | Message broker dashboard | http://localhost:15672 (user/pass: guest/guest) |
| MailHog UI | Local email inbox | http://localhost:8025 |
| Config Server | Centralized configuration | http://localhost:8888 |
| API Gateway | Single entry point | http://localhost:8080 |
| Authentication Service | Auth microservice | http://localhost:7280 |

Ensure each service is running (`docker compose ps`). If something is unhealthy, inspect logs (`docker compose logs <service-name>`).

---

## 7. Swagger & API Access via Gateway

All Swagger UIs and APIs are exposed through the API Gateway on **port 8080**. Replace `<service>` with the relevant segment:

| Microservice | Swagger URL |
|--------------|-------------|
| Authentication | http://localhost:8080/swagger/authentication |
| HR | http://localhost:8080/swagger/hr |
| User Management | http://localhost:8080/swagger/user-management |
| Finance | http://localhost:8080/swagger/finance |
| Inventory | http://localhost:8080/swagger/inventory |
| Reporting | http://localhost:8080/swagger/reporting |

Each Swagger UI is preconfigured with JWT Bearer support.

---

## 8. Logging in as Admin

1. Open Authentication Swagger: http://localhost:8080/swagger/authentication
2. Use `POST /api/auth/login` with:
   ```json
   {
     "email": "admin@konecta.com",
     "password": "Admin@123456"
   }
   ```
3. Copy the `accessToken` from the response.
4. Click the 🔓 **Authorize** button → paste `Bearer <token>` → Authorize.

You now have full access to protected endpoints across microservices via the gateway.

---

## 9. Creating & Onboarding Employees

1. **Create employee (HR Service)**
   - Go to http://localhost:8080/swagger/hr
   - Call `POST /api/employees` with payload similar to:
     ```json
     {
       "firstName": "ahmed",
       "lastName": "mohamed",
       "workEmail": "ahmed@konecta.com",
       "personalEmail": "personal email",
       "phoneNumber": "+201273400173",
       "position": "Software Engineer",
       "departmentId": "select a valid departmnet id "
       "hireDate": "2025-01-10T00:00:00Z",
       "salary": 50000.00
     }
     ```
2. **Account provisioning**
   - Authentication Service consumes the employee-created event, creates an identity user, and sends credentials via SendGrid (or MailHog in local development).
3. **Retrieve credentials**
   - If using SendGrid, check the employee’s email inbox.
   - For local testing, open MailHog at http://localhost:8025 to view the welcome email and temporary password.

---

## 10. Employee Login & Password Change

1. **Employee login**
   - Visit http://localhost:8080/swagger/authentication
   - Call `POST /api/auth/login` with the work email and temporary password from the welcome email.
2. **Change password immediately**
   - After authorizing with the token, call `PUT /api/auth/update-password`:
     ```json
     {
       "oldPassword": "<temporary-password>",
       "newPassword": "NewSecure@Password123",
       "confirmPassword": "NewSecure@Password123"
     }
     ```
   - The Authorization header must be in the format `Bearer <token>`.
3. **Re-login** using the new password to confirm the change.

---

## 11. Troubleshooting & Useful Commands

| Scenario | Command / Action |
|----------|------------------|
| View running containers | `docker compose ps` |
| Follow logs for all services | `docker compose logs -f` |
| Tail specific service logs | `docker compose logs -f authentication-service` |
| Rebuild a single service | `docker compose build authentication-service && docker compose up -d authentication-service` |
| Rerun SQL seeds | See commands in section 5 |
| Check MailHog emails | http://localhost:8025 |
| Check RabbitMQ queues | http://localhost:15672 (guest/guest) |

**Common issues:**

- *Missing Bearer prefix*: All authenticated calls must send `Authorization: Bearer <token>`.
- *Email not received*: Verify SendGrid API key or use MailHog for local captures.
- *Database mismatch*: Rerun seed scripts after deleting mismatched records.
- *Stale JWT claims*: Logout/login again after updating roles/permissions.

---

## 12. Stopping & Cleaning Up

- Stop containers without removing data:
  ```powershell
  docker compose down
  ```

- Stop and remove volumes (WARNING: wipes SQL data):
  ```powershell
  docker compose down -v
  ```

---

## 13. Next Steps & Customization

- Tailor `application.yml` under `backend/ApiGateWay` to expose additional routes or tweak CORS/security.
- Update configuration in the Spring Cloud Config repo (`backend/config`) to change environment settings without rebuilding containers.
- Extend SQL seed scripts under `docker/sqlserver` for additional roles, permissions, or demo data.

For development-specific workflows (debugging individual services, unit tests, etc.) refer to each service’s README or project file.

---

Happy coding! 🚀 If you encounter issues, check logs first and ensure Docker resources are sufficient.

Check out TEAM_GUIDE in Docs
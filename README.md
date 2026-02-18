# Stargate API - Astronaut Career Tracking System (ACTS)

A full-stack web application for managing and tracking astronaut personnel and their career assignments. Built with ASP.NET Core 10 backend, Angular 21 frontend, PostgreSQL 16 database, and OpenSearch for audit logging/observability.

**ACTS** maintains a centralized record of all personnel who have served as astronauts, tracking assignments by rank, title, start/end dates, and career progression.

---

## 🎯 Application Overview

### Core Functionality

- **Personnel Management**: Browse, search, and manage all personnel records with advanced filtering and pagination
- **Duty Assignment Tracking**: Record and track astronaut duty assignments with automatic business rule enforcement
- **Career History**: View complete career timeline showing current and previous duty assignments, rank progression, and retirement status
- **Automated Duty Rules**: System enforces business rules including:
  - Only one active duty per person at a time
  - Automatic end-dating of previous duties when new assignments are created
  - Retirement status tracking with career end dates
  - UTC date normalization for consistency across time zones

### Key Features

✅ **REST API** with comprehensive endpoints for personnel and duty management
✅ **Web Interface** with responsive design (mobile to 4K displays)
✅ **Input Validation** with defensive coding and OWASP Top 10 prevention
✅ **WCAG 2.1 Level AA Accessibility** across all pages
✅ **JWT Authentication** with role-based access control (Admin/User)
✅ **Comprehensive Logging** to OpenSearch with searchable audit trails
✅ **Health Checks** for database and external service monitoring
✅ **31 Unit Tests** with 95.3% code coverage
✅ **Section 508 Compliance** with keyboard navigation, ARIA labels, and high-contrast modes

---

## 📋 Requirements

### System Requirements
- **Windows**, **macOS**, or **Linux** (container runtime available on all platforms)
- **Rancher Desktop** (recommended for easiest setup) - [Download](https://rancherdesktop.io/)
  - Alternative: Docker Desktop, colima, podman, or any OCI-compatible runtime
- **.NET SDK 10.0 preview+** (for local development without containers)
- **Node.js 20+** (for Angular development)
- **PostgreSQL 16+** (included in container setup)

### Optional Requirements
- **OpenSearch 2.12+** for centralized logging (included in container setup)
- **Git** for version control
- **PowerShell 5.1+** or **Bash** for running scripts

---

## 🚀 Quick Start (Container Setup - Recommended)

### Prerequisites
1. Install [Rancher Desktop](https://rancherdesktop.io/)
   - **Windows/macOS**: Download and install from [rancherdesktop.io](https://rancherdesktop.io/)
   - **Linux**: Install via package manager (see [Rancher Desktop docs](https://docs.rancherdesktop.io/getting-started/installation))
   - Verify installation: `docker --version` or `nerdctl version`
2. Clone this repository

### Setup Steps

#### 1. Create Development HTTPS Certificate (One-time)

```powershell
# PowerShell
mkdir .aspnet\https
dotnet dev-certs https -ep .aspnet\https\stargateapi.pfx -p devpass
dotnet dev-certs https --trust
```

Or on **macOS/Linux**:
```bash
mkdir -p .aspnet/https
dotnet dev-certs https -ep .aspnet/https/stargateapi.pfx -p devpass
dotnet dev-certs https --trust
```

#### 2. Start the Application

```powershell
# Development mode (with hot-reload for both frontend and backend)
docker compose up --build

# Or production mode (optimized builds)
docker compose -f docker-compose.yml up --build
```

**Note:** Rancher Desktop provides `docker` and `docker compose` commands that are fully compatible with Docker Desktop, so the commands are identical.

**What Rancher Desktop starts:**
- ✅ **API** (ASP.NET Core 10) on port 8080 (HTTP) / 8443 (HTTPS)
- ✅ **Angular Client** on port 4201 (Dev) with hot-reload
- ✅ **PostgreSQL 16** database with auto-initialized schema
- ✅ **OpenSearch 2.12** for centralized logging
- ✅ **OpenSearch Dashboards 2.12** for log visualization (port 5601)
- ✅ **Health Checks** automatically monitor all services

#### 3. Access the Application

| Service | URL | Purpose |
|---------|-----|---------|
| **Angular Client (Dev)** | http://localhost:4201 | Main web interface with hot-reload |
| **API Swagger UI** | https://localhost:8443/swagger | REST API documentation |
| **API Health Check** | https://localhost:8443/health | Service health/readiness status |
| **OpenSearch Dashboards** | http://localhost:5601 | Log exploration and analysis |
| **PostgreSQL** | localhost:5432 | Direct database access (optional) |

#### 4. Verify Setup

You should see:
- ✅ All 6 services running: `docker ps` shows `stargate-api`, `stargate-client`, `stargate-db`, `stargate-opensearch`, `stargate-dashboards`, `stargate-init`
- ✅ No errors in container logs: `docker compose logs -f`
- ✅ Angular app loads at http://localhost:4201
- ✅ API responds at https://localhost:8443/api/person

---

## 💻 Local Development (Without Docker)

### Prerequisites
1. **.NET SDK 10.0+**: [Download](https://dotnet.microsoft.com/download)
2. **Node.js 20+**: [Download](https://nodejs.org/)
3. **PostgreSQL 16**: [Download](https://www.postgresql.org/download/)
4. **OpenSearch 2.12**: [Download](https://opensearch.org/) (optional, logging will fail gracefully)

### Setup Steps

#### 1. Create PostgreSQL Database

```sql
-- Using psql, pgAdmin, or your favorite SQL tool
CREATE DATABASE starbase;
CREATE USER stargate_user WITH PASSWORD 'stargate_password';
ALTER ROLE stargate_user WITH CREATEDB;
GRANT ALL PRIVILEGES ON DATABASE starbase TO stargate_user;
```

#### 2. Configure Backend

```powershell
cd api

# Update appsettings.Development.json with your connection string
# Change: "Host=stargate-db;..." to "Host=localhost;..."
# Change: Username=postgres;Password=postgres to your DB credentials
```

Edit `api\appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=starbase;Username=stargate_user;Password=stargate_password;Port=5432;"
  }
}
```

#### 3. Run Migrations & Start API

```powershell
cd api

# Install dependencies
dotnet restore

# Apply migrations to create schema
dotnet ef database update

# Run the application
dotnet run

# API available at:
# - https://localhost:5001/swagger
# - http://localhost:5000/swagger
```

#### 4. Run Angular Client

```powershell
cd ../client

# Install dependencies
npm install

# Start development server
ng serve

# Client available at http://localhost:4200
```

#### 5. (Optional) Run OpenSearch Locally

```bash
# macOS/Linux with Docker
docker run -d -p 9200:9200 -p 9600:9600 \
  -e "discovery.type=single-node" \
  -e OPENSEARCH_ADMIN_PASSWORD=Admin@123 \
  -e OPENSEARCH_INITIAL_ADMIN_PASSWORD=Admin@123 \
  opensearchproject/opensearch:2.12.0

# Logs will be sent here at http://localhost:9200/stargate-*
```

---

## �️ Architecture Overview

### System Components

```
┌─────────────────────────────────────────────────────────────────┐
│                      User Browser                               │
│                   (Angular 21 App)                              │
│              http://localhost:4201 (Dev)                        │
└─────────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │  nginx (reverse   │
                    │   proxy) / ALB    │
                    └─────────┬─────────┘
                              │
             ┌────────────────┼────────────────┐
             │                │                │
    ┌────────▼─────────┐ ┌────▼──────────┐ ┌──▼──────────────┐
    │  ASP.NET Core 10 │ │ PostgreSQL 16 │ │ OpenSearch 2.12 │
    │  (API)           │ │ (Database)    │ │ (Logging)       │
    │ Port 8080/8443   │ │ Port 5432     │ │ Port 9200       │
    └──────────────────┘ └───────────────┘ └─────────────────┘
         (HTTP/HTTPS)      (EF Core ORM)    (Elasticsearch)
```

### Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | Angular 21, TypeScript, SCSS | Responsive web UI with WCAG 2.1 compliance |
| **Backend** | ASP.NET Core 10, C# | REST API with business logic and validation |
| **Database** | PostgreSQL 16, EF Core | Relational data storage with migrations |
| **Logging** | OpenSearch 2.12 | Centralized audit logs and observability |
| **Authentication** | JWT + Role-based RBAC | Secure access control (Admin/User roles) |
| **Container Runtime** | Rancher Desktop, containerd, moby | Reproducible deployment and orchestration |
| **Testing** | xUnit + Moq | Unit test framework |

---

## 📡 API Endpoints

### Authentication
```
POST   /api/auth/register          Register new user
POST   /api/auth/login             Login and receive JWT token
GET    /api/auth/me                Get current user info
```

### Personnel Management
```
GET    /api/person                 List all personnel (paginated, sortable, searchable)
GET    /api/person/{id}            Get person by ID
GET    /api/person/name/{name}     Get person by name with astronaut details
POST   /api/person                 Create new person
PUT    /api/person/{id}            Update person
DELETE /api/person/{id}            Delete person
```

### Astronaut Duties
```
GET    /api/astronautduty                    List all duties (paginated)
GET    /api/astronautduty/name/{personName}  Get all duties for a person
POST   /api/astronautduty                    Create new duty assignment
PUT    /api/astronautduty/{id}               Update duty record
DELETE /api/astronautduty/{id}               Delete duty record
```

### Health & Monitoring
```
GET    /health                     Liveness probe (service is running)
GET    /health/live                Kubernetes-style liveness probe
GET    /health/ready               Kubernetes-style readiness probe
GET    /api/health/status          Detailed health status with dependencies
```

### Example API Calls

```bash
# Get all personnel
curl https://localhost:8443/api/person \
  -H "Authorization: Bearer <JWT_TOKEN>" \
  -H "Content-Type: application/json"

# Create a duty assignment
curl -X POST https://localhost:8443/api/astronautduty \
  -H "Authorization: Bearer <JWT_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "personName": "John Smith",
    "rank": "Major",
    "dutyTitle": "Pilot",
    "startDate": "2026-03-01T00:00:00Z"
  }'

# Search for a person by name
curl https://localhost:8443/api/person/name/John%20Smith \
  -H "Authorization: Bearer <JWT_TOKEN>"
```

---

## 🎨 Frontend Features

### Pages

#### **Home (Landing Page)**
- Overview of application with ACTS branding
- Stargate Command themed UI with animated star field
- Quick statistics: Total Personnel, Active Astronauts, Non-Astronauts
- Feature highlights and call-to-action

#### **Personnel Directory** (`/personnel`)
- List all personnel with sortable columns (Name, Current Rank, Current Duty, Career Start)
- Real-time search filtering
- Pagination (10, 25, 50 items per page)
- Click-through to individual person details
- Responsive design for mobile/tablet/desktop

#### **Person Detail** (`/personnel/:id`)
- Complete career profile
- Current duty assignment (if active)
- Full duty history with timeline visualization
- Career start and end dates
- Retirement status indicator
- Breadcrumb navigation

#### **Add Person** (`/add-person`)
- Form-based person creation
- Input validation (required name, character constraints)
- Success confirmation with auto-redirect
- Error handling with retry options
- WCAG 2.1 compliant form

#### **Add Duty** (`/add-duty`)
- Duty assignment form with searchable person dropdown
- Predefined rank and title dropdowns
- Date validation (start date must be in future)
- Duration validation (prevents zero/negative day duties)
- Automatic previous duty end-dating
- Business rule enforcement

### Accessibility Features
- ✅ Keyboard navigation (Tab, Enter, Arrow keys, Escape)
- ✅ ARIA labels and roles for screen readers
- ✅ High-contrast mode support
- ✅ Skip navigation links
- ✅ Focus management and visible focus indicators
- ✅ Semantic HTML structure
- ✅ Mobile-optimized (44x44px touch targets minimum)
- ✅ Prefers-reduced-motion support for animations

---

## 🗄️ Database Schema

### Tables

#### **Person**
```sql
CREATE TABLE "Person" (
  "Id" SERIAL PRIMARY KEY,
  "Name" VARCHAR(255) NOT NULL,
  "CreatedAtUtc" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "UpdatedAtUtc" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

#### **AstronautDetail**
```sql
CREATE TABLE "AstronautDetail" (
  "Id" SERIAL PRIMARY KEY,
  "PersonId" INT NOT NULL REFERENCES "Person"("Id"),
  "CurrentRank" VARCHAR(50),
  "CurrentDutyTitle" VARCHAR(100),
  "CareerStartDateUtc" TIMESTAMP,
  "CareerEndDateUtc" TIMESTAMP,
  "IsRetired" BOOLEAN
);
```

#### **AstronautDuty**
```sql
CREATE TABLE "AstronautDuty" (
  "Id" SERIAL PRIMARY KEY,
  "PersonId" INT NOT NULL REFERENCES "Person"("Id"),
  "Rank" VARCHAR(50) NOT NULL,
  "Title" VARCHAR(100) NOT NULL,
  "StartDateUtc" TIMESTAMP NOT NULL,
  "EndDateUtc" TIMESTAMP,
  "CreatedAtUtc" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

### Key Constraints
- Person Name is unique identifier for business logic
- One-to-many relationship: Person → AstronautDuty (1:N)
- One-to-one relationship: Person → AstronautDetail (1:1)
- UTC timestamps for consistency across time zones

---

## 🧪 Testing

### Running Tests

```powershell
# Run all tests
cd api.tests
dotnet test

# Run with coverage report
dotnet test --collect:"XPlat Code Coverage" --settings .runsettings

# Generate HTML coverage report
reportgenerator -reports:"TestResults\*\coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html

# View coverage report
start CoverageReport\index.html
```

### Test Suite

- **31 Unit Tests** covering authentication, business logic, validation
- **95.3% Code Coverage** on testable code (123/129 lines)
- **xUnit** test framework with Moq for mocking
- **EntityFrameworkCore.InMemory** for isolated database tests
- Tests for:
  - ✅ JWT token generation and validation
  - ✅ Password hashing with bcrypt
  - ✅ Role-based authorization
  - ✅ Input validation and error handling
  - ✅ Business rule enforcement

---

## 📧 Configuration

### Environment Variables (Docker)

Key environment variables can be configured in `docker-compose.yml`:

```yaml
ASPNETCORE_ENVIRONMENT: Development  # Development, Staging, Production
DATABASE_HOST: stargate-db
DATABASE_NAME: starbase
DATABASE_USER: postgres
DATABASE_PASSWORD: postgres
OPENSEARCH_ENDPOINT: http://stargate-opensearch:9200
JWT_SECRET_KEY: (configured via Secrets Manager in production)
LOG_LEVEL: Information  # Debug, Information, Warning, Error
```

### Local Development Config

Edit `api/appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=starbase;Username=postgres;Password=postgres;Port=5432;"
  }
}
```

Edit `client/src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080'  // Local dev API
};
```

---

## 🔧 Development Workflow

### Making Changes

#### Backend Changes
```powershell
# 1. Make changes to C# files in api/
# 2. Docker instantly recompiles on save
# 3. Test in Swagger UI: https://localhost:8443/swagger
# 4. Run tests: dotnet test
```

#### Frontend Changes
```powershell
# 1. Make changes to Angular components in client/src/
# 2. Browser auto-refreshes with hot-reload (port 4201)
# 3. Check console for errors
# 4. Run tests: ng test
```

#### Database Schema Changes
```powershell
# 1. Update EF Core models in api/Business/Data/
# 2. Create migration:
dotnet ef migrations add YourMigrationName

# 3. Apply migration:
dotnet ef database update

# 4. Migration code is generated in api/Business/Migrations/
# 5. Docker automatically applies on startup
```

### Code Quality

```powershell
# Format code (C#)
dotnet csharpier api/

# Lint TypeScript/Angular
cd client && ng lint

# Run unit tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🐛 Troubleshooting

### Issue: Port Already in Use

**Problem**: Docker fails with "port already allocated"

**Solution**:
```powershell
# Find and kill process on port
netstat -ano | findstr :8080
taskkill /PID <PID> /F

# Or change port in docker-compose.yml
# Change "8080:8080" to "8081:8080"
```

### Issue: PostgreSQL Connection Refused

**Problem**: API can't connect to database

**Checks**:
```powershell
# Verify PostgreSQL is running
docker ps | findstr stargate-db

# Check logs
docker compose logs stargate-db

# Verify connection string in appsettings.json
# For Docker: Host=stargate-db (not localhost)
# For local: Host=localhost
```

### Issue: OpenSearch Health Check Failing

**Problem**: Logging not working, OpenSearch errors in logs

**Solution**:
```powershell
# OpenSearch takes time to start - wait 30 seconds
# Check status
curl http://localhost:9200

# Restart OpenSearch
docker compose restart stargate-opensearch

# View logs
docker compose logs -f stargate-opensearch
```

### Issue: Hot-Reload Not Working

**Problem**: Changes don't reflect when editing files

**Solution**:
```powershell
# Verify volume mounting
docker inspect stargate-client | findstr -A 5 "Mounts"

# Ensure file polling interval is adequate
# In docker-compose.override.yml:
environment:
  - CHOKIDAR_USEPOLLING=true  # Enable file polling
  - CHOKIDAR_INTERVAL=2000    # Check every 2 seconds

# Restart container
docker compose restart stargate-client
```

### Issue: Certificate Trust Issues (HTTPS)

**Problem**: Browser shows "ERR_CERT_AUTHORITY_INVALID"

**Solution**:
```powershell
# Regenerate dev certificate
rm .aspnet/https/stargateapi.pfx
dotnet dev-certs https -ep .aspnet\https\stargateapi.pfx -p devpass
dotnet dev-certs https --trust

# Restart Rancher Desktop / containers
docker compose down
docker compose up --build
```

---

## 📚 Documentation

- [CHANGELOG.md](./CHANGELOG.md) - Feature history and release notes
- [SUGGESTIONS.md](./SUGGESTIONS.md) - Modernization and scalability recommendations
- [REQUIREMENTS.md](./REQUIREMENTS.md) - Original project requirements
- [COVERAGE_REPORT.md](./api.tests/COVERAGE_REPORT.md) - Code coverage analysis

---

## 🔒 Security Considerations

### Current Implementation
- ✅ JWT authentication with secure token generation
- ✅ Password hashing with bcrypt
- ✅ SQL injection prevention (EF Core parameterized queries only)
- ✅ HTTPS/TLS encryption in transit
- ✅ CORS policy restricting cross-origin requests
- ✅ Input validation on all API endpoints

### Recommended Enhancements
See [SUGGESTIONS.md](./SUGGESTIONS.md) for detailed recommendations:
- Migrate secrets to AWS Secrets Manager
- Implement ICAM integration for government SSO
- Add SAST/DAST security scanning
- Enable comprehensive audit logging
- Configuration via AWS Systems Manager Parameter Store

---

## 🚢 Deployment

### Prerequisites for Production
- AWS GovCloud IL-5 certified account (recommended)
- Application Load Balancer (ALB) for TLS termination
- AWS WAF for rate limiting and attack prevention
- RDS PostgreSQL for managed database
- OpenSearch domain for centralized logging
- ECS Fargate or similar container orchestration

### Deployment Steps
See [SUGGESTIONS.md](./SUGGESTIONS.md) for comprehensive AWS deployment guide including:
- Secrets Manager integration
- Multi-region high availability setup
- CI/CD pipeline with SAST/DAST
- STIG compliance configuration
- Disaster recovery planning

---

## 📞 Support

### Debugging
- **API Logs**: `docker compose logs stargate-api`
- **Frontend Logs**: Browser DevTools (F12)
- **Database Logs**: `docker compose logs stargate-db`
- **OpenSearch Logs**: `docker compose logs stargate-opensearch`

### Common Commands
```powershell
# View all running services
docker compose ps

# Stop all services
docker compose down

# Clean up volumes and images
docker system prune -a

# Rebuild everything
docker compose up --build --force-recreate

# View detailed logs
docker compose logs -f --tail=100
```

---

## 📄 License

This project is provided as-is for technical interview purposes.

# Changelog

High-level summary of features and improvements organized for interview discussion.

## Core Requirements (REQUIREMENTS.md Compliance)

### REST API - Personnel Management
- ✅ **Retrieve all people** with personnel directory listing
- ✅ **Retrieve person by name** with astronaut career details and duty history 
- ✅ **Add/update person** with full form validation
- ✅ **Retrieve astronaut duties by name** with complete duty records
- ✅ **Add astronaut duty** with business rule enforcement

### REST API - Business Rules Implementation
- ✅ **Astronaut duty rules** enforced: current duty validation, retirement logic, previous duty end date calculations
- ✅ **Input validation** with defensive coding: null checks, empty string validation, 404 responses for invalid names
- ✅ **Defensive coding improvements**: Replaced all raw SQL with EF Core LINQ (SQL injection prevention), proper error handling, request validation

### Web Interface - Angular 21 Frontend
- ✅ **Personnel management system** with searchable directory, sortable columns, filtering, and pagination
- ✅ **Person detail pages** showing current duty assignment and complete astronaut duty history
- ✅ **Add duty functionality** with business rule enforcement (duty duration validation, retirement support)
- ✅ **Add person functionality** with validation and success feedback
- ✅ **Custom searchable dropdown** for duty person selection ensuring data integrity

### Testing & Code Quality
- ✅ **31 comprehensive unit tests** for authentication system
- ✅ **All tests passing** with continuous validation

### Process Logging & Observability
- ✅ **Request/exception logging** middleware capturing all API activity
- ✅ **Unique exception IDs** returned in error responses for user troubleshooting
- ✅ **OpenSearch + Dashboards stack** providing centralized log storage and visualization
- ✅ **Automated index creation** on startup with zero-config dashboard setup

---

## Interview Talking Points

### Security & Defensive Coding
- **SQL Injection Prevention**: Eliminated all raw Dapper SQL queries, replacing with Entity Framework Core LINQ for type-safe database access
- **Input Validation Framework**: Comprehensive validation on person names, duty creation, with proper 404 responses for invalid queries
- **CORS Policy**: Configured secure cross-origin access for frontend-backend communication
- **Authentication System**: JWT token generation with role-based access control (Admin/User roles)
- **Password Security**: Implemented bcrypt hashing with salt, verified through 8+ security-focused tests

### Code Architecture & Patterns
- **Base Controller Pattern**: Created ApiControllerBase abstract class to centralize API routing configuration, eliminating boilerplate and improving maintainability
- **Health Check System**: Implemented comprehensive health monitoring for PostgreSQL and OpenSearch with Kubernetes-style liveness/readiness probes
- **Service Layer Pattern**: Dedicated PersonService and AstronautDutyService classes in the client app for API communication with consistent error handling
- **Type Safety**: TypeScript strict mode, comprehensive interfaces, Angular Signals for reactive state management
- **DRY Principle**: Centralized routing, base classes, and utility components to reduce code duplication

### Database Architecture
- **PostgreSQL 16**: Production-grade relational database with containerization support
- **Entity Framework Core**: Migration-based schema management with deterministic seed data
- **Proper Relationships**: Foreign key constraints between Person, AstronautDetail, and AstronautDuty tables
- **UTC Normalization**: Consistent date handling across all astronaut duty operations

### Frontend Features
- **Accessibility (WCAG 2.1 Level AA)**: Section 508 compliance with keyboard navigation, ARIA attributes, high-contrast support, and 44x44px touch targets
- **Responsive Design**: Mobile-first approach supporting 320px to 4K displays with adaptive layouts
- **Custom Components**: Built searchable dropdown with keyboard support, loading states, error handling, and accessibility
- **Data Integrity**: Client-side validation preventing zero/negative duty durations before API submission
- **Polished UI**: Stateful components with loading spinners, success confirmations, empty states, and error recovery options

### DevOps & Containerization
- **Multi-Stage Docker**: Optimized builds for both .NET API (production) and Angular client (development/production)
- **Docker Compose**: 6-service orchestration (API, client, PostgreSQL, OpenSearch, Dashboards, init) with health checks and dependency management
- **Hot Reload Development**: Volume-mounted code with automatic browser refresh for rapid iteration
- **HTTPS/TLS Support**: Development certificates with Kestrel configuration
- **Cross-Platform**: Tested on Windows Ranher Desktop with Windows-specific networking fixes

---

## 2026-02-17

### API Backend - Core Features
- Personnel retrieval by name and complete listing
- Astronaut duty assignment with business rule validation
- GetCurrentUser endpoint for authentication context
- UTC timestamp normalization for duty dates
- BaseResponse wrapper for consistent API responses

### API Backend - Infrastructure
- Health check endpoints (/api/Health/status, /api/Health/live, /api/Health/ready) with PostgreSQL and OpenSearch monitoring
- Request/exception logging middleware with unique exception IDs
- CORS configuration for frontend access
- Input validation and 404 handling for name-based queries

### Frontend - Core Pages
- Personnel listing with search, filtering, sorting, pagination
- Person detail view with duty history and career timeline
- Add person form with validation
- Add astronaut duty form with person dropdown and date validation
- Responsive navigation with breadcrumbs

### Frontend - Accessibility & UX
- WCAG 2.1 Level AA compliance across all pages
- Keyboard navigation and screen reader support
- Mobile-optimized design (320px-4K responsive)
- Loading states, error messages, success confirmations
- Custom searchable dropdown with full keyboard support

### Database
- PostgreSQL 16 migration from SQLite
- Deterministic schema migrations and seed data
- Person, AstronautDetail, AstronautDuty entity relationships
- Composite indexes for efficient name-based queries

### Containerization & Deployment
- Multi-stage Dockerfile for .NET API with .NET 10
- Multi-stage Dockerfile for Angular client with nginx reverse proxy
- Docker Compose orchestration with 6 services
- Hot-reload development environment with docker-compose.override.yml
- Health check-driven service startup ordering

### Testing & Code Quality
- 31 unit tests for authentication system (xUnit, Moq)
- Focused measurement strategy targeting testable code

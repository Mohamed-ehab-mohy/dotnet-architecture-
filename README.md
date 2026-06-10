# .NET Clean Architecture

A clean architecture solution template for .NET 10 applications following Domain-Driven Design principles and CQRS pattern.

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                  Presentation (API)                  │
│              Controllers · Middlewares · Filters      │
├─────────────────────────────────────────────────────┤
│                   Application                        │
│         Commands · Queries · Handlers · DTOs         │
│                  MediatR · FluentValidation           │
├─────────────────────────────────────────────────────┤
│                   Infrastructure                     │
│           EF Core · Repositories · Services          │
├─────────────────────────────────────────────────────┤
│                      Domain                          │
│        Entities · Value Objects · Interfaces         │
└─────────────────────────────────────────────────────┘
```

## Technologies

- **.NET 10** - Target Framework
- **Entity Framework Core** - ORM for data access
- **MediatR** - CQRS implementation
- **FluentValidation** - Request validation
- **AutoMapper** - Object mapping
- **ASP.NET Core Identity** - Authentication & Authorization
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Unit testing

## Projects

| Project | Description |
|---------|-------------|
| `Domain` | Core business logic, entities, value objects, interfaces |
| `Application` | Use cases, CQRS commands/queries, DTOs, validation |
| `Infrastructure` | Database, external services, identity implementation |
| `Presentation.API` | Web API controllers, middlewares, filters |

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (or Docker)

### Run with Docker

```bash
docker-compose up -d
```

### Run locally

```bash
# Update connection string in appsettings.json
cd src/Presentation/API
dotnet run
```

### Apply migrations

```bash
dotnet ef database update -p src/Infrastructure -s src/Presentation/API
```

## Project Structure

```
Solution.sln
├── src/
│   ├── Domain/                    # Core business layer
│   │   ├── Entities/              # Domain entities
│   │   ├── ValueObjects/          # Value objects
│   │   ├── Enums/                 # Enumerations
│   │   ├── Events/                # Domain events
│   │   ├── Exceptions/            # Custom exceptions
│   │   └── Interfaces/            # Repository interfaces
│   ├── Application/               # Application use cases
│   │   ├── Common/                # Shared concerns
│   │   │   ├── Interfaces/        # Application interfaces
│   │   │   ├── Models/            # Common models
│   │   │   ├── Mappings/          # AutoMapper profiles
│   │   │   └── Behaviours/        # MediatR pipelines
│   │   └── Features/              # Feature modules
│   │       ├── Products/          # Product use cases
│   │       └── Users/             # User use cases
│   ├── Infrastructure/            # Infrastructure implementation
│   │   ├── Persistence/           # EF Core setup
│   │   ├── Services/              # External services
│   │   └── Identity/              # ASP.NET Identity
│   └── Presentation/API/          # Web API project
│       ├── Controllers/           # API controllers
│       ├── Middlewares/            # Request pipeline
│       ├── Filters/               # Action filters
│       └── Models/                # Response models
└── tests/
    ├── Domain.Tests/              # Domain unit tests
    ├── Application.Tests/         # Application unit tests
    ├── Infrastructure.Tests/      # Infrastructure tests
    └── API.Tests/                 # API integration tests
```

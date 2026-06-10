# .NET Clean Architecture - Full Documentation

> **Project:** dotnet-architecture  
> **Architecture:** Clean Architecture + CQRS  
> **Target:** .NET 10

---

## Table of Contents

1. [Architecture Layers](#1-architecture-layers)
2. [Solution Structure](#2-solution-structure)
3. [Domain Layer](#3-domain-layer)
4. [Application Layer](#4-application-layer)
5. [Infrastructure Layer](#5-infrastructure-layer)
6. [Presentation Layer](#6-presentation-layer)
7. [Tests](#7-tests)
8. [Root Files](#8-root-files)
9. [Data Flow](#9-data-flow)
10. [Best Practices](#10-best-practices)

---

## 1. Architecture Layers

### Dependency Rule

```
Presentation  →  Application  →  Domain
     ↓                ↓
Infrastructure  →  Application  →  Domain
```

- **Domain** depends on nothing (no external packages)
- **Application** depends only on Domain
- **Infrastructure** depends on Application + Domain
- **Presentation** depends on Application + Infrastructure

### Layer Responsibilities

| Layer | Responsibility | Contains |
|-------|---------------|----------|
| **Domain** | Core business logic & rules | Entities, Value Objects, Enums, Events, Exceptions, Interfaces |
| **Application** | Use cases & orchestration | Commands, Queries, Handlers, DTOs, Validators, Behaviours |
| **Infrastructure** | External concerns implementation | Database, Repositories, Email, File Storage, Payment, Identity |
| **Presentation/API** | HTTP interface | Controllers, Middlewares, Filters, Models |

---

## 2. Solution Structure

```
Solution.sln
├── src/
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   └── Presentation/API/
├── tests/
│   ├── Domain.Tests/
│   ├── Application.Tests/
│   ├── Infrastructure.Tests/
│   └── API.Tests/
├── Directory.Build.props
├── docker-compose.yml
├── Dockerfile
├── .gitignore
├── README.md
└── ARCHITECTURE.md
```

---

## 3. Domain Layer

**Project:** `src/Domain/Domain.csproj`  
**Namespace:** `Domain.*`  
**Dependencies:** None (pure .NET)

The Domain layer is the heart of the application. It contains enterprise-wide business rules and logic.

### 3.1 Entities

| File | Role | Key Properties | Business Rules |
|------|------|----------------|----------------|
| `Entities/Product.cs` | Represents a product in the system | `Id`, `Name`, `Description`, `Price` | Price must be > 0; uses private setters for encapsulation |
| `Entities/User.cs` | Represents a system user | `Id`, `Name`, `Email`, `PasswordHash`, `Role` | Profile can be updated via `UpdateProfile()` method |
| `Entities/Order.cs` | Represents a customer order | `Id`, `UserId`, `TotalAmount`, `Status`, `OrderDate` | Status transitions are validated: Pending→Processing→Shipped→Delivered; delivered orders cannot be cancelled |

### 3.2 Value Objects

| File | Role | Key Properties | Characteristics |
|------|------|----------------|----------------|
| `ValueObjects/Money.cs` | Represents monetary value | `Amount`, `Currency` | Immutable, implements equality, validates currency consistency on Add/Subtract |
| `ValueObjects/Address.cs` | Represents a physical address | `Street`, `City`, `Country`, `PostalCode` | Immutable, implements equality, validates required fields |

### 3.3 Enums

| File | Role | Values |
|------|------|--------|
| `Enums/OrderStatus.cs` | Order lifecycle states | `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled` |
| `Enums/UserRole.cs` | System user roles | `Admin`, `User`, `Moderator` |

### 3.4 Events

| File | Role | Properties |
|------|------|------------|
| `Events/OrderCreatedEvent.cs` | Raised when a new order is created | `OrderId`, `UserId`, `OccurredOn` |
| `Events/UserRegisteredEvent.cs` | Raised when a new user registers | `UserId`, `Email`, `OccurredOn` |

### 3.5 Exceptions

| File | Role | When Thrown |
|------|------|-------------|
| `Exceptions/DomainException.cs` | Base exception for all domain errors | Base class for domain-specific exceptions |
| `Exceptions/ProductNotFoundException.cs` | Product not found | When a product with given ID doesn't exist (inherits `DomainException`) |

### 3.6 Interfaces

| File | Role | Methods |
|------|------|---------|
| `Interfaces/IRepository.cs` | Generic repository contract | `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` |
| `Interfaces/IUnitOfWork.cs` | Unit of Work contract | `SaveChangesAsync` |

### 3.7 Project File

| File | Role |
|------|------|
| `Domain.csproj` | Project file - no external packages, pure .NET 10 class library |

---

## 4. Application Layer

**Project:** `src/Application/Application.csproj`  
**Namespace:** `Application.*`  
**Dependencies:** `Domain` project + MediatR, FluentValidation, AutoMapper, BCrypt.Net

The Application layer orchestrates business use cases through the CQRS pattern.

### 4.1 Common

#### 4.1.1 Interfaces

| File | Role | Method(s) |
|------|------|-----------|
| `Common/Interfaces/IEmailService.cs` | Email sending contract | `SendEmailAsync(to, subject, body)` |
| `Common/Interfaces/IDateTime.cs` | DateTime abstraction for testability | `Now` property |

#### 4.1.2 Models

| File | Role | Properties |
|------|------|------------|
| `Common/Models/Result.cs` | Generic result pattern wrapping success/failure | `IsSuccess`, `Value`, `Error` with static factory methods `Success()` and `Failure()` |
| `Common/Models/PaginatedList.cs` | Paginated result wrapper | `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages` |

#### 4.1.3 Mappings

| File | Role | Mappings |
|------|------|----------|
| `Common/Mappings/MappingProfile.cs` | AutoMapper profile for entity↔DTO conversions | `Product ↔ ProductDto`, `CreateProductDto → Product` |

#### 4.1.4 Behaviours (MediatR Pipeline)

| File | Role | Behavior |
|------|------|----------|
| `Common/Behaviours/ValidationBehaviour.cs` | Pre-processing validation pipeline | Runs all FluentValidation validators before the handler executes |
| `Common/Behaviours/LoggingBehaviour.cs` | Request/response logging pipeline | Logs request name before and after handler execution |

### 4.2 Features / Products

#### Commands

**CreateProduct** (Folder: `Features/Products/Commands/CreateProduct/`)

| File | Role |
|------|------|
| `CreateProductCommand.cs` | Command record with `Name`, `Description`, `Price` fields. Implements `IRequest<Result<int>>` |
| `CreateProductCommandHandler.cs` | Handler that creates a `Product` entity, saves via repository, returns the new product ID |
| `CreateProductCommandValidator.cs` | Validator: Name required (max 100), Description max 500, Price > 0 |

**UpdateProduct** (Folder: `Features/Products/Commands/UpdateProduct/`)

| File | Role |
|------|------|
| `UpdateProductCommand.cs` | Command record with `Id`, `Name`, `Description`, `Price`. Implements `IRequest<Result<int>>` |
| `UpdateProductCommandHandler.cs` | Handler that finds existing product, updates details, saves changes |
| `UpdateProductCommandValidator.cs` | Validator: Id > 0, Name required (max 100), Price > 0 |

#### Queries

**GetProductById** (Folder: `Features/Products/Queries/GetProductById/`)

| File | Role |
|------|------|
| `GetProductByIdQuery.cs` | Query record with `Id`. Implements `IRequest<ProductDto?>` |
| `GetProductByIdQueryHandler.cs` | Handler that retrieves product by ID and maps to `ProductDto` |

**GetProducts** (Folder: `Features/Products/Queries/GetProducts/`)

| File | Role |
|------|------|
| `GetProductsQuery.cs` | Query record. Implements `IRequest<IReadOnlyList<ProductDto>>` |
| `GetProductsQueryHandler.cs` | Handler that retrieves all products and maps them to `ProductDto` list |

#### DTOs

| File | Role | Properties |
|------|------|------------|
| `Features/Products/DTOs/ProductDto.cs` | Data transfer object for product output | `Id`, `Name`, `Description`, `Price` |
| `Features/Products/DTOs/CreateProductDto.cs` | Data transfer object for product creation | `Name`, `Description`, `Price` |

### 4.3 Features / Users

| File | Role |
|------|------|
| `Features/Users/RegisterUserCommand.cs` | Command for user registration with `Name`, `Email`, `Password`, `Role` |
| `Features/Users/RegisterUserCommandHandler.cs` | Handler that hashes password, creates `User` entity, saves to repository |
| `Features/Users/LoginUserCommand.cs` | Command for user login with `Email`, `Password` |
| `Features/Users/GetUserByIdQuery.cs` | Query to retrieve user by ID |
| `Features/Users/GetUserByIdQueryHandler.cs` | Handler that fetches user by ID from repository |

### 4.4 Dependency Injection

| File | Role |
|------|------|
| `DependencyInjection.cs` | Extension method `AddApplication()` that registers MediatR, AutoMapper, FluentValidation validators, and pipeline behaviours |

### 4.5 Project File

| File | Role |
|------|------|
| `Application.csproj` | Project file referencing Domain + NuGet packages (MediatR, FluentValidation, AutoMapper, BCrypt.Net) |

---

## 5. Infrastructure Layer

**Project:** `src/Infrastructure/Infrastructure.csproj`  
**Namespace:** `Infrastructure.*`  
**Dependencies:** `Application` project + EF Core, ASP.NET Core Identity

The Infrastructure layer implements the interfaces defined in Domain and Application.

### 5.1 Persistence

#### 5.1.1 Configurations

| File | Role | Configures |
|------|------|------------|
| `Persistence/Configurations/ProductConfiguration.cs` | EF Core Fluent API config for Product | Table name `Products`, primary key, property constraints (max length, decimal precision) |
| `Persistence/Configurations/UserConfiguration.cs` | EF Core Fluent API config for User | Table name `Users`, primary key, property constraints |

#### 5.1.2 Repositories

| File | Role | Key Methods |
|------|------|-------------|
| `Persistence/Repositories/Repository.cs` | Generic repository implementation of `IRepository<T>` | `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` — all call `SaveChangesAsync` |
| `Persistence/Repositories/ProductRepository.cs` | Product-specific repository extending `Repository<Product>` | `GetByPriceRangeAsync`, `SearchByNameAsync` |
| `Persistence/Repositories/UnitOfWork.cs` | Unit of Work implementation | `SaveChangesAsync` delegating to DbContext |

#### 5.1.3 Migrations

| File | Role |
|------|------|
| `Persistence/Migrations/20240101000000_InitialCreate.cs` | Initial EF Core migration creating Products, Users, and Orders tables |

#### 5.1.4 DbContext

| File | Role | DbSets |
|------|------|--------|
| `Persistence/ApplicationDbContext.cs` | Main database context | `Products`, `Users`, `Orders` — applies entity configurations in `OnModelCreating` |

### 5.2 Services

| File | Role | Implementation Notes |
|------|------|---------------------|
| `Services/EmailService.cs` | Email sending implementation | Logs to ILogger (placeholder for SMTP/SendGrid) |
| `Services/FileStorageService.cs` | File storage implementation | Returns mock URL (placeholder for Azure Blob/AWS S3) |
| `Services/PaymentGatewayService.cs` | Payment processing implementation | Returns success boolean (placeholder for Stripe/PayPal) |

### 5.3 Identity

| File | Role | Base Class |
|------|------|------------|
| `Identity/ApplicationUser.cs` | ASP.NET Core Identity user entity | Extends `IdentityUser` with `FullName` |
| `Identity/ApplicationRole.cs` | ASP.NET Core Identity role entity | Extends `IdentityRole` |

### 5.4 Dependency Injection

| File | Role |
|------|------|
| `DependencyInjection.cs` | Extension method `AddInfrastructure()` that registers DbContext, Identity, Repositories, UnitOfWork, and all Services |

### 5.5 Project File

| File | Role |
|------|------|
| `Infrastructure.csproj` | Project file referencing Application + NuGet packages (EF Core, Npgsql/PostgreSQL, Identity) |

---

## 6. Presentation Layer

**Project:** `src/Presentation/API/API.csproj`  
**Namespace:** `Presentation.API.*`  
**Dependencies:** `Application` + `Infrastructure` projects

The Presentation layer handles HTTP requests and responses.

### 6.1 Controllers

| File | Route | Endpoints | Role |
|------|-------|-----------|------|
| `Controllers/ProductsController.cs` | `api/products` | `GET /`, `GET /{id}`, `POST /`, `PUT /{id}` | Product CRUD operations via MediatR |
| `Controllers/UsersController.cs` | `api/users` | `GET /{id}`, `POST /register`, `POST /login` | User management endpoints |
| `Controllers/AuthController.cs` | `api/auth` | `POST /login`, `POST /register`, `POST /refresh-token` | Authentication and token management |

### 6.2 Middlewares

| File | Role | Behavior |
|------|------|----------|
| `Middlewares/GlobalExceptionHandlerMiddleware.cs` | Global error handling | Catches all unhandled exceptions, logs them, returns structured JSON `ApiResponse` with appropriate HTTP status code |
| `Middlewares/RequestLoggingMiddleware.cs` | Request logging | Logs HTTP method, path, status code, and duration for every request |

### 6.3 Filters

| File | Role | Behavior |
|------|------|----------|
| `Filters/ValidationActionFilter.cs` | Model state validation | Checks `ModelState.IsValid` and returns a standardized error response with field-level errors |

### 6.4 Models

| File | Role | Properties |
|------|------|------------|
| `Models/ApiResponse.cs` | Standard API response wrapper | `Success`, `Message`, `Data`, `Errors` |
| `Models/PaginatedResponse.cs` | Paginated API response | `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages` |

### 6.5 Configuration

| File | Role |
|------|------|
| `Program.cs` | Application entry point — configures services, middleware pipeline, Swagger, and starts the web host |
| `appsettings.json` | Main configuration — connection strings, logging, JWT settings |
| `appsettings.Development.json` | Development environment overrides |

### 6.6 Project File

| File | Role |
|------|------|
| `API.csproj` | Web API project referencing Application + Infrastructure + Swashbuckle |

---

## 7. Tests

### 7.1 Domain.Tests

| File | Role | Tests |
|------|------|-------|
| `Domain.Tests/ProductTests.cs` | Unit tests for Product entity | Creating product with valid data, zero price validation, updating details |
| `Domain.Tests/MoneyTests.cs` | Unit tests for Money value object | Creating money, addition (same/different currency), equality |

### 7.2 Application.Tests

| File | Role | Tests |
|------|------|-------|
| `Application.Tests/CreateProductCommandHandlerTests.cs` | Handler unit tests | Valid command creates product and returns ID (uses Mock) |
| `Application.Tests/GetProductsQueryHandlerTests.cs` | Query handler tests | Returns all products mapped to DTOs (uses Mock + AutoMapper) |

### 7.3 Infrastructure.Tests

| File | Role |
|------|------|
| `Infrastructure.Tests/ProductRepositoryTests.cs` | Integration tests for ProductRepository |
| `Infrastructure.Tests/EmailServiceTests.cs` | Unit tests for EmailService |

### 7.4 API.Tests

| File | Role |
|------|------|
| `API.Tests/ProductsControllerTests.cs` | Integration tests for ProductsController endpoints |
| `API.Tests/AuthControllerTests.cs` | Integration tests for AuthController endpoints |

---

## 8. Root Files

| File | Role |
|------|------|
| `Solution.sln` | Visual Studio Solution file referencing all 8 projects |
| `Directory.Build.props` | Common MSBuild properties shared across all projects (TargetFramework, Nullable, ImplicitUsings) |
| `docker-compose.yml` | Docker Compose file to run the API + PostgreSQL in containers |
| `Dockerfile` | Multi-stage Docker build file for the API project |
| `.gitignore` | Git ignore rules for .NET, Visual Studio, NuGet, build artifacts |
| `README.md` | Project overview, architecture diagram, setup instructions |
| `ARCHITECTURE.md` | This file — comprehensive architecture documentation |

---

## 9. Data Flow

```
Client → Controller → MediatR → Command/Query Handler → Repository → DbContext → Database
                ↓                        ↓                      ↓
        ValidationBehaviour         Domain Entity          SaveChanges
        LoggingBehaviour            Business Rules
```

### Request Flow (e.g., Create Product)

1. **Client** sends HTTP POST to `/api/products`
2. **ValidationActionFilter** checks ModelState
3. **ProductsController** sends `CreateProductCommand` to MediatR
4. **ValidationBehaviour** runs FluentValidation validators
5. **LoggingBehaviour** logs the request
6. **CreateProductCommandHandler** creates `Product` entity and calls `IRepository<Product>.AddAsync()`
7. **Repository** adds to DbSet and calls `SaveChangesAsync()`
8. **Response** flows back through the pipeline with the new product ID

---

## 10. Best Practices

### Domain Layer
- ❤️ Domain entities have **private setters** — state changes only through methods
- ❤️ Value Objects are **immutable** with equality based on all properties
- ❤️ Domain exceptions handle business rule violations
- ❤️ **Zero dependencies** on frameworks or external packages

### Application Layer
- ⚙️ **CQRS** — separate Commands (write) from Queries (read)
- ⚙️ **FluentValidation** — each command/query has its own validator
- ⚙️ **MediatR Behaviours** — cross-cutting concerns (validation, logging) as pipeline behaviors
- ⚙️ **Result Pattern** — handlers return `Result<T>` instead of throwing exceptions for business logic

### Infrastructure Layer
- 🛠️ **Repository Pattern** — abstracts data access behind interfaces defined in Domain
- 🛠️ **Unit of Work** — ensures atomic operations
- 🛠️ **Dependency Injection** — all services registered through extension methods

### Presentation Layer
- 🖥️ **Thin Controllers** — no business logic, only MediatR calls
- 🖥️ **Global Exception Handler** — centralized error handling
- 🖥️ **Standard API Response** — consistent response format across all endpoints

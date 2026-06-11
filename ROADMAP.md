# Architecture Roadmap: Clean Architecture SaaS

> **Project:** dotnet-architecture
> **Architecture:** Clean Architecture + CQRS
> **Target Framework:** .NET 10
> **Document Scope:** Three-phase evolution from starting point to production-ready SaaS

---

## Phase 1: Current State (Starting Point)

The repository establishes a four-layer Clean Architecture foundation with CQRS
pattern implementation. This phase represents the baseline from which all
enhancements are derived.

### Architecture Layers

```
Presentation (API)  -->  Application  -->  Domain
         |                       |
    Infrastructure  -------->  Application  -->  Domain
```

- Domain depends on nothing (pure .NET class library)
- Application depends only on Domain
- Infrastructure depends on Application + Domain
- Presentation depends on Application + Infrastructure

### Solution Structure

```
Solution.sln
src/
  Domain/                  Core business logic
  Application/             Use cases, CQRS, DTOs, validation
  Infrastructure/          EF Core, Identity, external services
  Presentation/API/        Controllers, middleware, filters
tests/
  Domain.Tests/            Entity and value object tests
  Application.Tests/       Handler unit tests
  Infrastructure.Tests/    Repository and service tests
  API.Tests/               Controller integration tests
```

### What Exists Today

| Component | Location | Status |
|-----------|----------|--------|
| Domain entities with encapsulated state | `src/Domain/Entities/` | Implemented |
| Value objects (Money, Address) | `src/Domain/ValueObjects/` | Implemented |
| Domain events | `src/Domain/Events/` | Implemented |
| CQRS commands/queries via MediatR | `src/Application/Features/` | Implemented |
| FluentValidation pipeline | `src/Application/Common/Behaviours/ValidationBehaviour.cs` | Implemented |
| Logging pipeline | `src/Application/Common/Behaviours/LoggingBehaviour.cs` | Implemented |
| Result pattern | `src/Application/Common/Models/Result.cs` | Implemented |
| Generic repository | `src/Infrastructure/Persistence/Repositories/Repository.cs` | Implemented (flawed) |
| Unit of Work | `src/Infrastructure/Persistence/Repositories/UnitOfWork.cs` | Implemented (bypassed) |
| ASP.NET Core Identity | `src/Infrastructure/Identity/` | Partially configured |
| EF Core with PostgreSQL | `src/Infrastructure/Persistence/` | Implemented |
| Global exception handler | `src/Presentation/API/Middlewares/GlobalExceptionHandlerMiddleware.cs` | Implemented |
| Request logging middleware | `src/Presentation/API/Middlewares/RequestLoggingMiddleware.cs` | Implemented |
| Validation action filter | `src/Presentation/API/Filters/ValidationActionFilter.cs` | Implemented |
| Standard API response model | `src/Presentation/API/Models/ApiResponse.cs` | Implemented |
| Swagger/OpenAPI | `src/Presentation/API/Program.cs` | Enabled (no auth) |
| Docker Compose | `docker-compose.yml` | Configured |
| Multi-stage Dockerfile | `Dockerfile` | Configured |

### Current Tech Stack

| Technology | Purpose |
|------------|---------|
| .NET 10 | Target framework |
| Entity Framework Core + Npgsql | ORM for PostgreSQL |
| MediatR | CQRS implementation |
| FluentValidation | Request validation |
| AutoMapper | Object mapping |
| ASP.NET Core Identity | Authentication scaffolding |
| BCrypt.Net | Password hashing |
| xUnit | Testing framework |

---

## Phase 2: Production-Ready SaaS Foundation

Phase 2 addresses all gaps between the current starting point and a deployable
SaaS application. Items are split into foundational (required before first
deploy) and operational (required for production reliability).

### Section A: Foundational Enhancements

These items must be completed before the application is deployed to any
environment, including staging.

---

#### A.1 Authentication and Authorization

**Current State:** The `AuthController` (`src/Presentation/API/Controllers/AuthController.cs`)
defines login, register, and refresh-token endpoints. The refresh-token endpoint
returns a stub response. No authentication middleware is configured in
`Program.cs:18` -- `UseAuthorization()` is called but `UseAuthentication()` is
missing. No `[Authorize]` attributes appear on any controller.

**Problem:** The API is fully open. All endpoints accept unauthenticated requests.

**Required Changes:**

1. Add JWT bearer authentication in `Program.cs`:
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options => {
           options.TokenValidationParameters = new TokenValidationParameters {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = configuration["Jwt:Issuer"],
               ValidAudience = configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(
                   Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
           };
       });
   ```

2. Add `app.UseAuthentication()` before `app.UseAuthorization()` in `Program.cs`.

3. Implement refresh-token logic in `AuthController` -- generate access +
   refresh tokens, store refresh token hash, validate on refresh.

4. Add `[Authorize]` to `ProductsController` and any other protected endpoints.

5. Configure Swagger for JWT in `Program.cs`:
   ```csharp
   builder.Services.AddSwaggerGen(options => {
       options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
           Name = "Authorization",
           Type = SecuritySchemeType.Http,
           Scheme = "bearer",
           BearerFormat = "JWT",
           In = ParameterLocation.Header,
           Description = "Enter your JWT token"
       });
       options.AddSecurityRequirement(new OpenApiSecurityRequirement {
           { new OpenApiSecurityScheme {
               Reference = new OpenApiReference {
                   Type = ReferenceType.SecurityScheme,
                   Id = "Bearer"
               }
           }, Array.Empty<string>() }
       });
   });
   ```

**Files Affected:**
- `src/Presentation/API/Program.cs:1-27`
- `src/Presentation/API/Controllers/AuthController.cs`
- `src/Presentation/API/appsettings.json` (Jwt section)

---

#### A.2 Multi-Tenancy

**Current State:** No tenant isolation exists. All queries return data across
all tenants. The `Product`, `User`, and `Order` entities have no `TenantId`
field.

**Problem:** A SaaS application must isolate customer data at the database level.
Without this, adding it later requires rewriting every query, migration, and
repository method.

**Required Changes:**

1. Add `TenantId` (Guid) to all business entities:
   ```csharp
   public class Product
   {
       public Guid TenantId { get; private set; }
       // ... existing properties
   }
   ```

2. Create `ICurrentTenantService` interface in `src/Application/Common/Interfaces/`:
   ```csharp
   public interface ICurrentTenantService
   {
       Guid? TenantId { get; }
   }
   ```

3. Implement tenant resolution from JWT claims or subdomain in
   `src/Infrastructure/Services/CurrentTenantService.cs`.

4. Add global query filter in `ApplicationDbContext.OnModelCreating`:
   ```csharp
   foreach (var entityType in modelBuilder.Model.GetEntityTypes())
   {
       if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
       {
           modelBuilder.Entity(entityType.ClrType)
               .HasQueryFilter(TenantFilter.GetFilter(entityType.ClrType));
       }
   }
   ```

5. Auto-set `TenantId` on entity creation via `SaveChanges` interceptor.

**Files Affected:**
- All entities in `src/Domain/Entities/`
- `src/Infrastructure/Persistence/ApplicationDbContext.cs`
- New: `src/Application/Common/Interfaces/ICurrentTenantService.cs`
- New: `src/Infrastructure/Services/CurrentTenantService.cs`

---

#### A.3 Unit of Work Atomicity Fix

**Current State:** `Repository<T>` (`src/Infrastructure/Persistence/Repositories/Repository.cs:22-36`)
calls `SaveChangesAsync()` inside `AddAsync`, `UpdateAsync`, and `DeleteAsync`.
`UnitOfWork.SaveChangesAsync()` exists but is never called by handlers -- each
repository operation commits independently.

```csharp
// Repository.cs:22-24
public async Task AddAsync(T entity)
{
    await _context.Set<T>().AddAsync(entity);
    await _context.SaveChangesAsync();  // premature commit
}
```

**Problem:** Multiple repository calls in a single use case produce multiple
independent transactions. If the second call fails, the first is already
committed. This breaks atomicity and violates the Unit of Work pattern.

**Required Changes:**

1. Remove `SaveChangesAsync()` from all three methods in `Repository<T>`:
   ```csharp
   public async Task AddAsync(T entity)
   {
       await _context.Set<T>().AddAsync(entity);
       // SaveChanges removed -- committed by UnitOfWork
   }

   public void Update(T entity)
   {
       _context.Set<T>().Update(entity);
       // SaveChanges removed
   }

   public void Delete(T entity)
   {
       _context.Set<T>().Remove(entity);
       // SaveChanges removed
   }
   ```

2. Update handlers to call `IUnitOfWork.SaveChangesAsync()` explicitly:
   ```csharp
   // CreateProductCommandHandler.cs
   public async Task<Result<int>> Handle(CreateProductCommand request, ...)
   {
       var product = new Product(request.Name, request.Description, request.Price);
       await _repository.AddAsync(product);
       await _unitOfWork.SaveChangesAsync(cancellationToken);  // single commit
       return Result<int>.Success(product.Id);
   }
   ```

3. Register `IUnitOfWork` in handler constructors alongside repositories.

**Files Affected:**
- `src/Infrastructure/Persistence/Repositories/Repository.cs:22-36`
- All command handlers in `src/Application/Features/`
- `src/Domain/Interfaces/IUnitOfWork.cs`

---

#### A.4 Soft Delete and Audit Trail

**Current State:** `DeleteAsync` in `Repository<T>` removes rows permanently.
No `CreatedAt`, `UpdatedAt`, `CreatedBy`, or `DeletedAt` tracking exists on
any entity.

**Problem:** SaaS data retention policies, compliance requirements, and
debugging workflows require soft deletion and audit metadata.

**Required Changes:**

1. Create `AuditableEntity` base class in `src/Domain/Common/`:
   ```csharp
   public abstract class AuditableEntity
   {
       public DateTime CreatedAt { get; set; }
       public string? CreatedBy { get; set; }
       public DateTime? UpdatedAt { get; set; }
       public string? UpdatedBy { get; set; }
   }
   ```

2. Create `SoftDeletableEntity` extending `AuditableEntity`:
   ```csharp
   public abstract class SoftDeletableEntity : AuditableEntity
   {
       public bool IsDeleted { get; set; }
       public DateTime? DeletedAt { get; set; }
       public string? DeletedBy { get; set; }
   }
   ```

3. Update entities to inherit from `SoftDeletableEntity`.

4. Add global query filter for soft delete:
   ```csharp
   // ApplicationDbContext.cs
   foreach (var entityType in modelBuilder.Model.GetEntityTypes())
   {
       if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
       {
           var parameter = Expression.Parameter(entityType.ClrType, "e");
           var property = Expression.Property(parameter, "IsDeleted");
           var filter = Expression.Lambda(
               Expression.Equal(property, Expression.Constant(false)),
               parameter);
           modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
       }
   }
   ```

5. Override `SaveChangesAsync` in `ApplicationDbContext` to auto-set audit
   fields and convert hard deletes to soft deletes.

**Files Affected:**
- New: `src/Domain/Common/AuditableEntity.cs`
- New: `src/Domain/Common/SoftDeletableEntity.cs`
- All entities in `src/Domain/Entities/`
- `src/Infrastructure/Persistence/ApplicationDbContext.cs`

---

#### A.5 CORS Configuration

**Current State:** `Program.cs` contains no `AddCors()` or `UseCors()` calls.

**Problem:** If a frontend application (SPA, mobile app) consumes this API,
browsers will block cross-origin requests.

**Required Changes:**

1. Add CORS policy in `Program.cs`:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("SaaSPolicy", policy =>
       {
           policy.WithOrigins(configuration.GetSection("Cors:Origins").Get<string[]>())
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
       });
   });
   ```

2. Add allowed origins to `appsettings.json`:
   ```json
   "Cors": {
       "Origins": ["https://app.yourdomain.com", "https://admin.yourdomain.com"]
   }
   ```

3. Add `app.UseCors("SaaSPolicy")` before `app.UseAuthorization()`.

**Files Affected:**
- `src/Presentation/API/Program.cs`
- `src/Presentation/API/appsettings.json`

---

#### A.6 Rate Limiting

**Current State:** No rate-limit middleware exists.

**Problem:** Without rate limiting, the API is vulnerable to abuse, DDoS, and
resource exhaustion.

**Required Changes:**

1. Add rate limiting in `Program.cs`:
   ```csharp
   builder.Services.AddRateLimiter(options =>
   {
       options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

       options.AddFixedWindowLimiter("fixed", opt =>
       {
           opt.PermitLimit = 100;
           opt.Window = TimeSpan.FromMinutes(1);
       });

       options.AddSlidingWindowLimiter("sliding", opt =>
       {
           opt.PermitLimit = 50;
           opt.Window = TimeSpan.FromMinutes(1);
           opt.SegmentsPerWindow = 4;
       });
   });
   ```

2. Apply rate limiting to endpoints:
   ```csharp
   [EnableRateLimiting("fixed")]
   [HttpPost]
   public async Task<ActionResult<int>> Create(CreateProductCommand command)
   ```

3. Add `app.UseRateLimiter()` before `app.MapControllers()`.

**Files Affected:**
- `src/Presentation/API/Program.cs`
- Controllers in `src/Presentation/API/Controllers/`

---

#### A.7 Domain Layer Interface Cleanup

**Current State:** `IRepository<T>` and `IUnitOfWork` are defined in
`src/Domain/Interfaces/`. The Domain layer references these infrastructure
concepts.

**Problem:** Clean Architecture mandates that the Domain layer depends on
nothing. Repository and Unit of Work are persistence concerns that belong in
the Application layer's interface contract.

**Required Changes:**

1. Move `src/Domain/Interfaces/IRepository.cs` to
   `src/Application/Common/Interfaces/IRepository.cs`.

2. Move `src/Domain/Interfaces/IUnitOfWork.cs` to
   `src/Application/Common/Interfaces/IUnitOfWork.cs`.

3. Update namespace references in:
   - `src/Infrastructure/Persistence/Repositories/Repository.cs`
   - `src/Infrastructure/Persistence/Repositories/UnitOfWork.cs`
   - All handlers that reference `IRepository<T>` or `IUnitOfWork`
   - `src/Infrastructure/DependencyInjection.cs`

4. Domain layer should only contain entities, value objects, enums, events,
   exceptions, and domain-specific interfaces (e.g., `IOrderPricingService`).

**Files Affected:**
- `src/Domain/Interfaces/IRepository.cs` (moved)
- `src/Domain/Interfaces/IUnitOfWork.cs` (moved)
- `src/Infrastructure/Persistence/Repositories/Repository.cs:1`
- `src/Infrastructure/Persistence/Repositories/UnitOfWork.cs:1`
- `src/Infrastructure/DependencyInjection.cs`
- All command/query handlers

---

### Section B: Operational Enhancements

These items are required for production reliability, observability, and
maintainability. They should be completed before the first production deployment
but do not block staging deployments.

---

#### B.1 Structured Logging

**Current State:** `LoggingBehaviour` (`src/Application/Common/Behaviours/LoggingBehaviour.cs:26-30`)
logs only the request name with `LogInformation`. No correlation IDs, no
duration tracking, no structured properties.

```csharp
// LoggingBehaviour.cs:26-30
_logger.LogInformation("Processing request: {RequestName}", requestName);
var response = await next();
_logger.LogInformation("Completed request: {RequestName}", requestName);
```

**Problem:** Production debugging requires correlating requests across services,
measuring handler duration, and filtering by structured properties. Basic
`LogInformation` with string interpolation does not support this.

**Required Changes:**

1. Add Serilog to the project:
   ```csharp
   // Program.cs
   builder.Host.UseSerilog((context, config) =>
   {
       config.ReadFrom.Configuration(context.Configuration)
           .Enrich.FromLogContext()
           .Enrich.WithProperty("Application", "GymManagementSystem")
           .WriteTo.Console()
           .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
   });
   ```

2. Enhance `LoggingBehaviour` with duration and correlation ID:
   ```csharp
   public async Task<TResponse> Handle(TRequest request, ...)
   {
       var requestName = typeof(TRequest).Name;
       var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
       var stopwatch = Stopwatch.StartNew();

       using (LogContext.PushProperty("CorrelationId", correlationId))
       using (LogContext.PushProperty("RequestName", requestName))
       {
           _logger.LogInformation("Processing request");
           var response = await next();
           stopwatch.Stop();

           _logger.LogInformation(
               "Completed request in {ElapsedMs}ms",
               stopwatch.ElapsedMilliseconds);

           return response;
       }
   }
   ```

3. Add `System.Diagnostics` using to `LoggingBehaviour.cs`.

**Files Affected:**
- `src/Application/Common/Behaviours/LoggingBehaviour.cs:1-31`
- `src/Presentation/API/Program.cs`
- `src/Presentation/API/appsettings.json`

---

#### B.2 Health Checks

**Current State:** No health check endpoints exist.

**Problem:** Load balancers, orchestrators, and monitoring systems require health
endpoints to determine service availability.

**Required Changes:**

1. Add health check services in `Program.cs`:
   ```csharp
   builder.Services.AddHealthChecks()
       .AddNpgSql(configuration.GetConnectionString("DefaultConnection"),
           name: "postgresql")
       .AddCheck<self>("self");
   ```

2. Map health check endpoints:
   ```csharp
   app.MapHealthChecks("/health", new HealthCheckOptions {
       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
   });
   app.MapHealthChecks("/ready", new HealthCheckOptions {
       Predicate = check => check.Tags.Contains("ready")
   });
   ```

**Files Affected:**
- `src/Presentation/API/Program.cs`

---

#### B.3 Distributed Tracing

**Current State:** No OpenTelemetry or distributed tracing configuration.

**Problem:** In a microservice or scaled deployment, requests span multiple
services. Without tracing, identifying failure points requires manual log
correlation.

**Required Changes:**

1. Add OpenTelemetry packages and configure in `Program.cs`:
   ```csharp
   builder.Services.AddOpenTelemetry()
       .ConfigureResource(resource => resource.AddService("GymManagementSystem"))
       .WithTracing(tracing => tracing
           .AddAspNetCoreInstrumentation()
           .AddEntityFrameworkCoreInstrumentation()
           .AddOtlpExporter())
       .WithMetrics(metrics => metrics
           .AddAspNetCoreInstrumentation()
           .AddOtlpExporter());
   ```

2. Configure OTLP endpoint via environment variable or `appsettings.json`.

**Files Affected:**
- `src/Presentation/API/Program.cs`
- New NuGet packages: `OpenTelemetry.Extensions.Hosting`,
  `OpenTelemetry.Instrumentation.AspNetCore`,
  `OpenTelemetry.Exporter.OpenTelemetryProtocol`

---

#### B.4 Redis Caching

**Current State:** No caching layer exists. Every query hits PostgreSQL directly.

**Problem:** Read-heavy endpoints (product listing, user profiles) load the
entire table into memory on every request. At scale, this creates unnecessary
database load.

**Required Changes:**

1. Add Redis distributed cache:
   ```csharp
   builder.Services.AddStackExchangeRedisCache(options =>
   {
       options.Configuration = configuration.GetConnectionString("Redis");
       options.InstanceName = "GymManagementSystem_";
   });
   ```

2. Implement cache-aside pattern in query handlers:
   ```csharp
   public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, ...)
   {
       var cacheKey = "products:all";
       var cached = await _cache.GetStringAsync(cacheKey);
       if (cached != null)
           return JsonSerializer.Deserialize<List<ProductDto>>(cached);

       var products = await _repository.GetAllAsync();
       var dtos = _mapper.Map<List<ProductDto>>(products);
       await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dtos),
           new DistributedCacheEntryOptions {
               AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
           });
       return dtos;
   }
   ```

3. Invalidate cache on write operations (Create, Update, Delete).

**Files Affected:**
- `src/Presentation/API/Program.cs`
- Query handlers in `src/Application/Features/`
- Command handlers (cache invalidation)

---

#### B.5 API Versioning

**Current State:** All endpoints are unversioned (`/api/products`).

**Problem:** Breaking changes require coordinating all API consumers
simultaneously. No ability to deprecate old versions gracefully.

**Required Changes:**

1. Add API versioning:
   ```csharp
   builder.Services.AddApiVersioning(options =>
   {
       options.DefaultApiVersion = new ApiVersion(1, 0);
       options.AssumeDefaultVersionWhenUnspecified = true;
       options.ReportApiVersions = true;
   })
   .AddApiExplorer(options =>
   {
       options.GroupNameFormat = "'v'VVV";
       options.SubstituteApiVersionInUrl = true;
   });
   ```

2. Version controllers:
   ```csharp
   [ApiVersion("1.0")]
   [ApiController]
   [Route("api/v{version:apiVersion}/[controller]")]
   public class ProductsController : ControllerBase
   ```

**Files Affected:**
- `src/Presentation/API/Program.cs`
- All controllers in `src/Presentation/API/Controllers/`

---

#### B.6 Background Job Processing

**Current State:** No background task infrastructure exists.

**Problem:** SaaS workloads require async processing (email queues, report
generation, webhook retries, subscription renewal checks).

**Required Changes:**

1. Add Hangfire:
   ```csharp
   builder.Services.AddHangfire(config => config
       .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")));

   builder.Services.AddHangfireServer();
   ```

2. Map Hangfire dashboard:
   ```csharp
   app.MapHangfireDashboard("/hangfire");
   ```

3. Define recurring jobs for SaaS operations (e.g., subscription expiry checks).

**Files Affected:**
- `src/Presentation/API/Program.cs`
- New: `src/Infrastructure/Jobs/` directory

---

#### B.7 Swagger JWT Configuration

**Current State:** Swagger is enabled (`Program.cs:14-17`) but not configured
for JWT bearer token testing.

**Problem:** Developers cannot authenticate against Swagger UI during local
development and testing.

**Required Changes:** Covered in A.1 (Step 5). This item is tracked separately
as it is an operational convenience rather than a security requirement.

**Files Affected:**
- `src/Presentation/API/Program.cs:14-17`

---

## Phase 3: Scale and Reliability

Phase 3 items are required when the application reaches production traffic
levels and needs horizontal scaling, fault tolerance, and operational maturity.

---

#### C.1 Message Queue Integration

**Current State:** All operations are synchronous request-response.

**Problem:** High-throughput SaaS workloads require asynchronous processing
to decouple producers from consumers and handle traffic spikes.

**Required Changes:**

1. Integrate RabbitMQ or Azure Service Bus as a message broker.
2. Define message contracts in `src/Application/Common/Messages/`.
3. Implement publisher in `src/Infrastructure/Messaging/`.
4. Implement consumer workers as `IHostedService` or Hangfire jobs.
5. Add retry and dead-letter queue policies.

---

#### C.2 Distributed Caching

**Current State:** Redis is used as a simple key-value cache (Phase 2).

**Problem:** As the application scales across multiple instances, cache
invalidation and consistency become critical.

**Required Changes:**

1. Implement cache invalidation strategies (pub/sub via Redis channels).
2. Add cache warming for high-traffic endpoints.
3. Consider distributed cache patterns (e.g., Cache-Aside with versioning).

---

#### C.3 Request/Response Compression

**Current State:** No response compression middleware.

**Problem:** Large JSON payloads consume bandwidth and increase latency.

**Required Changes:**

1. Add Brotli/gzip compression:
   ```csharp
   builder.Services.AddResponseCompression(options =>
   {
       options.EnableForHttps = true;
   });
   ```

2. Configure compression levels for different content types.

---

#### C.4 Per-Tenant Rate Limiting

**Current State:** Global rate limiting (Phase 2).

**Problem:** A single tenant consuming excessive resources affects all other
tenants.

**Required Changes:**

1. Implement per-tenant rate limiting using tenant ID from JWT claims.
2. Define tier-based limits (free, pro, enterprise).
3. Store rate-limit counters in Redis with tenant-scoped keys.

---

#### C.5 CI/CD Pipeline

**Current State:** No automation for build, test, or deployment.

**Problem:** Manual deployments are error-prone and slow.

**Required Changes:**

1. Define GitHub Actions workflow:
   - Build and restore on push/PR
   - Run unit and integration tests
   - Run linting and static analysis
   - Build Docker image
   - Deploy to staging on main branch merge
   - Deploy to production on tag/release

2. Add database migration step in pipeline.

3. Add security scanning (dependency audit, SAST).

---

#### C.6 Container Orchestration

**Current State:** Docker Compose for local development only.

**Problem:** Production deployments require health management, auto-scaling,
and service discovery.

**Required Changes:**

1. Create Kubernetes manifests (Deployment, Service, Ingress).
2. Configure Horizontal Pod Autoscaler based on CPU/memory.
3. Add ConfigMaps and Secrets for environment-specific configuration.
4. Implement liveness and readiness probes using Phase 2 health endpoints.

---

## Appendix: Current Gaps Summary

| Category | Gap | Phase | Priority |
|----------|-----|-------|----------|
| Security | No authentication middleware | A.1 | P0 |
| Security | No `[Authorize]` on endpoints | A.1 | P0 |
| Security | No CORS configuration | A.5 | P0 |
| Security | No rate limiting | A.6 | P0 |
| Architecture | UoW atomicity broken | A.3 | P0 |
| Architecture | Domain layer contains persistence interfaces | A.7 | P0 |
| Data | No multi-tenancy | A.2 | P0 |
| Data | No soft delete | A.4 | P0 |
| Data | No audit trail | A.4 | P0 |
| Observability | Basic logging only | B.1 | P1 |
| Observability | No health checks | B.2 | P1 |
| Observability | No distributed tracing | B.3 | P1 |
| Performance | No caching | B.4 | P1 |
| API Design | No versioning | B.5 | P1 |
| Operations | No background jobs | B.6 | P1 |
| DevOps | No CI/CD pipeline | C.5 | P2 |
| DevOps | No Kubernetes orchestration | C.6 | P2 |
| Performance | No compression | C.3 | P2 |
| Performance | No per-tenant rate limiting | C.4 | P2 |
| Architecture | No message queue | C.1 | P2 |

---

## Appendix: File Reference Index

| File | Phase | Enhancement |
|------|-------|-------------|
| `src/Presentation/API/Program.cs` | A.1, A.5, A.6, B.1, B.2, B.3, B.5, B.6 | Auth, CORS, Rate Limit, Logging, Health, Tracing, Versioning, Jobs |
| `src/Infrastructure/Persistence/Repositories/Repository.cs:22-36` | A.3 | Remove premature SaveChanges |
| `src/Application/Common/Behaviours/LoggingBehaviour.cs:26-30` | B.1 | Add correlation ID, duration |
| `src/Domain/Interfaces/IRepository.cs` | A.7 | Move to Application layer |
| `src/Domain/Interfaces/IUnitOfWork.cs` | A.7 | Move to Application layer |
| `src/Presentation/API/Controllers/AuthController.cs` | A.1 | Implement refresh token |
| `src/Presentation/API/Controllers/ProductsController.cs` | A.1 | Add [Authorize] |
| `src/Infrastructure/Persistence/ApplicationDbContext.cs` | A.2, A.4 | Tenant filter, soft delete filter |
| `src/Application/Common/Models/Result.cs` | -- | No changes required |

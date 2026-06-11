# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Sentinal.Api** is a secure file sharing API built as a take-home interview project. The project demonstrates a production-grade architecture using clean architecture principles with CQRS pattern and MediatR for command/query handling. 

**Note on Transparency**: This project includes a transcript export of all development discussions for the hiring company to review. All architectural decisions are documented in CLAUDE.md and TODO.md to provide maximum transparency into the development approach and thinking process.

## Building and Running

### Prerequisites
- .NET 10.0 SDK must be installed
- PostgreSQL 12+ (for database layer)
- Docker (for containerized deployment)
- Use Rider, Visual Studio, or `dotnet` CLI

### Build
```bash
dotnet build
```

### Run
```bash
# Run with HTTP (default development profile)
dotnet run --launch-profile http

# Run with HTTPS
dotnet run --launch-profile https

# Run without specifying a profile (uses the first one)
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5230`
- HTTPS: `https://localhost:7096`

### Run Tests
```bash
# Run all XUnit tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity detailed

# Run specific test project
dotnet test Sentinal.Api.Tests
```

### Access OpenAPI/Swagger
When running in development mode, OpenAPI documentation is available at:
- `http://localhost:5230/openapi/v1.json`

### Database Migrations
```bash
# Apply pending migrations
dotnet ef database update

# Create a new migration
dotnet ef migrations add MigrationName
```

## Architecture

Sentinal.Api follows **Clean Architecture** principles organized in distinct layers with **CQRS pattern** for command/query separation using **MediatR**.

### Directory Structure (Current)

```
Sentinal/
├── src/
│   ├── Sentinal.Domain/                # Domain layer - entities, aggregates, value objects
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   ├── Sentinal.Application/           # Application layer - CQRS commands, queries, handlers
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Handlers/
│   │   ├── DTOs/
│   │   └── Interfaces/
│   ├── Sentinal.Infrastructure/        # Infrastructure layer - database, repositories, external services
│   │   ├── Data/
│   │   ├── Repositories/
│   │   └── Services/
│   ├── Sentinal.Api/                   # Presentation layer - controllers, HTTP entry points
│   │   ├── Controllers/
│   │   │   ├── FoldersController.cs
│   │   │   └── FilesController.cs
│   │   ├── Program.cs                  # Startup and dependency injection
│   │   ├── appsettings.json            # Logging and configuration
│   │   └── Properties/launchSettings.json
│   └── Sentinal.Tests/                 # Presentation, Domain, Application, Infrastructure tests
├── Sentinal.slnx                       # Solution file
├── CLAUDE.md                           # This file - development guidance
├── TODO.md                             # Project tasks and roadmap
└── ai-usage/                           # AI usage and session transcripts
```

**Structure Details**:
- Each layer is organized as a separate .NET project/assembly for clear separation of concerns
- Domain, Application, and Infrastructure layers are library projects (no HTTP exposure)
- Sentinal.Api is the presentation/entry point (ASP.NET Core Web SDK)
- Test projects mirror the architecture for testing each layer

### Architectural Principles

**Clean Architecture Layers**:
1. **Domain Layer**: Core business logic, entities, aggregates, and domain rules (no dependencies on other layers)
2. **Application Layer**: Use cases, CQRS commands and queries, handlers, and DTOs (depends only on Domain)
3. **Infrastructure Layer**: Database access, repositories, external service implementations (depends on Application and Domain)
4. **Presentation Layer**: API controllers, request/response models (depends on Application)

**CQRS Pattern with MediatR**:
- **Commands**: Write operations (Create, Update, Delete) - return nothing or aggregate ID
- **Queries**: Read operations (GetById, GetAll) - return DTOs
- **Handlers**: Implement command/query logic using MediatR
- All commands and queries flow through MediatR pipeline for cross-cutting concerns

### Current Phase 1 Scope (80% Complete)

**Controllers** (Presentation Layer):
- `UserController` - Authentication and user management (Register & Login fully wired)
- `FoldersController` - Manage folder hierarchies (CRUD operations, awaiting CQRS wiring)
- `FilesController` - Manage file operations (CRUD operations, awaiting CQRS wiring)

**Domain Models** (Entities):
- **UserEntity**: User authentication and file/folder ownership
  - `Id` (Guid), `Username` (string, 255 max, unique), `Email` (string, 255 max, unique)
  - `PasswordHash` (string, 512 max), `EmailConfirmed` (bool), `TwoFactorEnabled` (bool)
  - Soft delete: `MarkedForDeletion`, `DeletedAt` (7-day retention before permanent deletion)
  - Navigation: `Files`, `Folders`

- **FolderEntity**: Hierarchical folder structure
  - `Id` (Guid), `FolderName` (string, 255 max), `ParentFolderId` (Guid, nullable for root)
  - `UserId` (Guid), timestamps, soft delete fields
  - Navigation: `Parent` (self-reference), `Children` (collection), `Files`

- **FileEntity**: File storage and metadata
  - `Id` (Guid), `FileName` (string, 1000 max for long names), `FileSize` (long)
  - `ContentType` (string, 255 max), `FolderId` (Guid), `UserId` (Guid)
  - Timestamps, soft delete fields
  - Navigation: `Folder`

**Database**: PostgreSQL with Entity Framework Core
- Entity Configurations: All entities have proper constraints, indexes, and relationships
- Future: pgvector extension for semantic search capabilities

## Security & Authentication

**Password Hashing**:
- Algorithm: **Argon2i** (memory-hard, resistant to GPU/ASIC attacks)
- Parameters: 2 iterations, 8MB memory, parallelism 2 (~300-400ms per hash)
- Salt: 16 bytes (128 bits), randomly generated per password
- Implementation: `Argon2PasswordService` implementing `IPasswordHasher`
- Service is registered in Infrastructure DI layer

**File Storage Security**:
- Storage path structure: `/{userId}/{folderId}/{fileId}` (all GUIDs)
- GUID-based paths prevent naming conflicts and provide security through obscurity
- User-provided filenames stored in database, not filesystem
- Multiple storage provider support (Local, AWS S3, Azure Blob) via `IFileStorageService`
- Configuration-driven via `FileStorageOptions` with `StorageType` enum
- Future: AES-256 encryption at rest (post-submission enhancement)

**Soft Deletes**:
- All entities support soft deletes: `MarkedForDeletion` (bool) and `DeletedAt` (DateTime?)
- 7-day retention period before permanent deletion (configured in `FileStorageOptions.DeletedFileRetentionDays`)
- Soft-delete cleanup service planned (runs daily, removes items > 7 days old)
- Prevents accidental data loss and maintains audit trail

**JWT Authentication** (Phase 1 - Complete):
- ✅ Token generation in `LoginUserCommand` after successful password verification via `JwtTokenService`
- ✅ Claims include: userId (custom), ClaimTypes.Name, ClaimTypes.Email
- ✅ `JwtTokenService` provides `GenerateToken()` and `ValidateToken()` methods
- ✅ Algorithm: HmacSha256 with SymmetricSecurityKey from `Jwt:Key` in appsettings.json
- ✅ Issuer and Audience validation configured
- ✅ Token expiration: 120 minutes
- ✅ Bearer token middleware configured in `Program.cs` with `AddAuthentication()` and `AddJwtBearer()`
- ✅ TokenValidationParameters set up in Program.cs for ASP.NET Core validation pipeline
- ⏳ Next: Add [Authorize] attributes to Folder/File controllers
- ⏳ Next: Extract UserId from JWT claims in Folder/File handlers for authorization

**Future Authentication**:
- 2FA support flagged in UserEntity (`TwoFactorEnabled`)
- Plan: TOTP (Time-based One-Time Password) + email-based 2FA
- Passkey/WebAuthn support (FIDO2) for Bitwarden integration (Phase 2)

### Error Handling

**FluentResults Pattern**:
- All repository and service methods return `Result<T>` instead of throwing exceptions
- Exceptions reserved for truly exceptional conditions only
- Benefits: Composable, chainable, multiple errors in single result
- Controllers map results to HTTP responses (200 Ok, 400 BadRequest, 404 NotFound, etc.)
- Example:
  ```csharp
  public async Task<Result<Guid>> CreateUserAsync(RegisterUserCommand cmd)
  {
      if (string.IsNullOrEmpty(cmd.Username))
          return Result.Fail("Username is required");
      
      var user = new UserEntity { /* ... */ };
      return Result.Ok(user.Id);
  }
  ```

### Configuration

**appsettings.json**: Configures logging levels and file storage options:
- Default log level: Information
- ASP.NET Core framework logs: Warning (reduces noise)
- FileStorage section: StorageProvider, MaxFileSizeBytes, BasePath, provider-specific settings

**FileStorageOptions** (`Infrastructure/Options/FileStorageOptions.cs`):
- `StorageProvider`: StorageType enum (Local, AwsS3, AzureBlob)
- `MaxFileSizeBytes`: 5GB default
- `BasePath`: Local storage path (default: "./Sentinal")
- `DeletedFileRetentionDays`: 7 (before permanent deletion)
- AWS S3 options: AccessKey, SecretKey, Region, BucketName
- Azure options: ConnectionString, ContainerName

**launchSettings.json**: Defines two launch profiles (http and https) for local development:
- Both disable automatic browser launching
- Both set `ASPNETCORE_ENVIRONMENT` to Development
- HTTP profile: port 5230
- HTTPS profile: ports 7096 (secure) and 5230 (fallback)

## Dependencies

**Application Layer**:
- **MediatR** (v14.1.0) - CQRS command/query mediator pattern
- **FluentResults** (v4.0.0) - Result<T> pattern for error handling without exceptions

**Infrastructure Layer**:
- **Npgsql.EntityFrameworkCore.PostgreSQL** (v10.0.2) - PostgreSQL database provider
- **Pgvector.EntityFrameworkCore** (v0.3.0) - Vector support for semantic search (future)
- **Konscious.Security.Cryptography.Argon2** (v1.3.1) - Argon2i password hashing

**Presentation Layer**:
- **Microsoft.AspNetCore.OpenApi** (v10.0.8) - OpenAPI specification support
- **Scalar.AspNetCore** (v1.2.48) - Beautiful API documentation UI

**Testing**:
- **xUnit** (v2.9.3) - Unit testing framework
- **xUnit.runner.visualstudio** (v3.1.5) - Visual Studio test runner
- **Moq** (v4.20.72) - Mocking library for unit tests

All other ASP.NET Core features are included via the Web SDK (`Microsoft.NET.Sdk.Web`).

## Code Style and Settings

- **Language Version**: C# 12 (implicit with .NET 10)
- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
  - These automatically include common namespaces like `System`, `System.Collections.Generic`, `Microsoft.AspNetCore.Mvc`, etc.
- **Target Framework**: .NET 10.0

## Git & Version Control

**IMPORTANT**: Claude Code cannot execute any git commands (`git commit`, `git push`, `git branch`, etc.). The developer (you) must:
- Review all code changes
- Make all git commits after approval
- Push to the local Gitea server
- Manage replication to GitHub for offsite backup

This ensures you maintain full control and visibility over all repository changes for the interview submission.

## Testing the API

Use the included `Sentinal.Api.http` file with REST Client plugins (available in Rider, VS Code, etc.) or use curl:

```bash
# Get weather forecasts
curl http://localhost:5230/weatherforecast

# Pretty-print with jq (if installed)
curl http://localhost:5230/weatherforecast | jq
```

## CI/CD Pipeline

The project uses a two-repository strategy for version control:

1. **Primary**: Local Gitea server with configured runners
   - Handles automated builds, tests, and Docker image creation
   - Serves as the source of truth for the project
   
2. **Backup**: GitHub repository
   - Receives replicated code for offsite backup
   - Acts as a secondary remote

**Push Workflow**:
1. Code changes are committed to local Gitea
2. Gitea runners automatically build, test, and publish Docker images
3. Code is then replicated to GitHub for backup

## Common Development Tasks

### Adding a New CQRS Command
1. Create command class in `Application/Commands/`
2. Implement handler in `Application/Handlers/`
3. Register handler in MediatR via dependency injection
4. Call command from controller using MediatR `_mediator.Send()`

### Adding a New CQRS Query
1. Create query class in `Application/Queries/` returning a DTO
2. Implement handler in `Application/Handlers/`
3. Register handler in MediatR via dependency injection
4. Call query from controller using MediatR `_mediator.Send()`

### Adding a New Endpoint
1. Add action method to appropriate controller (FoldersController or FilesController)
2. Decorate with HTTP verb attribute (`[HttpGet]`, `[HttpPost]`, etc.)
3. Use MediatR to dispatch command or query
4. Return appropriate HTTP response with DTO
5. Endpoint will be automatically documented in OpenAPI

### Adding Domain Models
1. Create entity or value object in `Domain/Entities/` or `Domain/ValueObjects/`
2. Implement aggregate root pattern where applicable
3. Define necessary constructors and methods
4. Create database mapping in Infrastructure layer (implement `IEntityTypeConfiguration<T>`)

### File Storage Implementation
1. Create implementation of `IFileStorageService` in `Infrastructure/Services/`
2. Example: `LocalFileStorageService`, `S3FileStorageService`, `AzureFileStorageService`
3. Constructor takes `IOptions<FileStorageOptions>`
4. Implement methods using the configured `StorageProvider`
5. Register in Infrastructure DI: `services.AddScoped<IFileStorageService, ConcreteImplementation>();`

### Password Hashing
1. Use `Argon2PasswordService` for all password operations
2. Inject `IPasswordHasher` into authentication handlers/commands
3. For registration: `var hash = _passwordHasher.HashPassword(plainPassword);`
4. For login: `var isValid = _passwordHasher.VerifyPassword(plainPassword, storedHash);`

### User Authentication Implementation (Reference)
The User CQRS implementation (`RegisterUserCommand` and `LoginUserCommand`) provides a reference pattern for wiring commands in controllers:

**1. Define Request DTOs** (`Api/Models/Requests/`):
```csharp
public class RegisterUserRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Username { get; set; } = null!;
    // ... other properties with validation attributes
}
```

**2. Create CQRS Command** (`Application/Users/{Feature}/`):
```csharp
public record RegisterUserCommand(string Username, string Email, string Password) 
    : IRequest<Result<RegisterUserDto>>;
```

**3. Implement Handler with Logging**:
```csharp
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public async Task<Result<RegisterUserDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Validation, business logic, error handling
        _logger.LogError(e, "Error creating user with username {Username}", command.Username);
        return Result.Ok(new RegisterUserDto(...));
    }
}
```

**4. Wire in Controller**:
```csharp
[HttpPost("register")]
public async Task<ActionResult<RegisterUserDto>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
{
    var command = new RegisterUserCommand(request.Username, request.Email, request.Password);
    var commandResult = await _mediator.Send(command, ct);
    if(commandResult.IsSuccess)
        return Ok(commandResult.Value);
    return BadRequest(commandResult.Errors);
}
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage (if coverlet installed)
dotnet test /p:CollectCoverage=true
```

## Architecture Patterns & Conventions

**Repository Pattern**:
- All data access through repository interfaces defined in `Application/Common/Interfaces/`
- Implementations in `Infrastructure/{Entity}/Persistence/`
- Methods return `Result<T>` (FluentResults pattern)
- Repositories handle EF Core `SaveChangesAsync()` internally

**CQRS with MediatR**:
- Commands in `Application/Commands/` for write operations
- Queries in `Application/Queries/` for read operations
- Handlers in `Application/Handlers/` returning `Result<T>`
- Controllers inject `IMediator` and dispatch via `await _mediator.Send(command)`

**Dependency Injection**:
- Application layer: `AddApplication()` registers MediatR
- Infrastructure layer: `AddInfrastructure(config)` registers DbContext, repositories, services
- API layer: Both called in `Program.cs`
- Each layer has a `DependencyInjection.cs` extension method

**Entity Configuration**:
- All EF Core mappings in `IEntityTypeConfiguration<T>` implementations
- Configurations auto-discovered by `modelBuilder.ApplyConfigurationsFromAssembly()`
- Define constraints, indexes, relationships, and delete behavior

## Notes for Future Developers

- This project uses modern C# idioms (nullable reference types, implicit usings, property initialization)
- All repository and service methods return `Result<T>` (not exceptions for expected errors)
- Password hashing is centralized in `Argon2PasswordService`—never hash passwords in command handlers
- File storage is abstracted—add new providers by implementing `IFileStorageService`
- Soft deletes are enforced across all entities—use MarkedForDeletion before DeleteFileAsync
- The GUID-based file path structure prevents naming conflicts and maintains security
- Entity relationships are configured with `OnDelete(DeleteBehavior.Restrict)` to prevent orphaning
- MediatR registration auto-discovers handlers in the Application assembly
- All command/query handlers should inject `ILogger<T>` for audit trails and debugging (see User CQRS pattern)
- Controller endpoints map request DTOs → commands/queries → handlers → response DTOs (see UserController for reference)
- JWT is implemented via `JwtTokenService`—use `ValidateToken()` to extract `ClaimsPrincipal` in middleware
- All Folder/File endpoints must have [Authorize] attribute and extract UserId from JWT claims in handlers
- When implementing 2FA, use the `TwoFactorEnabled` flag and `CreatedAt`/`UpdatedAt` for audit trails
- Update/Delete user endpoints (scaffolded as empty) should follow the same CQRS pattern as Register/Login
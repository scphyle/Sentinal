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

### Run with Docker (recommended for local dev)
```bash
# Copy the env template and fill in secrets (JWT secret, Postgres creds, optional Azure storage)
cp .env.example .env

# Build and start Postgres + API
docker compose up --build
```
- API is published on `http://localhost:5230` (container port 8080)
- Postgres data persists in the `pgdata` named volume
- Local file storage persists in the `file_storage` named volume, mounted at `/data/files` in the container
- EF Core migrations run automatically on container startup (`db.Database.Migrate()` in `Program.cs`) — no manual `dotnet ef database update` needed for first-time setup
- `FILE_STORAGE_PROVIDER` env var switches between `Local` and `AzureBlob` (see `.env.example`)

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
The `InitialCreate` migration exists in `Sentinal.Infrastructure/Migrations/` and is applied automatically on API startup via `db.Database.Migrate()`. For manual/local (non-Docker) work:

```bash
# Apply pending migrations
dotnet ef database update -p src/Sentinal.Infrastructure -s src/Sentinal.Api

# Create a new migration
dotnet ef migrations add MigrationName -p src/Sentinal.Infrastructure -s src/Sentinal.Api
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

### Current Phase 1 Scope (~98% Complete)

**Controllers** (Presentation Layer):
- `UserController` - Register, Login, UpdatePassword, UpdateEmail, UpdateUsername, ConfirmEmail, soft DeleteUser all wired (UserAuthDto reissued with new JWT on email/username change). Logout and GetUser-by-id remain Phase 2 stubs.
- `FileController` - Fully wired CQRS endpoints: Create (multipart upload via `[FromForm]`), Get/GetAll/GetAllInFolder, Update name/description, Move, Delete, Search, RecycleBin, and `PUT` content update (`UpdateFileContentCommand`, retains version history). `GET /api/File/{fileId}` streams the file bytes directly (acts as the download endpoint — no separate download route needed). File history/versioning available via repository (`GetFileHistory`) but not exposed as its own endpoint yet.
- `FolderController` - Fully wired CQRS endpoints: Create, GetById/GetAll/Subfolders, UpdateName, Move, Delete, Search, RecycleBin. `CreateFolder` defaults `ParentId` to the user's root folder (`Id == UserId`) when none is supplied.

**Domain Models** (Entities):
- **UserEntity**: User authentication and file/folder ownership
  - `Id` (Guid), `Username` (string, 255 max, unique), `Email` (string, 255 max, unique)
  - `PasswordHash` (string, 512 max), `EmailConfirmed` (bool), `TwoFactorEnabled` (bool)
  - Soft delete: `MarkedForDeletion`, `DeletedAt` (7-day retention before permanent deletion)
  - Navigation: `Files`, `Folders`

- **FolderEntity**: Hierarchical folder structure
  - `Id` (Guid), `FolderName` (string, 255 max), `ParentFolderId` (Guid, nullable for root)
  - `UserId` (Guid), timestamps, soft delete fields
  - `FolderType` (nullable `SpecialFolderTypes` enum: `RecycleBin`, `History`) - identifies a user's special folders independent of their display name, so renames (e.g. on username change) don't break lookups
  - Navigation: `Parent` (self-reference), `Children` (collection), `Files`
  - A user's root folder has `Id == UserId` and `ParentFolderId == null` (created via `CreateRootFolderAsync` on registration)

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
- Storage is **flat per-user**: `/{BasePath}/{userId}/{fileId}` (all GUIDs) — the folder hierarchy is virtual and exists only in the database (`FolderEntity`/`FolderId`), not on disk
- GUID-based paths prevent naming conflicts and provide security through obscurity
- User-provided filenames stored in database, not filesystem
- Multiple storage provider support (Local, AWS S3, Azure Blob) via `IFileStorageService` — `LocalFileStorageService` and `AzureBlobFileStorageService` implemented; `S3FileStorageService` still a stub
- Configuration-driven via `FileStorageOptions` with `StorageType` enum (`FileStorage:StorageProvider` config/env var)
- Future: AES-256 encryption at rest (post-submission enhancement)

**Soft Deletes**:
- All entities support soft deletes: `MarkedForDeletion` (bool) and `DeletedAt` (DateTime?)
- 7-day retention period before permanent deletion (configured in `FileStorageOptions.DeletedFileRetentionDays`)
- Soft-delete cleanup service planned (runs daily, removes items > 7 days old)
- Prevents accidental data loss and maintains audit trail

**JWT Authentication** (Phase 1 - Complete):
- ✅ Token generation in both `LoginUserCommand` and `RegisterUserCommand` via `JwtTokenService`
- ✅ Registration returns `UserAuthDto` with token for immediate login (auto-login on sign-up)
- ✅ Claims include: userId (custom), ClaimTypes.Name, ClaimTypes.Email
- ✅ `JwtTokenService` provides `GenerateToken()` and `ValidateToken()` methods
- ✅ Algorithm: HmacSha256 with SymmetricSecurityKey from `Jwt:Secret` (set via `Jwt__Secret` env var / user secrets — appsettings.json ships with an empty placeholder)
- ✅ Issuer and Audience validation configured
- ✅ Token expiration: 120 minutes
- ✅ Bearer token middleware configured in `Program.cs` with `AddAuthentication()` and `AddJwtBearer()`
- ✅ TokenValidationParameters set up in Program.cs for ASP.NET Core validation pipeline
- ✅ `[Authorize]` on UserController/FileController/FolderController, `[AllowAnonymous]` on register/login
- ✅ UserId extracted from JWT claims (`User.FindFirstValue("userId")`) in FileController/FolderController handlers
- ✅ **Config bug fixed**: `JwtTokenService` and `Program.cs` both now read `Jwt:Secret` consistently; both throw a clear `InvalidOperationException` if it's missing

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
- Azure options: `AzureConnectionString`, `AzureContainerName` (bound from `FileStorage` config section; also configurable via `FILE_STORAGE_PROVIDER`/`AZURE_STORAGE_CONNECTION_STRING`/`AZURE_STORAGE_CONTAINER_NAME` env vars in docker-compose)
- Bound in `Infrastructure/DependencyInjection.cs` via `services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"))`; the concrete `IFileStorageService` (Local/S3/Azure) is selected at registration time based on `StorageProvider`
- `AzureBlobStorage` section in `appsettings.json` (`ConnectionString`, `ContainerName`) backs `AzureBlobFileStorageService`

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

Use `src/Sentinal.Api/Http/example.http` with Rider's HTTP Client (or another REST Client plugin) or curl:

- `example.http` covers register → login → file upload/download/update → folder creation → recycle bin, end to end.
- The login request is named (`# @name login`) and its response handler captures the JWT into a global var `authToken`, which subsequent requests reuse via `Authorization: Bearer {{authToken}}`.
- `src/Sentinal.Api/Http/http-client.env.json` defines a `dev` environment — select it in Rider so captured response/global variables persist across requests in the file.
- File upload/update endpoints (`POST`/`PUT /api/File`) require `multipart/form-data`, not JSON — see the `--SentinalBoundary` examples in `example.http`.

```bash
# Example: curl-based upload
curl -s -X POST http://localhost:5230/api/File \
  -H "Authorization: Bearer $TOKEN" \
  -F "FileName=test.txt" \
  -F "ContentType=text/plain" \
  -F "Description=Test upload" \
  -F "File=@/path/to/test.txt;type=text/plain"
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
    : IRequest<Result<UserAuthDto>>;
```

**3. Implement Handler with Logging**:
```csharp
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserAuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public async Task<Result<UserAuthDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Validation, business logic, error handling
        var user = await _userRepository.CreateUserAsync(command.Username, command.Email, hashedPassword);
        var token = _jwtTokenService.GenerateToken(user);
        return Result.Ok(new UserAuthDto(user.Id, user.Username, user.Email, token));
    }
}
```

**4. Wire in Controller**:
```csharp
[HttpPost("register")]
public async Task<ActionResult<UserAuthDto>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
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
- Repositories return raw values or throw exceptions for validation failures
- Handlers wrap repository results in `Result<T>` (FluentResults pattern) for API responses
- Repositories handle EF Core `SaveChangesAsync()` internally
- Example: `FolderRepository` validates business logic (duplicate names, ownership) in repository methods

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
- Repositories handle business logic validation (ownership checks, duplicate prevention) and throw exceptions for failures
- Handlers catch repository exceptions and wrap them in `Result<T>` for consistent API error handling
- Password hashing is centralized in `Argon2PasswordService`—never hash passwords in command handlers
- File storage is abstracted—add new providers by implementing `IFileStorageService`
- Soft deletes are enforced across all entities—use MarkFolderAsDeletedAsync, not direct deletion
- The GUID-based file path structure prevents naming conflicts and maintains security
- Entity relationships are configured with `OnDelete(DeleteBehavior.Restrict)` to prevent orphaning
- MediatR registration auto-discovers handlers in the Application assembly
- All command/query handlers should inject `ILogger<T>` for audit trails and debugging (see User & Folder CQRS patterns)
- Controller endpoints map request DTOs → commands/queries → handlers → response DTOs (see UserController for reference)
- **FolderRepository** is fully implemented with: Create, Read (GetFolder, GetAllFolders, GetSubfolders, SearchByName), Update (rename), Move (with duplicate check), Delete (soft), and RecycleBin queries
- **User authentication** uses unified `UserAuthDto` for both register and login responses with token
- Registration (`RegisterUserCommand`) now returns JWT token for immediate login—no separate login required
- JWT is implemented via `JwtTokenService`—use `ValidateToken()` to extract `ClaimsPrincipal` in middleware
- All Folder/File endpoints must have [Authorize] attribute and extract UserId from JWT claims in handlers
- When implementing 2FA, use the `TwoFactorEnabled` flag and `CreatedAt`/`UpdatedAt` for audit trails
- Update/Delete user endpoints (scaffolded as empty) should follow the same CQRS pattern as Register/Login
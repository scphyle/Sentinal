# Sentinal - Project TODO

## Phase 1: Core File Sharing API - Groundwork & Framework

### Architecture & Project Structure
- [x] Set up clean architecture layer structure (Domain, Application, Infrastructure, Presentation)
- [x] Create separate .NET projects for each layer
- [x] Configure MediatR for CQRS pattern
- [x] Set up dependency injection in Program.cs

### Domain Layer - Core Models
- [x] Define File aggregate root with properties (Name, Size, CreatedAt, UpdatedAt, FolderId)
- [x] Define Folder aggregate root with properties (Name, ParentFolderId, CreatedAt, UpdatedAt)
- [x] Define User aggregate root with properties (Username, Email, PasswordHash, TwoFactorEnabled)
- [x] Add soft delete support across all entities (MarkedForDeletion, DeletedAt)
- [x] Create domain interfaces (IFileRepository, IFolderRepository, IUserRepository, IPasswordHasher, IFileStorageService)
- [ ] Implement domain events if needed (future enhancement)

### Application Layer - CQRS Commands & Queries
**User Commands & Queries**:
- [x] RegisterUserCommand & handler with validation, password hashing, and duplicate checks
- [x] LoginUserCommand & handler with email/username lookup and Argon2 password verification
- [x] JWT Token Generation in both LoginUserCommand and RegisterUserCommand with claims (UserId, Username, Email)
- [x] UserRepository with full CRUD and recovery support
- [x] Create unified user DTO (UserAuthDto) consolidating registration and login responses
- [x] UpdatePasswordCommand & handler (verifies current password via Argon2, hashes new)
- [x] UpdateUserEmailCommand & handler (duplicate check, resets EmailConfirmed, reissues JWT)
- [x] UpdateUsernameCommand & handler (duplicate check, reissues JWT)
- [x] ConfirmEmailCommand & handler (sets EmailConfirmed - simplified, no token verification yet)
- [x] DeleteUserCommand & handler (soft delete via MarkUserAsDeletedAsync)

**Folder Commands & Queries**:
- [x] CreateFolderCommand & handler with validation and duplicate name check
- [x] UpdateFolderNameCommand & handler with name validation
- [x] DeleteFolderCommand & handler (soft delete) with ownership verification
- [x] GetFolderQuery & handler with ownership check and full folder data
- [x] GetFolderSubFoldersQuery & handler for hierarchical navigation
- [x] GetAllFoldersQuery & handler retrieving all user folders
- [x] SearchFolderByNameQuery & handler with LIKE search support
- [x] MoveFolderCommand & handler with duplicate validation
- [x] GetFoldersInRecycleBinQuery & handler for soft-deleted folders
- [x] Create folder-related DTOs (CreateFolderDto, FolderDto, UpdateFolderDto)

**File Commands & Queries**:
- [x] CreateFileCommand & handler (upload file)
- [x] UpdateFileNameCommand & handler (rename)
- [x] UpdateFileDescriptionCommand & handler
- [x] UpdateFileContentCommand & handler (new content, retains old version for history)
- [x] MoveFileCommand & handler
- [x] DeleteFileCommand & handler (soft delete)
- [x] GetFileByIdQuery & handler
- [x] GetAllFilesInFolderQuery & handler
- [x] GetAllFilesQuery & handler
- [x] SearchFileByNameQuery & handler
- [x] GetAllFilesInRecycleBinQuery & handler
- [x] GetFileHistory (file versioning) - repository method, returns prior versions
- [x] Create file-related DTOs (FileDataDto, FileContentDto)
- [x] File download - `GetFileByIdQuery`/`GET /api/File/{fileId}` streams the file bytes directly (FileStreamResult); no separate download endpoint needed

### Infrastructure Layer - Data Access & Services
- [x] Set up Entity Framework Core DbContext (SentinalDbContext)
- [x] Configure PostgreSQL connection in appsettings.json
- [x] Entity configurations for File, Folder, User with constraints and relationships
- [x] Implement Argon2PasswordService (IPasswordHasher)
- [x] Implement LocalFileStorageService (flat per-user storage `/{BasePath}/{userId}/{fileId}`) and AzureBlobFileStorageService (IFileStorageService); S3FileStorageService still a stub
- [x] Configure FileStorageOptions and StorageType enum, bound from config via `services.Configure<FileStorageOptions>(...)`
- [x] Implement User repository (IUserRepository) with soft-delete awareness and recovery flows
- [x] **Implement JwtTokenService (IJwtTokenService)**
  - [x] Token generation with claims (UserId, Username, Email)
  - [x] Token validation and claim extraction
  - [x] Configurable expiration (120 minutes) and secret key (`Jwt:Secret`)
- [x] **Complete Folder repository implementation** (all CRUD, hierarchy, search, and soft-delete operations with validation; `CreateRootFolderAsync` for `Id == UserId` root folders; `FolderType` enum for RecycleBin/History lookup)
- [x] **Complete File repository implementation** (CRUD, move, search, recycle bin, content/version history)
- [x] Create and apply database migrations (`InitialCreate`, applied automatically on startup via `db.Database.Migrate()`)
- [x] Database initialization on first run handled by migration-on-startup (no separate seeding step needed for Phase 1)
- [ ] Configure pgvector extension (preparation for semantic search)

### Presentation Layer - API Endpoints & Authentication
**Authentication** (Phase 1 - Complete):
- [x] JwtTokenService implemented with token generation and validation
- [x] LoginUserCommand returns JWT token in LoginUserDto
- [x] JWT bearer token authentication middleware configured in Program.cs
- [x] TokenValidationParameters (key, issuer, audience, lifetime) properly configured
- [x] Add [Authorize] attributes to protected endpoints (Folder/File controllers)
- [x] Extract user context (UserId) from JWT claims in File/Folder handlers
- [x] **Bug fixed**: `Jwt:Key`/`Jwt:Secret` config mismatch resolved - `JwtTokenService` and Program.cs both read `Jwt:Secret`, with a clear error if missing; set via `Jwt__Secret` env var (docker-compose) or user secrets locally

**Controllers**:
- [x] UserController - Fully wired except Logout/GetUser (Phase 2)
  - [x] POST /api/User/register - RegisterUserCommand, returns UserAuthDto
  - [x] POST /api/User/login - LoginUserCommand, returns UserAuthDto with JWT token
  - [x] POST /api/User/update-password - UpdatePasswordCommand
  - [x] POST /api/User/update-email - UpdateUserEmailCommand, returns new UserAuthDto (reissued token)
  - [x] POST /api/User/update-username - UpdateUsernameCommand, returns new UserAuthDto (reissued token)
  - [x] POST /api/User/confirm-email - ConfirmEmailCommand (sets EmailConfirmed, no token verification yet)
  - [x] DELETE /api/User/{id} - DeleteUserCommand (soft delete, self-only via JWT userId check)
  - [ ] POST /api/User/logout - Phase 2 (NotFound stub)
  - [ ] GET /api/User/{id} - Phase 2, profile view (NotFound stub)
- [x] FolderController - Fully wired (Create, GetById, GetAll, Subfolders, Search, RecycleBin, UpdateName, Move, Delete); Create defaults to the user's root folder as parent when none specified
- [x] FileController - Fully wired (Create via multipart upload, Get/Download (streams file), GetAll, GetAllInFolder, Update name/description, Move, Delete, Search, RecycleBin, Update content via PUT)

**FolderController Endpoints** (`api/Folder`): ✅ all wired
- [x] POST /api/Folder - Create folder (CreateFolderCommand)
- [x] GET /api/Folder/{folderId} - Get folder by ID (GetFolderQuery)
- [x] PATCH /api/Folder/{folderId}/Name - Rename folder (UpdateFolderNameCommand)
- [x] PATCH /api/Folder/{folderId}/Move - Move folder (MoveFolderCommand)
- [x] DELETE /api/Folder/{folderId} - Delete folder (DeleteFolderCommand)
- [x] GET /api/Folder/Subfolders/{folderId} - Get subfolders (GetFolderSubFoldersQuery)
- [x] GET /api/Folder/SearchFolderByName/{searchTerm} - Search folders by name (SearchFolderByNameQuery)
- [x] GET /api/Folder/RecycleBin - Get folders in recycle bin (GetFoldersInRecycleBinQuery)
- [x] GET /api/Folder/AllFolders - Get all folders (GetAllFoldersQuery)

**FileController Endpoints** (`api/File`):
- [x] POST /api/File - Upload file (CreateFileCommand, multipart/form-data via SaveFileRequest)
- [x] GET /api/File/{fileId} - Get file metadata and stream file content (GetFileByIdQuery, doubles as download endpoint)
- [x] GET /api/File/Allfiles - Get all files (GetAllFilesQuery)
- [x] GET /api/File/Allfolders - Get all folders (GetAllFoldersQuery, convenience for file UI)
- [x] GET /api/File/AllFilesInFolder/{folderId} - Files in a folder (GetAllFilesInFolderQuery)
- [x] GET /api/File/AllFilesInRecycleBin - Files in recycle bin (GetAllFilesInRecycleBinQuery)
- [x] GET /api/File/SearchFileByName/{searchTerm} - Search files (SearchFileByNameQuery)
- [x] PATCH /api/File/MoveFile - Move file (MoveFileCommand)
- [x] PATCH /api/File/UpdateFileDescription - Update description (UpdateFileDescriptionCommand)
- [x] PATCH /api/File/UpdateFileName - Rename file (UpdateFileNameCommand)
- [x] DELETE /api/File/{fileId} - Delete file (DeleteFileCommand)
- [x] PUT /api/File - Update file content (UpdateFileContentCommand, retains version history), via SaveFileRequest (multipart/form-data) with FileId

### Testing & Validation
- [x] XUnit test project (Sentinal.Tests) created
- [ ] Unit tests for domain entities (File, Folder, User)
- [ ] Unit tests for CQRS handlers (Commands & Queries)
- [ ] Unit tests for Argon2PasswordService
- [ ] Integration tests for repositories (File, Folder, User)
- [ ] Integration tests for FileStorageService implementations
- [ ] API endpoint tests (FoldersController, FilesController, AuthController)
- [ ] (Future: End-to-end integration tests)
- [ ] (Future: Load testing and performance benchmarks)

### Deployment & CI/CD
- [x] Dockerfile for containerization (multi-stage: .NET 10 SDK build -> aspnet runtime)
- [x] Docker Compose for local development (API + PostgreSQL, named volumes for db data and file storage, `.env`-driven config)
- [x] Environment configuration (appsettings.json, appsettings.Development.json, launchSettings.json, `.env`/`.env.example` for Docker)
- [ ] CI/CD pipeline setup (Gitea runners: build, test, publish)
- [ ] Automated Docker image creation and publishing
- [ ] GitHub replication setup (backup remote)
- [ ] Health check endpoints (GET /health)

### Validation & Documentation
- [x] OpenAPI/Swagger documentation generation (built-in via Microsoft.AspNetCore.OpenApi)
- [x] Scalar.AspNetCore for beautiful API documentation UI
- [ ] API endpoint documentation review and completion
- [ ] Code documentation (minimal, only when WHY is non-obvious)
- [ ] README updates with examples and API usage

### Maintenance & Cleanup Services
- [ ] Create background service/job to remove soft-deleted items older than 7 days
  - Files with MarkedForDeletion=true and DeletedAt > 7 days old
  - Folders with MarkedForDeletion=true and DeletedAt > 7 days old
  - Users with MarkedForDeletion=true and DeletedAt > 7 days old
- [ ] Schedule cleanup job to run daily (e.g., with Hangfire or similar)
- [ ] Add logging for cleanup operations

---

## Phase 2 & Beyond (Future)

### Intelligence & Frontend
- [ ] Semantic search implementation (pgvector integration)
- [ ] React + TypeScript frontend
- [ ] User authentication and authorization
- [ ] JWT token implementation
- [ ] Role-based access control (RBAC)
- [ ] Two-Factor Authentication (2FA) implementation
  - TOTP support (Google Authenticator, Authy, etc.)
  - Email-based 2FA as fallback
- [ ] Passkey support (WebAuthn/FIDO2)
  - Enable Bitwarden integration
  - Hardware security key support

### Advanced Features
- [ ] File versioning and history tracking
- [ ] Sharing and permissions management
- [ ] Collaborative features
- [ ] Audit logging
- [ ] Soft deletes and recovery

### Security & Privacy Enhancements (Post-Submission)
- [ ] End-to-end file encryption at rest
  - AES-256 encryption for stored files
  - Per-file encryption keys
  - Key derivation from user credentials
- [ ] Encryption in transit (TLS/HTTPS)
- [ ] File integrity verification (HMAC/signatures)

### Optimization
- [ ] Caching strategy (Redis for frequently accessed data)
- [ ] Query optimization and indexing
- [ ] Performance profiling and tuning
- [ ] Load testing

---

**Last Updated**: 2026-06-15
**Current Phase**: Phase 1 - Groundwork & Framework (~98% complete)
**Status**: Domain, Infrastructure, DI, User CQRS, JWT authentication, Folder CQRS, and File CQRS/repository all complete. FileController and FolderController fully wired, including file content update (PUT) and file download (via GET, streaming). Jwt:Key/Jwt:Secret config mismatch fixed. File storage redesigned to flat per-user layout (`/{userId}/{fileId}`) with folder hierarchy purely virtual/DB-driven. EF Core migrations created and applied automatically on startup. Docker/Docker Compose set up for local dev (Postgres + API, env-configurable storage provider). Remaining: S3 storage implementation, soft-delete cleanup job, unit/integration tests, health check endpoint, CI/CD.

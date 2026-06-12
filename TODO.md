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
- [ ] UpdateUserCommand & handler (scaffolded, pending implementation)
- [ ] DeleteUserCommand & handler (soft delete, scaffolded, pending implementation)

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
- [ ] CreateFileCommand & handler (upload file)
- [ ] UpdateFileCommand & handler (metadata only, not file content)
- [ ] DeleteFileCommand & handler (soft delete)
- [ ] GetFileByIdQuery & handler
- [ ] GetFilesByFolderQuery & handler
- [ ] GetAllFilesQuery & handler
- [ ] DownloadFileQuery & handler
- [ ] Create file-related DTOs (CreateFileDto, FileDto, UpdateFileDto)

### Infrastructure Layer - Data Access & Services
- [x] Set up Entity Framework Core DbContext (SentinalDbContext)
- [x] Configure PostgreSQL connection in appsettings.json
- [x] Entity configurations for File, Folder, User with constraints and relationships
- [x] Implement Argon2PasswordService (IPasswordHasher)
- [x] Implement LocalFileStorageService, S3FileStorageService, AzureBlobFileStorageService (IFileStorageService)
- [x] Configure FileStorageOptions and StorageType enum
- [x] Implement User repository (IUserRepository) with soft-delete awareness and recovery flows
- [x] **Implement JwtTokenService (IJwtTokenService)**
  - [x] Token generation with claims (UserId, Username, Email)
  - [x] Token validation and claim extraction
  - [x] Configurable expiration (120 minutes) and secret key
- [x] **Complete Folder repository implementation** (all CRUD, hierarchy, search, and soft-delete operations with validation)
- [ ] **Complete File repository implementation** (GetFiles, update/delete methods)
- [ ] Create and apply database migrations
- [ ] Set up database initialization and seeding
- [ ] Configure pgvector extension (preparation for semantic search)

### Presentation Layer - API Endpoints & Authentication
**Authentication** (Phase 1 - Complete):
- [x] JwtTokenService implemented with token generation and validation
- [x] LoginUserCommand returns JWT token in LoginUserDto
- [x] JWT bearer token authentication middleware configured in Program.cs
- [x] TokenValidationParameters (key, issuer, audience, lifetime) properly configured
- [ ] Add [Authorize] attributes to protected endpoints (Folder/File controllers)
- [ ] Extract user context (UserId) from JWT claims in Folder/File handlers

**Controllers**:
- [x] UserController - Fully wired with Register & Login endpoints dispatching CQRS commands
  - [x] POST /api/users/register - Dispatches RegisterUserCommand, returns RegisterUserDto
  - [x] POST /api/users/login - Dispatches LoginUserCommand, returns LoginUserDto with JWT token
  - [ ] POST /api/users/update-password - Scaffolded, pending UpdateUserCommand
  - [ ] POST /api/users/update-email - Scaffolded, pending UpdateUserCommand
  - [ ] POST /api/users/update-username - Scaffolded, pending UpdateUserCommand
  - [ ] POST /api/users/confirm-email - Scaffolded, pending implementation
  - [ ] DELETE /api/users/{id} - Scaffolded, pending DeleteUserCommand
- [ ] FolderController - Stubbed with HTTP routes (awaiting JWT auth & CQRS integration)
- [ ] FileController - Stubbed with HTTP routes (awaiting JWT auth & CQRS integration)

**FoldersController Endpoints**:
- [ ] POST /api/folders - Create folder (wire to CreateFolderCommand)
- [ ] GET /api/folders/{id} - Get folder by ID (wire to GetFolderByIdQuery)
- [ ] PUT /api/folders/{id} - Update folder (wire to UpdateFolderCommand)
- [ ] DELETE /api/folders/{id} - Delete folder (wire to DeleteFolderCommand)
- [ ] GET /api/folders?parentId={id} - Get folders by parent (wire to GetFoldersByParentQuery)
- [ ] GET /api/folders - Get all folders (wire to GetAllFoldersQuery)

**FilesController Endpoints**:
- [ ] POST /api/files - Upload file (wire to CreateFileCommand)
- [ ] GET /api/files/{id} - Get file metadata (wire to GetFileByIdQuery)
- [ ] PUT /api/files/{id} - Update file metadata (wire to UpdateFileCommand)
- [ ] DELETE /api/files/{id} - Delete file (wire to DeleteFileCommand)
- [ ] GET /api/files/{id}/download - Download file (wire to DownloadFileQuery)
- [ ] GET /api/files?folderId={id} - Get files by folder (wire to GetFilesByFolderQuery)
- [ ] GET /api/files - Get all files (wire to GetAllFilesQuery)

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
- [ ] Dockerfile for containerization
- [ ] Docker Compose for local development (API + PostgreSQL)
- [x] Environment configuration (appsettings.json, appsettings.Development.json, launchSettings.json)
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

**Last Updated**: 2026-06-11
**Current Phase**: Phase 1 - Groundwork & Framework (90% complete)
**Status**: Domain, Infrastructure, DI, User CQRS, JWT authentication fully complete. User and Folder CQRS commands/queries/handlers complete with unified UserAuthDto and full FolderRepository implementation. Registration now returns auth token for immediate login. Next: Wire FolderController endpoints to handlers, File CQRS implementation, and controller authorization integration.

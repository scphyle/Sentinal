# Sentinal - Project TODO

## Phase 1: Core File Sharing API - Groundwork & Framework

### Architecture & Project Structure
- [x] Set up clean architecture layer structure (Domain, Application, Infrastructure, Presentation)
- [x] Create separate .NET projects for each layer
- [x] Configure MediatR for CQRS pattern
- [ ] Set up dependency injection in Program.cs

### Domain Layer - Core Models
- [ ] Define File aggregate root with properties (Name, Size, CreatedAt, UpdatedAt, FolderId)
- [ ] Define Folder aggregate root with properties (Name, ParentFolderId, CreatedAt, UpdatedAt)
- [ ] Define value objects (FileId, FolderId, FileSize, etc.)
- [ ] Create domain interfaces and repositories
- [ ] Implement domain events if needed

### Application Layer - CQRS Commands & Queries
**Folder Commands & Queries**:
- [ ] CreateFolderCommand & handler
- [ ] UpdateFolderCommand & handler
- [ ] DeleteFolderCommand & handler
- [ ] GetFolderByIdQuery & handler
- [ ] GetFoldersByParentQuery & handler
- [ ] GetAllFoldersQuery & handler
- [ ] Create folder-related DTOs

**File Commands & Queries**:
- [ ] CreateFileCommand & handler
- [ ] UpdateFileCommand & handler
- [ ] DeleteFileCommand & handler
- [ ] GetFileByIdQuery & handler
- [ ] GetFilesByFolderQuery & handler
- [ ] GetAllFilesQuery & handler
- [ ] Create file-related DTOs

### Infrastructure Layer - Data Access & Services
- [ ] Set up Entity Framework Core DbContext
- [ ] Configure PostgreSQL connection and migrations
- [ ] Implement Folder repository (IFolderRepository)
- [ ] Implement File repository (IFileRepository)
- [ ] Set up database initialization and seeding
- [ ] Configure pgvector extension (preparation)

### Presentation Layer - API Endpoints
**FoldersController**:
- [ ] POST /api/folders - Create folder
- [ ] GET /api/folders/{id} - Get folder by ID
- [ ] PUT /api/folders/{id} - Update folder
- [ ] DELETE /api/folders/{id} - Delete folder
- [ ] GET /api/folders?parentId={id} - Get folders by parent
- [ ] GET /api/folders - Get all folders

**FilesController**:
- [ ] POST /api/files - Create/upload file
- [ ] GET /api/files/{id} - Get file by ID
- [ ] PUT /api/files/{id} - Update file metadata
- [ ] DELETE /api/files/{id} - Delete file
- [ ] GET /api/files?folderId={id} - Get files by folder
- [ ] GET /api/files - Get all files

### Testing & Validation
- [ ] Set up XUnit test projects (Sentinal.Tests, Sentinal.Api.Tests)
- [ ] Unit tests for domain entities
- [ ] Unit tests for CQRS handlers
- [ ] Integration tests for repositories
- [ ] (Future: API endpoint tests)
- [ ] (Future: End-to-end tests)

### Deployment & CI/CD
- [ ] Containerization setup (Dockerfile)
- [ ] Docker Compose for local development
- [ ] Environment configuration (.env, appsettings profiles)
- [ ] CI/CD pipeline setup (build, test, publish)
- [ ] Automated Docker image creation and publishing
- [ ] Health check endpoints

### Validation & Documentation
- [ ] OpenAPI/Swagger documentation generation
- [ ] API documentation review
- [ ] Code documentation and comments where needed
- [ ] README updates

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

**Last Updated**: 2026-06-10
**Current Phase**: Phase 1 - Groundwork & Framework (In Progress)
**Status**: Project structure complete, implementation phase starting

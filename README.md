# Sentinal - Secure File Sharing API

Sentinal is a secure, type-safe file sharing API built with modern C# and .NET architecture patterns. It provides a robust foundation for file management operations with JWT authentication, soft-delete support, folder hierarchies, and a pluggable storage backend ready to be extended for cloud deployment.

## What Has Been Built

### Core API
- **123 unit tests** covering commands, queries, and edge cases (all passing)
- **CQRS Architecture** with MediatR for clean command/query separation
- **Authentication**: JWT tokens with Argon2 password hashing
- **File Management**: Upload, download, delete (soft), wildcard search, move operations
- **File Versioning**: Infrastructure in place, not exposed as endpoint yet
- **Folder Hierarchy**: Nested folder support with root/recycle/history folders
- **Storage Abstraction**: Local filesystem with S3 and Azure Blob ready
- **Clean Architecture**: Domain → Application → Infrastructure → API layers
- **PostgreSQL Integration**: Full ORM with Entity Framework Core

### Phase 1.5: React + TypeScript UI ✅
- React 18 + TypeScript frontend in `sentinal-ui/`
- Login/authentication page with token storage
- File upload and folder management interfaces
- File list with folder navigation and breadcrumbs
- PDF file viewing with react-pdf
- Served directly from API (wwwroot) - single deployment artifact

## Architectural Decisions

### Why CQRS + MediatR?
Clean separation between read (queries) and write (commands) operations. Each handler is independently testable, and the pattern scales well as the application grows.

### Why Soft Deletes?
Files marked for deletion are moved to a RecycleBin folder rather than permanently removed. This allows recovery and audit trails without complicating the data model. Users can permanently delete from the recycle bin if desired.

### Why Storage Abstraction?
`IFileStorageService` interface allows plugging in different backends (local, S3, Azure) without changing business logic. Currently using local filesystem; production would use cloud storage. This could also be easily extended to act as a backup location.

### Why JWT + Argon2?
Stateless authentication (JWT) is ideal for distributed/containerized systems. Argon2 is a modern, resistant password hashing algorithm better than bcrypt/scrypt.

### Why Three Default Folders Per User?
Every user gets a Root (named after their username), RecycleBin, and History folders on registration. This ensures the application never reaches a "no folders" state, which is an invariant enforced by tests. Each serves a different purpose. The History folder will eventually show file versions over time, though this is not yet exposed in the UI.

### Why Postgres, not SQLite?
PostgreSQL with pgvector support enables efficient vector similarity search for future file similarity features. SQLite lacks vector search capabilities, making PostgreSQL the better choice for long-term scalability.

## Technology Stack

### Backend
- **Runtime**: .NET 10.0
- **Language**: C# 12
- **Architecture**: CQRS + Clean Architecture with MediatR
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT + Argon2
- **Testing**: XUnit, Moq, FluentAssertions
- **Deployment**: Docker + Docker Compose

### Frontend
- **Framework**: React 18
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **HTTP**: Fetch API with centralized API client
- **PDF Viewing**: react-pdf + pdfjs-dist

## Project Structure

```
Sentinal/
├── src/
│   ├── Sentinal.Domain/           # Entities, value objects, enums
│   ├── Sentinal.Application/      # CQRS handlers, DTOs, interfaces
│   ├── Sentinal.Infrastructure/   # Repositories, EF migrations, storage
│   ├── Sentinal.Api/              # Controllers, middleware, wwwroot (UI)
│   └── Sentinal.Tests/            # 123 XUnit tests
├── sentinal-ui/                   # React + TypeScript source
├── docker-compose.yml             # PostgreSQL + API
├── CLAUDE.md                       # AI usage documentation
└── README.md                       # This file
```

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 12+ (or use Docker)
- Node.js 18+ (for frontend development/build only)

### Quick Start with Docker (Recommended)

```bash
# Copy environment file and fill in secrets
cp .env.example .env

# Build and start PostgreSQL + API with integrated React UI
docker-compose up --build
```

API & UI: `http://localhost:5230`

### Or Run Locally (Manual)

**Backend + Integrated UI:**
```bash
# Set up environment variables
cp .env.example .env
# Edit .env with your JWT_SECRET, database credentials, etc.

# Build React UI and copy to wwwroot
cd sentinal-ui
npm install
npm run build
cp -r dist/* ../src/Sentinal.Api/wwwroot/

# Run the API (serves both API + React UI)
cd ../src/Sentinal.Api
dotnet run --launch-profile http
```

The API and UI will be available at `http://localhost:5230`

**Frontend Development (Optional - for active UI work):**

If you want to develop the React UI with hot reload:

```bash
cd sentinal-ui
npm install
npm run dev
```

The dev server will start on `http://localhost:5173`. Note: This requires the backend API running separately on `http://localhost:5230` for API calls.

### Run Tests

```bash
dotnet test
```

Expected: **123/123 passing**

## Testing the API

### Interactive API Documentation (Scalar)

When running in development mode, an interactive API explorer is available at:

```
http://localhost:5230/scalar/v1
```

Scalar allows you to:
- Browse all API endpoints with schemas
- Test endpoints directly with authentication
- View live request/response examples
- Explore parameter types and validations

**Quick Test Flow:**
1. Open Scalar at `http://localhost:5230/scalar/v1`
2. Go to `POST /api/User/register` and create a test account
3. The response includes a JWT token
4. Use that token in the `Authorization: Bearer {token}` header for subsequent requests
5. Test file upload, folder creation, and file management endpoints

### Command Line Testing

**Register a user:**
```bash
curl -X POST http://localhost:5230/api/User/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "TestPassword123!"
  }'
```

**Login:**
```bash
curl -X POST http://localhost:5230/api/User/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "testuser",
    "password": "TestPassword123!"
  }'
```

## API Endpoints

### Authentication
- `POST /api/User/register` - Register new user (returns JWT)
- `POST /api/User/login` - Login (returns JWT)
- `POST /api/User/update-password` - Update password
- `POST /api/User/update-email` - Update email
- `POST /api/User/update-username` - Update username
- `POST /api/User/confirm-email` - Confirm email
- `DELETE /api/User/{id}` - Delete user (soft delete)

### Files
- `POST /api/File` - Upload file
- `GET /api/File/{fileId}` - Download/get file content
- `GET /api/File/Allfiles` - List all user files
- `GET /api/File/Allfolders` - List all user folders
- `GET /api/File/AllFilesInFolder/{folderId}` - List files in folder
- `GET /api/File/AllFilesInRecycleBin` - List deleted files
- `GET /api/File/SearchFileByName/{searchTerm}` - Search files by name
- `PATCH /api/File/UpdateFileName` - Rename file
- `PATCH /api/File/UpdateFileDescription` - Update file description
- `PATCH /api/File/MoveFile` - Move file to folder
- `PUT /api/File` - Update file content (multipart)
- `DELETE /api/File/{fileId}` - Soft delete file (move to RecycleBin)

### Folders
- `POST /api/Folder` - Create folder
- `GET /api/Folder/{folderId}` - Get folder details
- `GET /api/Folder/AllFolders` - List all user folders
- `GET /api/Folder/Subfolders/{folderId}` - List subfolders
- `GET /api/Folder/RecycleBin` - List deleted folders
- `GET /api/Folder/SearchFolderByName/{searchTerm}` - Search folders by name
- `PATCH /api/Folder/{folderId}/Name` - Rename folder
- `PATCH /api/Folder/{folderId}/Move` - Move folder
- `DELETE /api/Folder/{folderId}` - Soft delete folder

## AI Tool Usage

### Tools Used
- **Claude** (Interactive chat) - Architecture design, refactoring guidance, debugging
- **Claude Code** (Rider integration) - Test generation, handler boilerplate, code review
- **Claude Code** (WebStorm integration) - React component generation, TypeScript types

### Where AI Helped Most
1. **Test Generation** - Quickly produced 123 comprehensive tests with proper mocking patterns
2. **CQRS Handler Boilerplate** - Handlers follow consistent patterns; AI generation ensured consistency
3. **React Components** - Generated Login, Dashboard, API client with TypeScript types (used most heavily here)
4. **Debugging** - AI helped identify and fix edge cases in repository logic

### Where I Corrected AI
1. **Error Messages** - Tests initially had wrong expected messages; corrected to match actual handlers
2. **Mocking** - Some data setup was incorrect; had to adjust test fixtures
3. **Code Quality** - AI sometimes generated patterns that conflicted with existing design choices
4. **UI Integration** - Guided Claude Code on API structure and implementation preferences

### What I Built Myself
- Initial architecture design and entity modeling
- Repository interfaces and database schema design
- Error handling strategy and Result pattern usage
- Test data builders for consistent test setup
- Core CQRS command/query logic
- Storage provider abstraction and implementations
- Authentication flow and JWT integration

## Third-party Libraries

### Backend (.NET)
**Architecture & Patterns:**
- MediatR 14.1.0 - CQRS mediator pattern
- FluentResults 4.0.0 - Result<T> error handling

**Database:**
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2 - PostgreSQL provider
- Microsoft.EntityFrameworkCore.Design 10.0.5 - EF Core tooling
- Pgvector.EntityFrameworkCore 0.3.0 - Vector search support (future)

**Authentication & Security:**
- System.IdentityModel.Tokens.Jwt 8.19.1 - JWT handling
- Microsoft.AspNetCore.Authentication.JwtBearer 10.0.9 - JWT middleware
- Konscious.Security.Cryptography.Argon2 1.3.1 - Password hashing

**Cloud Storage:**
- Azure.Storage.Blobs 12.29.0 - Azure Blob Storage

**API Documentation:**
- Microsoft.AspNetCore.OpenApi 10.0.8 - OpenAPI support
- Scalar.AspNetCore 1.2.48 - Interactive API docs UI

**Testing:**
- xunit 2.9.3 - Test framework
- Moq 4.20.72 - Mocking
- FluentAssertions 6.12.1 - Readable assertions
- Microsoft.NET.Test.SDK 17.11.1 - Test runner

### Frontend (React)
- react 19.2.6 - UI framework
- react-router-dom 6.30.4 - Client-side routing
- react-pdf 10.4.1 - PDF viewer component
- pdfjs-dist 5.4.296 - PDF rendering engine
- TypeScript 5.3.3 - Type safety
- Tailwind CSS - Styling
- Vite 8.0.12 - Build tool

## Future Improvements

### Given More Time
1. **Pagination** - File/folder lists should paginate large datasets
2. **Advanced Search** - PostgreSQL full-text search (FTS) and semantic search with pgvector
3. **Sharing & Permissions** - Share files with other users, role-based access control
4. **Advanced UI** - Drag-and-drop upload, file preview gallery, file history timeline
5. **Audit Logging** - Log all file operations for compliance and forensics
6. **Cloud Storage** - S3 or Azure Blob primary storage with backup location option

### Alternative Architectural Choices (Not Implemented)
These were considered but not chosen for Phase 1:
1. **Event Sourcing** - Log all changes as immutable events instead of updating records. Pros: full audit trail, event replay. Cons: increased complexity, event versioning challenges.
2. **CQRS Read Model** - Separate optimized read database. Pros: read/write scaling. Cons: eventual consistency complexity.
3. **Saga Pattern** - Orchestrate multi-step operations. Pros: explicit workflows. Cons: added complexity for current scope.
4. **GraphQL** - Query language instead of REST. Pros: flexible queries. Cons: overkill for current API surface.

## Development Notes

- See `CLAUDE.md` for detailed architectural guidance and development patterns
- All handlers follow the same validation → try/catch → Result pattern
- Tests use Moq for repository mocks; each test covers happy path + validation + error cases
- Frontend and backend are deployed as a single artifact (React built to wwwroot)
- 123 tests provide comprehensive coverage of business logic and edge cases
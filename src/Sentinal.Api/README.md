# Sentinal - Secure File Sharing API

Sentinal is a secure file sharing API designed to guard and manage file access through a containerized file system. Built with modern C# and .NET architecture patterns, it provides a robust foundation for file management operations with plans for semantic search and collaborative features.

## Vision

Sentinal aims to be a comprehensive file sharing platform that combines secure file storage, intelligent semantic search, and an intuitive interface—all built on a strong architectural foundation following CQRS and clean architecture principles.

## Current Phase: Phase 1 - Groundwork & Framework

Phase 1 focuses on establishing the core infrastructure and basic file management APIs:
- **Folders Controller**: Manage folder hierarchies
- **Files Controller**: Manage file operations
- **Clean Architecture**: Layered structure with separation of concerns
- **CQRS Pattern**: Command/Query Responsibility Segregation using MediatR

## Technology Stack

### Backend
- **Runtime**: .NET 10.0
- **Language**: C# 12
- **Architecture Pattern**: CQRS with MediatR
- **Database**: PostgreSQL (with pgvector for future semantic search)
- **Testing**: XUnit
- **Deployment**: Docker containers with CI/CD pipeline

### Frontend (Phase 2)
- **Framework**: React
- **Language**: TypeScript

## Project Structure

```
Sentinal.Api/
├── Domain/                 # Domain entities, aggregates, value objects
├── Application/            # CQRS commands, queries, handlers, DTOs
├── Infrastructure/         # Database context, repositories, external services
├── Presentation/           # API controllers, request/response models
├── Sentinal.Api.Tests/     # XUnit test project
└── ...configuration files
```

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 12+
- Docker (for containerized development)

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --launch-profile http
```

The API will be available at `http://localhost:5230`

### Run Tests
```bash
dotnet test
```

## Roadmap

### Phase 1: Core File API ✓ (In Progress)
- Folders and Files controllers
- Basic CQRS framework
- PostgreSQL integration

### Phase 2: Intelligence & Frontend
- Semantic search using pgvector
- React + TypeScript frontend
- User authentication and authorization

### Phase 3: Advanced Features
- File versioning and history
- Sharing and permissions management
- Collaborative features
- Audit logging

## Development Notes

- See `CLAUDE.md` for detailed development guidance
- See `TODO.md` for current tasks and progress
- Follow clean architecture principles in all new code
- Use CQRS pattern for all business operations

## Contributing

When contributing to Sentinal:
1. Ensure code follows clean architecture principles
2. Write unit tests using XUnit for new handlers
3. Update documentation as scope changes
4. Reference TODO.md for task tracking

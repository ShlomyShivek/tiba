# Tiba - Microservices Todo API

A .NET 8.0 microservices-based Todo API application demonstrating asynchronous request-reply patterns with RabbitMQ messaging and PostgreSQL persistence.

## Architecture Overview

This application consists of three main components:

- **REST Service** (`rest_service/`) - JWT-authenticated HTTP API gateway
- **Backend Service** (`backend_service/`) - Background worker for business logic processing
- **Shared Library** (`shared/`) - Common models and messaging contracts

### Communication Pattern
- **Synchronous**: HTTP REST API for client-facing operations
- **Asynchronous**: RabbitMQ request-reply pattern for inter-service communication
- **Persistence**: PostgreSQL with Entity Framework Core Code First approach

## Quick Start

### Prerequisites
- Docker & Docker Compose
- .NET 8.0 SDK (for local development)

### Running with Docker (Recommended)

```bash
# Start all services
./start.sh

# Stop all services
./stop.sh
```

This will start:
- PostgreSQL database (localhost:5432)
- RabbitMQ with management UI (localhost:5672, UI at localhost:15672)
- Backend service for message processing
- REST API service (localhost:5000)

### Manual Development Setup

```bash
# Build solution
dotnet build tiba.sln

# Start backend service (requires PostgreSQL and RabbitMQ running)
cd backend_service && dotnet run

# Start REST API (in separate terminal)
cd rest_service && dotnet run
```

## API Documentation

- **Swagger UI**: http://localhost:5000/swagger
- **API Base URL**: http://localhost:5000

### Endpoints
- `GET /todos` - Get user's todos (requires JWT)
- `POST /todos` - Create new todo (requires JWT)

### Authentication
Uses JWT Bearer tokens. Test token available in `app.http` file with `userId=1`.

## Database

- **PostgreSQL 15** with automatic schema creation
- **Connection**: `Host=localhost;Database=tiba_todos;Username=postgres;Password=postgres`
- **Entity Framework Core** with Code First migrations
- **Tables**: `todos` with user-scoped data isolation

## Message Queues

- **Main Queues**: `todo.get_by_user`, `todo.create`
- **Dead Letter Queues**: Automatic retry (3 attempts) with dead letter routing
- **Management UI**: http://localhost:15672 (guest/guest)

## Development

### Environment Variables
- `POSTGRES_CONNECTION_STRING` - Database connection (optional)
- `RABBITMQ_HOST` - RabbitMQ hostname (default: localhost)
- `RABBITMQ_USER/RABBITMQ_PASSWORD` - RabbitMQ credentials (default: guest/guest)

### Project Commands
```bash
# Restore dependencies
dotnet restore

# Clean build
dotnet clean

# Build solution
dotnet build

# Add package to specific project
dotnet add backend_service package PackageName
```

### Testing
Use the `app.http` file with provided JWT tokens for API testing.

## Current Implementation Status

✅ **Completed:**
- PostgreSQL persistence with Entity Framework Core
- RabbitMQ messaging with dead letter queues and retry logic
- JWT authentication (mock validation for development)
- Docker-based development environment
- Global exception handling

## Known Issues & TODO Items

### High Priority
1. **Logging**: Change console to structured logging with file output
2. **Authorization**: Implement real JWT validation and user service
3. **Unit Tests**: Add comprehensive test coverage
4. **Healthcheck**: Implement real healthcheck

### Medium Priority
4. **Shared Library**: Consider moving to separate NuGet package
5. **Model Separation**: Create separate REST API models with mapping
6. **Build Warnings**: Address all compiler warnings

### Architecture Improvements
- Separate models for API and messaging layers
- Implement proper dependency injection for handlers
- Add monitoring and observability
- Implement API versioning
- Add integration tests

## Contributing

1. Follow existing code patterns and conventions
2. Update tests for new functionality
3. Ensure Docker environment works correctly
4. Update documentation for significant changes
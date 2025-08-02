# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a production-ready .NET 8.0 microservices Todo API application demonstrating asynchronous request-reply patterns with RabbitMQ messaging and PostgreSQL persistence. The system is designed for high-throughput scenarios with connection pooling via PgBouncer and horizontal scaling of worker services.

## Architecture

### Three-Tier Microservices Pattern
- **REST Service** (`rest_service/`): JWT-authenticated HTTP API gateway with minimal logic
- **Backend Service** (`backend_service/`): Horizontally scalable background workers for business logic processing (4 instances in docker-compose)
- **Shared Library** (`shared/`): Common models and messaging contracts

### Request-Reply Communication Flow
1. **HTTP Request** → REST API validates JWT and extracts user ID
2. **Service Layer** → TodoService creates typed requests (`GetTodosByUserIdRequest`, `CreateTodoRequest`)
3. **RabbitMQ Client** → Publishes to queues with correlation IDs using `amq.rabbitmq.reply-to` pattern
4. **Background Workers** → Multiple TodoWorker instances consume from queues with scoped dependency injection
5. **Repository Layer** → ITodoRepository executes operations via Entity Framework Core
6. **Reply Queue** → Handler sends typed responses back via correlation ID matching
7. **HTTP Response** → REST API returns deserialized results to client

### High-Performance Infrastructure
- **PgBouncer**: Transaction-level connection pooling (max 25k client connections, 500 DB connections)
- **PostgreSQL**: High-concurrency configuration (250 max connections, optimized shared buffers)
- **RabbitMQ**: Dead letter queues with 3-retry logic and durable message persistence
- **Docker Scaling**: 4 backend service instances for load distribution

## Development Commands

### Docker-Based Development (Required)
```bash
# Complete environment startup (builds .NET solution and starts all services)
./start.sh

# Stop all services
./stop.sh
```

The start script performs:
1. .NET solution build with `dotnet clean`, `dotnet restore`, `dotnet build --configuration Release`
2. Docker Compose orchestration with health checks
3. PgBouncer configuration reload
4. API test execution with sample JWT token

### Manual Development (Not Recommended)
```bash
# Build entire solution
dotnet build tiba.sln

# Required infrastructure: PostgreSQL on :5432, RabbitMQ on :5672
# Start backend workers first (4 instances recommended for load testing)
cd backend_service && dotnet run

# Start REST API
cd rest_service && dotnet run
```

### Performance Testing
```bash
# JMeter load testing (10,000 concurrent users)
./run_jmeter_test.sh
```

## Key Implementation Patterns

### Extension-Based Service Registration
The REST service uses fluent extension methods in `ServiceCollectionExtensions.cs`:
```csharp
builder.Services
    .AddApplicationServices()      // TodoService, RabbitMqClient
    .AddJwtAuthentication()        // Mock JWT validation
    .AddExceptionHandling()        // Global exception middleware
    .AddHealthCheck();             // Health endpoints
```

### RabbitMQ Request-Reply Pattern
- **Direct Reply-To**: Uses `amq.rabbitmq.reply-to` for efficient response routing
- **Correlation IDs**: Guid-based message correlation between request and response
- **Dead Letter Queues**: Automatic retry (3 attempts) with `x-dead-letter-exchange` configuration
- **Timeout Handling**: Configurable timeouts (default 240 seconds) with cancellation tokens

### Repository Pattern with Scoped Injection
```csharp
// Backend workers create scoped repositories per message
using var scope = _serviceProvider.CreateScope();
var todoRepository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
```

### Environment-Based Configuration
- **POSTGRES_CONNECTION_STRING**: Database via PgBouncer (Host=pgbouncer;Port=6432;Database=tiba_todos;Username=postgres;Password=postgres)
- **RABBITMQ_HOST/USER/PASSWORD**: Message broker connectivity
- **JWT_VALIDATE_***: JWT validation flags (all disabled for development)
- **SERVICE_NAME**: Unique identifier for connection tracking

## Namespace Architecture

**REST Service**:
- `Tiba.Rest.Services`: Core service implementations (TodoService, RabbitMqClient, MockAuthService)
- `Tiba.Rest.Extensions`: Fluent service registration extensions  
- `Tiba.Rest.Exceptions`: Custom exception types and global handlers

**Backend Service**:
- `Tiba.BackendService`: TodoWorker background service
- `Tiba.BackendService.Dal`: Repository pattern with Entity Framework (ITodoRepository, TodoDbContext)
- `Tiba.BackendService.Handlers`: Message processing handlers (GetTodosHandler, CreateTodoHandler)

**Shared Library**:
- `Tiba.Shared.Model`: Domain entities (Todo, User)
- `Tiba.Shared.Messaging`: Request/response contracts with BaseResponse inheritance

## Service URLs and Testing

- **REST API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger  
- **Health Check**: http://localhost:5000/health
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **PostgreSQL Direct**: localhost:5432 (postgres/postgres)
- **PgBouncer**: localhost:6432 (transaction pooling)

### JWT Testing
Use `app.http` file with provided test token:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwidXNlcklkIjoiMSIsIm5hbWUiOiJKb2huIERvZSIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```
Token contains: `userId: "1"`, extracted by MockAuthService for user scoping.

## Database Schema and Persistence

### Entity Framework Code First
- **Auto-Migration**: `EnsureCreatedAsync()` runs on backend service startup
- **User Scoping**: All Todo operations filtered by authenticated user ID
- **Repository Pattern**: Abstracted data access via ITodoRepository interface

### Connection Architecture
```
Client → REST API → RabbitMQ → Backend Workers → PgBouncer → PostgreSQL
```

## Common Development Tasks

### Adding New Endpoints
1. Create request/response contracts in `Tiba.Shared.Messaging`
2. Implement handler in `Tiba.BackendService.Handlers` 
3. Add queue consumer in `TodoWorker.cs`
4. Add REST endpoint in `Program.cs` with JWT validation

### Scaling Backend Processing
- Increase backend service replicas in `docker-compose.yml`
- Each instance creates independent RabbitMQ consumers
- PgBouncer handles connection pooling automatically

### Message Queue Configuration
- Main queues: `todo.get_by_user`, `todo.create`
- Dead letter exchange: `todo.deadletter` (direct type)
- Dead letter queues: `{queue_name}.deadletter`
- Retry headers: `x-retry-count` for tracking failed attempts

## Current Implementation Status

### Production-Ready Features
- Horizontal scaling with 4 backend worker instances
- Transaction-level connection pooling via PgBouncer
- Dead letter queue handling with retry logic
- JWT authentication framework (mock validation for development)
- Global exception handling with structured error responses
- Health check endpoints for monitoring

### Development-Mode Configurations  
- JWT validation disabled (all JWT_VALIDATE_* flags set to false)
- Console logging instead of structured logging
- Auto-database creation instead of proper migrations
- No unit tests or integration tests

## Known Technical Debt

1. **JWT Security**: Mock validation needs production implementation with proper issuer/audience validation
2. **Logging**: Console logging should be replaced with structured logging (Serilog/NLog)
3. **Testing**: Missing comprehensive unit and integration test suites
4. **Health Checks**: Basic implementation needs actual database/RabbitMQ connectivity checks
5. **Model Separation**: REST API uses domain entities directly instead of separate DTOs
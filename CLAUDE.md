# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a production-ready .NET 8.0 microservices Todo API application with three main components:

- **rest_service**: JWT-authenticated REST API with health checks and robust error handling
- **backend_service**: Background worker service with PostgreSQL persistence and retry logic
- **shared**: Common library for models and messaging contracts

## Architecture

### Microservices Communication Pattern
- **Synchronous API**: REST endpoints for client communication
- **Asynchronous Processing**: RabbitMQ request-reply pattern with dead letter queues
- **Message Queues**: `todo.get_by_user`, `todo.create` with comprehensive retry logic
- **Reply Pattern**: Uses `amq.rabbitmq.reply-to` with correlation IDs
- **Error Handling**: 3-retry limit with dead letter queue routing and proper exception handling

### Data Persistence
- **Database**: PostgreSQL with Entity Framework Core Code First approach
- **Repository Pattern**: `ITodoRepository` with full CRUD operations
- **Auto-Migration**: Database schema created automatically on startup
- **Connection**: Configurable via `POSTGRES_CONNECTION_STRING` environment variable

### Namespace Architecture

All namespaces follow consistent capitalization standards:

**REST Service**:
- **Core**: `Tiba.Rest.Services` - Service implementations and interfaces
- **Extensions**: `Tiba.Rest.Extensions` - Service registration and configuration extensions
- **Exceptions**: `Tiba.Rest.Exceptions` - Custom exception types

**Backend Service**:
- **Core**: `Tiba.BackendService` - Main worker and service registration
- **Data Access**: `Tiba.BackendService.Dal` - Repository patterns and Entity Framework
- **Handlers**: `Tiba.BackendService.Handlers` - Message processing logic

**Shared Library**:
- **Models**: `Tiba.Shared.Model` - Domain entities (Todo, User)
- **Messaging**: `Tiba.Shared.Messaging` - Request/response contracts and base classes

**REST Service Features**:
- Extension-based service registration (`ServiceCollectionExtensions`)
- JWT authentication with configurable validation
- Health checks at `/health` endpoint
- Global exception handling with Problem Details
- Swagger/OpenAPI documentation

**Backend Service Features**:
- Entity Framework Core with PostgreSQL
- Repository pattern implementation
- RabbitMQ message processing with scoped dependency injection
- Dead letter queue configuration with retry logic
- Background service with proper disposal patterns

### Data Flow

1. **HTTP Request** → REST API with JWT auth and health monitoring
2. **Service Layer** → TodoService creates typed requests with validation
3. **Message Queue** → RabbitMqClient sends requests with correlation IDs
4. **Background Worker** → TodoWorker processes messages with scoped repositories
5. **Database Operations** → Repository executes CRUD operations via Entity Framework
6. **Response** → Handler returns results via RabbitMQ reply queues
7. **HTTP Response** → REST API returns structured responses to client

### Authentication & Authorization

- **JWT Bearer Tokens**: Required for all endpoints
- **Mock Validation**: Cryptographic validation disabled for development
- **User Extraction**: Parses `sub` or `userId` claims from token
- **User Scoping**: All operations filtered by authenticated user ID

## Development Commands

### Docker-Based Development (Recommended)
```bash
# Start all services with Docker Compose
./start.sh

# Stop all services
./stop.sh
# or: docker-compose down

# View logs for all services
docker-compose logs -f

# View logs for specific service
docker-compose logs -f backend-service
docker-compose logs -f rest-service
```

### Manual Development
```bash
# Build entire solution
dotnet build tiba.sln

# Run backend service first (requires RabbitMQ)
cd backend_service && dotnet run

# Run REST API (in separate terminal)
cd rest_service && dotnet run
```

### Development Workflow
```bash
# Restore all dependencies
dotnet restore

# Clean build artifacts
dotnet clean

# Run tests (when available)
dotnet test
```

### Project Management
```bash
# Add package to specific project
dotnet add backend_service package PackageName

# Add project reference
dotnet add rest_service reference shared/shared.csproj
```

## Key Dependencies

**REST Service**:
- Microsoft.AspNetCore.Authentication.JwtBearer (JWT auth)
- RabbitMQ.Client (messaging)
- Swashbuckle.AspNetCore (OpenAPI/Swagger)

**Backend Service**:
- Microsoft.Extensions.Hosting (background services)
- RabbitMQ.Client (messaging)

## Architecture Notes

### Current Implementation Status
- **Data Layer**: Production-ready PostgreSQL with Entity Framework Core
- **Security**: JWT parsing with configurable validation (mock mode for development)
- **Error Handling**: Comprehensive exception handling with retry logic and dead letter queues
- **Health Monitoring**: Health check endpoints with status reporting
- **Configuration**: Environment-based configuration with Docker Compose orchestration

### Dead Letter Queue Configuration
- **Dead Letter Exchange**: `todo.deadletter` (direct type)
- **Dead Letter Queues**: `todo.get_by_user.deadletter`, `todo.create.deadletter`
- **Retry Logic**: Maximum 3 retries before sending to dead letter queue
- **Queue Arguments**: `x-dead-letter-exchange`, `x-dead-letter-routing-key`, `x-max-retries`


### Messaging Contracts
- **Base Response**: `BaseResponse` with `Success` and `ErrorMessage`
- **Typed Requests**: `GetTodosByUserIdRequest`, `CreateTodoRequest`
- **Typed Responses**: `GetTodosResponse`, `CreateTodoResponse`
- **Inheritance**: All responses inherit from `BaseResponse`

### Development Dependencies
- **Docker & Docker Compose**: Required for complete development environment
- **PostgreSQL 15**: Database with persistent volumes
- **RabbitMQ**: Message broker with management UI at localhost:15672
- **Service URLs**: 
  - REST API: localhost:5000
  - Health Check: localhost:5000/health
  - Swagger: localhost:5000/swagger
  - PostgreSQL: localhost:5432

### Environment Configuration
- **POSTGRES_CONNECTION_STRING**: Database connection (default: Host=localhost;Database=tiba_todos;Username=postgres;Password=postgres)
- **RABBITMQ_HOST**: RabbitMQ hostname (default: localhost)
- **RABBITMQ_USER/RABBITMQ_PASSWORD**: RabbitMQ credentials (default: guest/guest)

### Extension Pattern Usage
- **Service Registration**: Use `ServiceCollectionExtensions` for modular service configuration
- **Method Chaining**: Services are registered using fluent builder pattern
- **Separation of Concerns**: Each extension method handles specific functionality (Auth, Health, etc.)

### Entity Framework Patterns
- **Repository Pattern**: Use `ITodoRepository` for data access abstraction
- **Scoped Injection**: Repository injected per message processing scope
- **Code First**: Database schema generated from entity models
- **Auto-Migration**: Use `EnsureCreatedAsync()` for development database setup

### Common Tasks
- **Add New Endpoint**: Create contracts in `Tiba.Shared.Messaging`, implement handler in `Tiba.BackendService.Handlers`, add endpoint in REST service
- **Database Changes**: Modify entity models in `Tiba.Shared.Model`, update repository interface and implementation
- **Service Configuration**: Add extension methods to `ServiceCollectionExtensions` for new services
- **Health Checks**: Extend `AddHealthCheck()` method with additional health check implementations
- **Testing**: Use `app.http` file with provided JWT tokens for API testing

### Known Issues & Technical Debt
- JWT validation in mock mode for development (needs production implementation)
- Console logging instead of structured logging framework
- Missing comprehensive unit and integration tests
- Health checks are basic (need actual database connectivity checks)
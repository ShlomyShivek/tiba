# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET microservices Todo API application with three main components:

- **rest_service**: JWT-authenticated REST API (.NET 8.0) that handles HTTP requests
- **backend_service**: Background worker service (.NET 8.0) that processes business logic via RabbitMQ
- **shared**: Common library for models and messaging contracts shared between services

## Architecture

### Microservices Communication Pattern
- **Synchronous API**: REST endpoints for client communication
- **Asynchronous Processing**: RabbitMQ request-reply pattern between services
- **Message Queues**: `todo.get_by_user`, `todo.create` with dead letter queues
- **Reply Pattern**: Uses `amq.rabbitmq.reply-to` with correlation IDs
- **Error Handling**: 3-retry limit with dead letter queue routing for failed messages

### Data Persistence
- **Data Layer**: Mock implementations, no database persistence yet
- **Handlers**: Return hardcoded sample data for development/testing

### Project Structure

**REST Service** (`rest_service/`):
- **Program.cs**: API endpoints with JWT authentication middleware and global exception handling
- **Services**: TodoService (RabbitMQ client), AuthService (JWT parsing), RabbitMqClient
- **Namespace**: `tiba.rest.*`

**Backend Service** (`backend_service/`):
- **Program.cs**: Hosted service setup with dependency injection
- **TodoWorker**: Background service that consumes RabbitMQ messages with retry logic
- **Handlers**: GetTodosHandler and CreateTodoHandler (currently mock implementations)
- **Namespace**: `backend_service`

**Shared Library** (`shared/`):
- **Models**: Todo, User entities
- **Messaging**: Request/response contracts with inheritance hierarchy
- **Namespace**: Mixed `tiba.rest.*` and `tiba.shared.*` (needs standardization)

### Data Flow

1. **HTTP Request** → REST API with JWT auth and global exception handling
2. **Service Layer** → TodoService creates typed request
3. **Message Queue** → RabbitMqClient sends request with correlation ID
4. **Background Worker** → TodoWorker deserializes and processes with retry logic
5. **Handler Processing** → Handlers return mock data (no database persistence)
6. **Response** → Handler returns typed response via RabbitMQ
7. **HTTP Response** → REST API returns result to client

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
- **Data Layer**: Mock implementations, no persistence yet
- **Security**: JWT parsing without cryptographic validation (development mode)
- **Error Handling**: Global exception handling with retry logic and dead letter queues
- **Configuration**: Environment-based configuration with Docker Compose

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
- **Docker**: Required for RabbitMQ and recommended development workflow
- **RabbitMQ**: Configured via Docker with management UI at localhost:15672
- **Service URLs**: REST API at localhost:5000, Swagger at localhost:5000/swagger

### Environment Configuration
- **RABBITMQ_HOST**: RabbitMQ hostname (default: localhost)
- **RABBITMQ_USER/RABBITMQ_PASSWORD**: RabbitMQ credentials (default: guest/guest)

### Common Tasks
- **Add Endpoint**: Create message contracts in shared, implement handler in backend, add endpoint in REST service
- **Shared Models**: Add to shared project, reference from both services
- **Message Processing**: Extend TodoWorker with new queue consumers and dead letter handling
- **Testing**: Use `app.http` file with provided JWT token for API testing

### Known Issues & Technical Debt
- Namespace inconsistency between `tiba.rest.*` and `tiba.shared.*`
- Shared models used across service boundaries (tight coupling)
- Console logging instead of structured logging
- Missing unit tests and health checks
- JWT validation disabled for development
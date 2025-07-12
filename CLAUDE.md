# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Esiritori is a real-time multiplayer drawing guessing game with four main components:

- **Frontend**: Next.js 15 + React 19 + TypeScript + Tailwind CSS
- **API Mock**: Node.js + Express (for prototyping)
- **C# API**: .NET 8.0 + Clean Architecture + DynamoDB (main implementation)
- **Infrastructure**: Docker Compose + LocalStack (AWS services mock)

## Development Commands

### Frontend (Next.js)
```bash
# Navigate to frontend project
cd frontend

# Install dependencies
pnpm install

# Development server
pnpm dev

# Build production
pnpm build

# Type checking
pnpm type-check
# or
npx tsc --noEmit

# Linting
pnpm lint
```

### C# API
```bash
# Navigate to C# project
cd backend

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API server (requires DynamoDB)
dotnet run --project EsiritoriApi.Api

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=GameTests"

# Run tests by category
dotnet test --filter "Category=ドメインモデル"

# Debug Lambda function locally
./scripts/debug-lambda-local.sh

# Install Lambda Test Tool manually
dotnet tool install -g amazon.lambda.testtool-8.0
```

### LocalStack Setup (DynamoDB + Lambda)
```bash
# Create DynamoDB table in LocalStack
chmod +x scripts/create-dynamodb-table.sh
./scripts/create-dynamodb-table.sh

# Build and deploy Lambda function to LocalStack
chmod +x scripts/build-lambda.sh
chmod +x scripts/deploy-lambda-localstack.sh
./scripts/build-lambda.sh
./scripts/deploy-lambda-localstack.sh

# Verify services
aws dynamodb list-tables --endpoint-url http://localhost:4566 --region ap-northeast-1
aws lambda list-functions --endpoint-url http://localhost:4566 --region ap-northeast-1
```

### API Mock (Node.js)
```bash
cd frontend/mock
npm install
npm run dev  # Development server with nodemon
npm start    # Production server
npm run generate  # Generate mock server from OpenAPI spec
```

### Docker Environment
```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# Stop services
docker compose down
```

**Included Services:**
- **Frontend**: Next.js development server (port 3000)
- **API Mock**: Node.js mock server (port 3001)  
- **LocalStack**: AWS services mock including DynamoDB and Lambda (port 4566)
- **DynamoDB Admin UI**: Web interface for DynamoDB management (port 8001)
- **Lambda Function**: C# API running as AWS Lambda function (accessible via LocalStack)

**DynamoDB Management:**
- Access DynamoDB Admin UI at http://localhost:8001
- View tables, items, and perform CRUD operations
- Connected to LocalStack DynamoDB automatically

## Architecture

### C# API Clean Architecture
The C# API follows Clean Architecture with Domain-Driven Design:

```
backend/
├── EsiritoriApi.Domain/          # Domain layer - business entities and rules
│   ├── Entities/                 # Business entities (Game, Player)
│   └── ValueObjects/             # Immutable domain values
├── EsiritoriApi.Application/     # Application layer - use cases
│   ├── DTOs/                     # Data transfer objects
│   ├── Interfaces/               # Repository contracts
│   └── UseCases/                 # Business workflows
├── EsiritoriApi.Infrastructure/  # Infrastructure layer - external concerns
│   └── Repositories/             # Data persistence implementations
├── EsiritoriApi.Api/            # Presentation layer - web API
│   └── Controllers/              # HTTP request handlers
├── EsiritoriApi.Domain.Tests/        # Domain layer tests
├── EsiritoriApi.Application.Tests/   # Application layer tests
├── EsiritoriApi.Infrastructure.Tests/ # Infrastructure layer tests
├── EsiritoriApi.Api.Tests/          # API layer tests
└── EsiritoriApi.Integration.Tests/  # Integration tests
```

**Key Patterns:**
- Domain entities encapsulate business rules
- Value objects are immutable and validated
- Use cases orchestrate domain operations
- Repository pattern abstracts data access
- Dependency injection for loose coupling

### Value Object Implementation Rules

**❌ Avoid WithXXX methods:**
```csharp
public Turn WithStatus(TurnStatus status)  // Don't use
```

**✅ Use domain-meaningful operations:**
```csharp
public Turn StartSettingAnswer()           // Express domain intent
public Turn SetAnswerAndStartDrawing(string answer, DateTime startTime)
public Turn FinishTurn(DateTime endedAt)
```

### Testing Strategy

The C# API has comprehensive test coverage organized by architectural layers:

**Layer-based Test Projects:**
- **EsiritoriApi.Domain.Tests**: Entity and Value Object behavior tests
- **EsiritoriApi.Application.Tests**: Use case and DTO tests
- **EsiritoriApi.Infrastructure.Tests**: Repository and data access tests
- **EsiritoriApi.Api.Tests**: Controller and HTTP endpoint tests
- **EsiritoriApi.Integration.Tests**: End-to-end API scenarios

**Test Commands:**
```bash
# Run all tests
dotnet test

# Run specific layer tests
dotnet test EsiritoriApi.Domain.Tests/
dotnet test EsiritoriApi.Application.Tests/
dotnet test EsiritoriApi.Infrastructure.Tests/
dotnet test EsiritoriApi.Api.Tests/
dotnet test EsiritoriApi.Integration.Tests/

# Run tests by category
dotnet test --filter "Category=ドメインモデル"
dotnet test --filter "Category=ユースケース"
```

**Coverage Achievements:**
- Domain layer: 82.89% (Target: 80%+) ✅
- Application layer: 88.81% (Target: 80%+) ✅
- Infrastructure layer: 100% (Target: 100%) ✅
- API layer: 53.94% (Target: 50%+) ✅

### Frontend Architecture

Next.js application with:
- App Router (Next.js 15)
- Server and Client Components
- TypeScript for type safety
- Tailwind CSS for styling
- WebSocket integration for real-time features

Key directories:
- `frontend/src/app/` - Next.js App Router pages
- `frontend/src/components/` - Reusable React components
- `frontend/src/lib/` - Utility functions and API clients

## Development Workflow

1. **Primary development** happens in the C# API following Clean Architecture
2. **Frontend** implements the designed user experience
3. **API Mock** serves as early prototyping tool
4. **LocalStack** provides AWS service mocking for development

## Important Files

- `docs/Coderule.md` - Comprehensive coding guidelines and DDD implementation rules
- `design/` - System design documentation and specifications
- `.env.local` - Environment configuration
- `docker-compose.yml` - Service orchestration
- `backend/EsiritoriApi.sln` - C# solution file

## Service URLs (Docker)

- Frontend: http://localhost:3000
- API Mock: http://localhost:3001  
- LocalStack: http://localhost:4566
- DynamoDB Admin UI: http://localhost:8001
- Lambda Function: Available via LocalStack Function URL (check deploy script output)

## Key Implementation Guidelines

- Follow Clean Architecture principles strictly
- Use Japanese test method names for living documentation
- Implement Value Objects with domain-meaningful operations (not WithXXX methods)
- Maintain high test coverage with meaningful assertions
- Use dependency injection for all cross-cutting concerns
- Handle errors appropriately at each architectural layer
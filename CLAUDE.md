# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Esiritori is a real-time multiplayer drawing guessing game with three main components:

- **Frontend**: Next.js 15 + React 19 + TypeScript + Tailwind CSS
- **API Mock**: Node.js + Express (for prototyping)
- **C# API**: .NET 8.0 + Clean Architecture (main implementation)
- **Infrastructure**: Docker Compose + LocalStack (AWS services mock)

## Development Commands

### Frontend (Next.js)
```bash
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
cd api-csharp

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API server
dotnet run --project EsiritoriApi.Api

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=GameTests"

# Run tests by category
dotnet test --filter "Category=ドメインモデル"
```

### API Mock (Node.js)
```bash
cd api
npm install
npm run dev  # Development server with nodemon
npm start    # Production server
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

## Architecture

### C# API Clean Architecture
The C# API follows Clean Architecture with Domain-Driven Design:

```
api-csharp/
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
└── EsiritoriApi.Tests/          # Test project - comprehensive test suite
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

The C# API has comprehensive test coverage organized by domain concepts:

- **Domain Tests**: Entity and Value Object behavior
- **Use Case Tests**: Application logic workflows  
- **Integration Tests**: End-to-end API scenarios
- **Repository Tests**: Data access layer

**Test Categories** (use `--filter "Category=..."`):**
- `ドメインモデル` - Domain model tests
- `ユースケース` - Use case tests
- `API` - Controller tests
- `統合テスト` - Integration tests

**Coverage Requirements:**
- Domain layer: 85%+
- Application layer: 90%+
- Infrastructure layer: 100%
- API layer: 70%+

### Frontend Architecture

Next.js application with:
- App Router (Next.js 15)
- Server and Client Components
- TypeScript for type safety
- Tailwind CSS for styling
- WebSocket integration for real-time features

Key directories:
- `src/app/` - Next.js App Router pages
- `src/components/` - Reusable React components
- `src/lib/` - Utility functions and API clients

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
- `api-csharp/EsiritoriApi.sln` - C# solution file

## Service URLs (Docker)

- Frontend: http://localhost:3000
- API Mock: http://localhost:3001  
- LocalStack: http://localhost:4566

## Key Implementation Guidelines

- Follow Clean Architecture principles strictly
- Use Japanese test method names for living documentation
- Implement Value Objects with domain-meaningful operations (not WithXXX methods)
- Maintain high test coverage with meaningful assertions
- Use dependency injection for all cross-cutting concerns
- Handle errors appropriately at each architectural layer
# DeveloperStore - Sales Management API

A robust, enterprise-grade RESTful API for Sales Management built with **.NET 8.0**, **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS (Command Query Responsibility Segregation)**.

---

## 🚀 Overview

The **Sales Management API** is part of the **DeveloperStore** ecosystem. It provides a complete CRUD solution for managing sales records, calculating tiered volume discounts, publishing domain lifecycle events, and managing sales and item cancellations with high reliability and testability.

Following DDD best practices, the Sales domain interacts with external bounded contexts (Customers, Products, Branches) using the **External Identities Pattern** with description denormalization, ensuring domain independence and decoupling.

---

## 🏛 Architecture & Design Principles

The application is architected following **Clean Architecture** and **SOLID** principles:

```
root
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/         # Enterprise business rules, Entities, Domain Events, Invariants
│   ├── Ambev.DeveloperEvaluation.Application/    # Application use cases, CQRS Commands/Queries, MediatR Handlers, AutoMapper
│   ├── Ambev.DeveloperEvaluation.ORM/            # Infrastructure, EF Core, PostgreSQL DbContext, Migrations, Repositories
│   ├── Ambev.DeveloperEvaluation.IoC/            # Dependency Injection registration
│   ├── Ambev.DeveloperEvaluation.Common/         # Cross-cutting concerns (Security, Validation, Logging, HealthChecks)
│   └── Ambev.DeveloperEvaluation.WebApi/         # Presentation layer, REST Controllers, Filters, Swagger, Middleware
├── tests/
│   ├── Ambev.DeveloperEvaluation.Unit/           # Unit tests (Domain, CQRS Handlers, Validators, Bogus Data)
│   ├── Ambev.DeveloperEvaluation.Integration/    # Integration tests (EF Core Repository against In-Memory DB)
│   └── Ambev.DeveloperEvaluation.Functional/     # End-to-End API tests with WebApplicationFactory
├── .doc/                                         # Technical specifications and architectural documentation
├── docker-compose.yml                            # Containerized multi-service orchestration (API, Postgres, Mongo, Redis)
└── Dockerfile                                    # Multi-stage optimized container build
```

### Key Patterns & Frameworks
* **Domain-Driven Design (DDD)**: Aggregate Roots (`Sale`), Entities (`SaleItem`), domain invariants protected via rich behavior methods (`AddItem`, `Cancel`, `CancelItem`).
* **External Identities Pattern**: Preserves referenced snapshot descriptors (`CustomerId`/`CustomerName`, `ProductId`/`ProductName`, `BranchId`/`BranchName`).
* **CQRS with MediatR**: Complete separation of write commands and read queries.
* **Domain Events**: Dispatches `SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent`, and `ItemCancelledEvent` with structured Serilog telemetry.
* **FluentValidation**: Request-level and command-level input validation with automated pipeline behaviors.
* **AutoMapper**: Convention-based object-to-object mapping across API, Application, and Domain layers.
* **Conventional Commits**: Clean, semantic Git history (`feat`, `fix`, `test`, `refactor`, `docs`).

---

## 💼 Business Rules & Quantity Discounts

The sales engine automatically enforces quantity-based discounting and limits per item:

| Quantity per Product | Discount Tier | Behavior |
| :--- | :---: | :--- |
| **1 – 3 items** | `0%` | Regular price, no discounts permitted |
| **4 – 9 items** | `10%` | 10% discount applied to the item subtotal |
| **10 – 20 items** | `20%` | 20% discount applied to the item subtotal |
| **> 20 items** | ❌ **Error** | Rejected with `400 Bad Request` (`DomainException`) |
| **<= 0 items** | ❌ **Error** | Rejected with `400 Bad Request` |

### Cancellations
* **Sale Cancellation (`PATCH /api/sales/{id}/cancel`)**: Marks the sale as cancelled, cancels all active items, sets active totals to zero, and raises `SaleCancelledEvent`.
* **Item Cancellation (`PATCH /api/sales/{id}/items/{itemId}/cancel`)**: Cancels a specific line item, recalculates sale totals, and raises `ItemCancelledEvent` and `SaleModifiedEvent`.

---

## 📡 API Endpoints

All endpoints are standardized under `/api/sales` with consistent JSON envelopes:

| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :---: |
| `POST` | `/api/sales` | Creates a new sale with items and calculated discounts | `201`, `400` |
| `GET` | `/api/sales/{id}` | Retrieves sale details and line items by ID | `200`, `404` |
| `GET` | `/api/sales` | Lists sales with pagination, ordering, and filtering | `200`, `400` |
| `PUT` | `/api/sales/{id}` | Updates existing sale details and recalculates totals | `200`, `400`, `404` |
| `DELETE` | `/api/sales/{id}` | Removes a sale record | `200`, `404` |
| `PATCH` | `/api/sales/{id}/cancel` | Cancels an entire sale | `200`, `404` |
| `PATCH` | `/api/sales/{id}/items/{itemId}/cancel` | Cancels a specific item within a sale | `200`, `404` |
| `GET` | `/health` | Application health check probe (liveness/readiness) | `200` |

### Query Parameters for `GET /api/sales`
In accordance with [`.doc/general-api.md`](./.doc/general-api.md):
* `_page`: Page number (default: `1`)
* `_size`: Items per page (default: `10`)
* `_order`: Sort expression (e.g. `saleDate desc`, `totalAmount asc`, `"saleDate desc, customerName asc"`)
* `customer`: Filter by customer name or ID
* `branch`: Filter by branch name or ID
* `status`: Filter by status (`1` for Active, `2` for Cancelled)
* `_minDate` / `_maxDate`: Date range filters (`yyyy-MM-ddTHH:mm:ssZ`)

---

## 🛠 Tech Stack

| Technology | Purpose |
| :--- | :--- |
| **.NET 8.0 & C# 12** | Core runtime platform and programming language |
| **PostgreSQL 13** | Relational primary database |
| **Entity Framework Core 8** | Object-Relational Mapping (Npgsql provider, Migrations, Fluent API) |
| **MongoDB 8.0** | Document store (provisioned in docker-compose stack) |
| **Redis 7.4** | In-memory distributed caching |
| **MediatR 12** | In-process mediator for CQRS commands, queries, and domain events |
| **FluentValidation 11** | Strongly-typed business and request validation |
| **AutoMapper 13** | DTO, command, and entity mapping profiles |
| **Serilog** | Structured application and domain event logging |
| **xUnit, FluentAssertions** | Test framework and expressive assertion library |
| **NSubstitute** | Mocking library for unit tests |
| **Bogus** | Realistic synthetic test data generation |
| **WebApplicationFactory** | In-memory end-to-end API functional integration testing |

---

## 🏃 Running the Application

### Option 1: Docker Compose (Recommended)

To run the entire ecosystem (API, PostgreSQL, MongoDB, Redis) in one step:

```bash
# Clone the repository
git clone https://github.com/Asascar/desafio-ambev.git
cd desafio-ambev

# Build and start all containers
docker compose up -d --build
```

The services will be available at:
* **API Swagger UI**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
* **Health Check**: [http://localhost:8080/health](http://localhost:8080/health)
* **PostgreSQL**: `localhost:5432` (`developer_evaluation` / `developer` / `ev@luAt10n`)
* **MongoDB**: `localhost:27017`
* **Redis**: `localhost:6379`

To apply database migrations automatically to PostgreSQL:
```bash
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

To stop containers:
```bash
docker compose down
```

---

### Option 2: Running Locally with .NET CLI

1. Ensure PostgreSQL is running (e.g. via `docker compose up -d ambev.developer_evaluation_database`).
2. Apply database migrations:
   ```bash
   dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
   ```
3. Run the WebApi:
   ```bash
   dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
   ```
4. Access Swagger at `http://localhost:5000/swagger` or `https://localhost:5001/swagger`.

---

## 🧪 Automated Testing Suite

The repository includes a comprehensive 3-tier testing strategy with **116 passing tests**:

1. **Unit Tests (`Ambev.DeveloperEvaluation.Unit`)** - 107 tests:
   * Boundary checks on volume discounts (0%, 10%, 20%, error on > 20).
   * Aggregate Root invariants and entity lifecycle.
   * CQRS Command & Query Handler execution with NSubstitute mocks.
   * Request and Command validators with FluentValidation.
   * Synthetic data generation via Bogus (`SaleTestData`).

2. **Repository Integration Tests (`Ambev.DeveloperEvaluation.Integration`)** - 4 tests:
   * EF Core queries, async pagination, sorting, and eager loading against an In-Memory relational database.

3. **End-to-End Functional Tests (`Ambev.DeveloperEvaluation.Functional`)** - 5 tests:
   * Full HTTP lifecycle testing with `WebApplicationFactory<Program>`.
   * Real endpoint requests (`POST`, `GET`, `PATCH`, pagination, validation errors).

### Execute all tests:
```bash
dotnet test Ambev.DeveloperEvaluation.sln
```

---

## 📝 Example Requests

### Create a Sale (`POST /api/sales`)
```json
POST /api/sales
Content-Type: application/json

{
  "saleDate": "2026-09-14T18:00:00Z",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "John Doe",
  "branchId": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
  "branchName": "Downtown Branch",
  "items": [
    {
      "productId": "b2c3d4e5-f6a7-4b5c-8d9e-1f2a3b4c5d6e",
      "productName": "Beer Brahma 350ml",
      "quantity": 5,
      "unitPrice": 10.00
    },
    {
      "productId": "c3d4e5f6-a7b8-4c5d-8e9f-2a3b4c5d6e7f",
      "productName": "Beer Skol 350ml",
      "quantity": 10,
      "unitPrice": 20.00
    }
  ]
}
```

**Response (`201 Created`):**
```json
{
  "success": true,
  "message": "Sale created successfully",
  "data": {
    "id": "36489678-60b9-40ef-aeac-a9d1cc0252c4",
    "saleNumber": "SALE-20260914212916-5599",
    "saleDate": "2026-09-14T18:00:00Z",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "John Doe",
    "branchId": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
    "branchName": "Downtown Branch",
    "totalAmount": 205.00,
    "status": 1,
    "isCancelled": false,
    "items": [
      {
        "id": "0b03abb6-76a7-4872-98cb-a766d451a047",
        "productId": "b2c3d4e5-f6a7-4b5c-8d9e-1f2a3b4c5d6e",
        "productName": "Beer Brahma 350ml",
        "quantity": 5,
        "unitPrice": 10.00,
        "discountPercentage": 0.10,
        "discount": 5.00,
        "totalAmount": 45.00,
        "isCancelled": false
      },
      {
        "id": "5970164b-fcd8-4a69-af15-baf265c43f3f",
        "productId": "c3d4e5f6-a7b8-4c5d-8e9f-2a3b4c5d6e7f",
        "productName": "Beer Skol 350ml",
        "quantity": 10,
        "unitPrice": 20.00,
        "discountPercentage": 0.20,
        "discount": 40.00,
        "totalAmount": 160.00,
        "isCancelled": false
      }
    ]
  },
  "errors": []
}
```

---

## 📄 Documentation References
Detailed technical documentation is available in the [`.doc/`](./.doc/) directory:
* [Overview & Competencies](/.doc/overview.md)
* [Tech Stack](/.doc/tech-stack.md)
* [Frameworks](/.doc/frameworks.md)
* [General API Definitions](/.doc/general-api.md)
* [Project Structure](/.doc/project-structure.md)

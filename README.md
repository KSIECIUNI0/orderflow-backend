# OrderFlow 🧾

![Build Status](https://github.com/KSIECIUNI0/orderflow-backend/actions/workflows/build.yml/badge.svg)
![Coverage](./coverage-badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=KSIECIUNI0_orderflow-backend&metric=alert_status&token=ec5dcf4c42c3d67e7cae0b396f858eec5cba36a1)](https://sonarcloud.io/summary/new_code?id=KSIECIUNI0_orderflow-backend)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=KSIECIUNI0_orderflow-backend&metric=coverage&token=ec5dcf4c42c3d67e7cae0b396f858eec5cba36a1)](https://sonarcloud.io/summary/new_code?id=KSIECIUNI0_orderflow-backend)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=KSIECIUNI0_orderflow-backend&metric=bugs&token=ec5dcf4c42c3d67e7cae0b396f858eec5cba36a1)](https://sonarcloud.io/summary/new_code?id=KSIECIUNI0_orderflow-backend)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=KSIECIUNI0_orderflow-backend&metric=code_smells&token=ec5dcf4c42c3d67e7cae0b396f858eec5cba36a1)](https://sonarcloud.io/summary/new_code?id=KSIECIUNI0_orderflow-backend)

OrderFlow is a backend service built to demonstrate modern .NET architectural patterns and provide scalable order management functionality.

The solution is designed with high maintainability and long-term extensibility in mind, leveraging:

- Clean Architecture
- Domain-Driven Design principles
- CQRS with MediatR
- Entity Framework Core + PostgreSQL
- Repository & Unit of Work patterns
- Docker for local development
- Automated CI pipeline (GitHub Actions)

---

## 🧠 Architecture Overview

```
                       ┌────────────────────────────────────┐
                       │           Presentation             │
                       │        (OrderFlow.Api Layer)       │
                       │  - Controllers                     │
HTTP                   │  - Model binding                   │
Requests  ───────────▶ │  - API Contracts (DTO requests)    │
                       └────────────────────────────────────┘
                                         │
                                         ▼
                       ┌────────────────────────────────────┐
                       │          Application Layer         │
                       │  - CQRS (Commands + Queries)       │
                       │  - MediatR Handlers                │
                       │  - DTO responses                   │
                       └────────────────────────────────────┘
                                         │
                                         ▼
                       ┌────────────────────────────────────┐
                       │             Domain Layer           │
                       │  - Entities / Aggregates           │
                       │  - Business rules / Invariants     │
                       └────────────────────────────────────┘
                                         │
                                         ▼
                       ┌────────────────────────────────────┐
                       │         Infrastructure Layer       │
                       │  - EF Core                         │
                       │  - DbContext                       │
                       │  - Repositories + Unit of Work     │
                       │  - Migrations                      │
                       └────────────────────────────────────┘
                                         │
                                         ▼
                                   PostgreSQL DB
                                   (Docker Compose)
```

---

## 🚀 Technology Stack

| Component | Stack |
|--|--|
| Runtime | .NET 9 |
| Architecture | Clean Architecture + CQRS |
| Database | PostgreSQL (via Docker) |
| ORM | Entity Framework Core 9 |
| Messaging | MediatR |
| Tests | xUnit |
| DevOps | Docker, GitHub Actions |

---

## 📦 Solution Structure

```
OrderFlow.sln
|
├─ src/
│  ├─ OrderFlow.Api/
│  ├─ OrderFlow.Application/
│  ├─ OrderFlow.Domain/
│  └─ OrderFlow.Infrastructure/
|
├─ tests/
│  └─ OrderFlow.UnitTests/
|
└─ docker/
   └─ docker-compose.yml
```

---

## ▶️ Local Development

### Pre Requirements

#### Install:
* .NET 9 SDK
* Docker
* awscli

#### AWS configuration:
  ```
  AWS Access Key ID [None]: test
  AWS Secret Access Key [None]: test
  Default region name [None]: us-east-1
  Default output format [None]: json 
  ```

### Run infrastructure

```bash
docker compose up -d
```

### Apply migrations

```bash
cd src/OrderFlow.Infrastructure
dotnet ef database update
```

### Run API

```bash
cd ../OrderFlow.Api
dotnet run
```

API → http://localhost:5270

---

## ✅ HTTP Test Examples

```http
@host = http://localhost:5270

### Create order
POST {{host}}/api/orders
Content-Type: application/json

{
  "productName": "Coca-Cola",
  "quantity": 1
}

### Get all orders
GET {{host}}/api/orders
Accept: application/json
```

---

## 🔄 CI Pipeline

✔️ Restore → Build → Test  
✔️ Publish artifact of API

Workflow location:
```
.github/workflows/build.yml
```
# 🏦 FinLedger SaaS
**Cloud-Native Double-Entry Accounting Engine**

> A high-performance, audit-ready financial ledger system designed for multi-tenant SaaS platforms. Built with **.NET 8**, **PostgreSQL**, and **Domain-Driven Design (DDD)** principles.

![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen) ![License](https://img.shields.io/badge/License-MIT-blue) ![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-orange)

## 🏗️ Architectural Overview (C4 Model)
FinLedger follows a **Modular Monolith** architecture to ensure strict domain boundaries while maintaining deployment simplicity.

### System Context
```mermaid
C4Container
    title Container Diagram for FinLedger SaaS

    Person(accountant, "Accountant / CFO", "Manages financial records")
    System_Boundary(c1, "FinLedger Platform") {
        Container(spa, "Dashboard", "React", "Financial visualization")
        Container_Boundary(backend, "Core Engine (.NET 8)") {
            Component(api, "API Gateway", "YARP", "Routing & Security")
            Component(mod_ledger, "Ledger Core", "Domain Module", "Double-Entry Logic")
            Component(mod_report, "Reporting", "CQRS Read Model", "Financial Statements")
        }
        ContainerDb(db, "Database", "PostgreSQL", "Schema-per-Tenant Storage")
        ContainerQueue(bus, "Event Bus", "RabbitMQ", "Integration Events")
    }
    Rel(accountant, spa, "Uses")
    Rel(spa, api, "API Calls")
    Rel(mod_ledger, db, "ACID Transactions")
    Rel(mod_ledger, bus, "Publishes Events")
```

## 🚀 Key Features (Principal Level)
- **Multi-Tenancy:** Schema-per-tenant isolation strategy ensuring 100% data privacy.
- **Double-Entry Integrity:** `Debit == Credit` invariant enforced at the Domain level.
- **Audit Trail:** Immutable ledger entries using Event Sourcing concepts.
- **Concurrency Control:** Optimistic locking to handle high-throughput financial transactions.

## 🛠️ Tech Stack
- **Core:** .NET 8, EF Core, MassTransit
- **Data:** PostgreSQL 16, Redis
- **DevOps:** Docker Compose, GitHub Actions

## 🚦 Getting Started
1. Clone the repository.
2. Run infrastructure:
   ```bash
   docker-compose up -d
   ```
3. Open the solution in Visual Studio / Rider.

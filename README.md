
# 🏦 FinLedger SaaS
**Cloud-Native Double-Entry Accounting Engine**

> A high-performance, audit-ready financial ledger system designed for multi-tenant SaaS platforms. Built with **.NET 9**, **PostgreSQL 16**, and **Domain-Driven Design (DDD)** principles.

![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen) ![Runtime](https://img.shields.io/badge/.NET-9.0-blue) ![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-orange)

## 🎯 Problem Statement
Most modern SaaS applications handle financial data using "Anemic Domain Models", leading to **Data Integrity** issues. In high-stakes FinTech, systems often fail to enforce double-entry invariants or ensure strict tenant isolation. **FinLedger** bridges this gap by combining deep **Accounting Domain expertise** with **Robust Engineering patterns** to provide an immutable, compliant, and highly scalable financial engine.

## 🏗️ Architectural Overview (C4 Model)
FinLedger follows a **Modular Monolith** architecture to ensure strict domain boundaries while maintaining deployment simplicity.

### System Context
```mermaid
C4Container
    title Container Diagram for FinLedger SaaS
    Person(accountant, "Accountant / CFO", "Manages financial records")
    System_Boundary(c1, "FinLedger Platform") {
        Container(spa, "Dashboard", "React", "Financial visualization")
        Container_Boundary(backend, "Core Engine (.NET 9)") {
            Component(api, "API Gateway", "YARP", "Routing & Security")
            Component(mod_ledger, "Ledger Core", "Domain Module", "Double-Entry Logic")
            Component(mod_report, "Reporting", "CQRS Read Model", "Financial Statements")
        }
        ContainerDb(db, "Database", "PostgreSQL 16", "Schema-per-Tenant Strategy")
        ContainerQueue(bus, "Event Bus", "RabbitMQ", "Integration Events")
    }
    Rel(accountant, spa, "Uses")
    Rel(spa, api, "API Calls")
    Rel(mod_ledger, db, "ACID Transactions")
    Rel(mod_ledger, bus, "Publishes Events")
```

## 🚀 Core Features & Technical Excellence

### 🏦 Advanced Financial Engine
- **Immutable Ledger (Zero-Delete Policy):** Implements a high-integrity accounting system where journal entries are finalized (Posted) and cannot be modified or deleted. All corrections are handled through **Automated Reversal Logic**, ensuring a 100% reliable audit trail.
- **Double-Entry Integrity:** The domain layer strictly enforces the fundamental accounting equation (`Sum(Debit) == Sum(Credit)`) as a system invariant, preventing out-of-balance transactions at the core level.

### 🏗️ Enterprise Architecture Patterns
- **Modular Monolith:** Designed with strict bounded contexts to ensure high maintainability and ease of future migration to microservices, while avoiding unnecessary distributed system overhead.
- **CQRS with MediatR:** Clear separation of read and write concerns, improving performance and making the codebase highly testable and extensible.
- **Outbox Pattern:** Guarantees transactional consistency between the database and external systems. Financial events are captured within the same atomic transaction as the business data, ensuring 100% delivery reliability.

### 🔐 Infrastructure & Resilience
- **Automated Multi-Tenancy:** Uses a sophisticated **Schema-per-Tenant** isolation strategy. The system dynamically creates and migrates database schemas for new tenants on-the-fly, ensuring maximum data privacy and regulatory compliance.
- **Redis Distributed Locking (RedLock):** Prevents race conditions during concurrent financial operations. This ensures that sensitive resources (like account balances) are never compromised during high-throughput traffic.
- **Global Error Handling:** Implements RFC 7807 (Problem Details for HTTP APIs), providing standardized, machine-readable error responses for all validation and system failures.

Health Monitoring: Integrated ASP.NET Core Health Checks for real-time monitoring of PostgreSQL and Redis connectivity.

## 🗺️ Project Roadmap
- [x] **Phase 1: Foundation & Persistence**
    - [x] Modular Monolith & Solution Architecture.
    - [x] Multi-tenancy Core (Schema-per-tenant via EF Core).
    - [x] Domain Modeling (Account, JournalEntry).
- [x] **Phase 2: Application Patterns & API Excellence**
    - [x] Implementation of **MediatR** for Command/Query separation (CQRS).
    - [x] **FluentValidation** & Global Exception Handling.
    - [x] **Dynamic Schema Creation** for automated tenant onboarding.
- [x] **Phase 3: Resilience & Consistency**
    - [x] **Redis Distributed Locking** for financial concurrency safety.
    - [x] **Outbox Pattern** for guaranteed event delivery (Transactional Integrity).
    - [x] **Structured Logging** with Serilog & JSON formatting.
- [x] **Phase 4: Financial Excellence & Reporting**
    - [x] **Immutable Ledger** posting logic & state machine.
    - [x] **Reversal Logic** for automatic counter-entry creation.
    - [x] **High-performance reporting** using optimized SQL/Dapper.
    - [x] **Automated PDF generation** for financial statements (QuestPDF).





## 🚦 Getting Started
1. `docker-compose up -d`
2. Run `FinLedger.Modules.Ledger.Api`.
3. Explore Swagger at `http://localhost:5000/swagger`.

---
**Status:** 🟢 *Phase 2 Complete. Dynamic Tenant Onboarding & CQRS Pipeline Operational.*
```

---



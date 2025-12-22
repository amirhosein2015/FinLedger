
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

## 🚀 Key Features 
- **Dynamic Multi-Tenancy:** Automated **Schema-per-tenant** isolation for 100% data privacy.
- **Double-Entry Integrity:** Strict `Debit == Credit` invariant enforced at the Domain level.
- **Enterprise API Standards:** Versioning (v1.0), Global Exception Handling, and RFC-standard ProblemDetails.
- **Automated Onboarding:** Real-time database schema generation for new tenants.

## 🗺️ Project Roadmap
- [x] **Phase 1: Foundation & Persistence**
    - [x] Modular Monolith & Solution Architecture.
    - [x] Multi-tenancy Core (Schema-per-tenant via EF Core).
    - [x] Domain Modeling (Account, JournalEntry).
    - [x] Initial API Integration & PostgreSQL Deployment.
- [x] **Phase 2: Application Patterns & API Excellence**
    - [x] Implementation of **MediatR** for Command/Query separation (CQRS).
    - [x] **FluentValidation** for automatic request validation.
    - [x] **Global Exception Handling** & API Versioning (v1.0).
    - [x] **Dynamic Schema Creation** for automated tenant onboarding.
- [ ] **Phase 3: Resilience & Consistency**
    - [x] **Redis Distributed Locking** using RedLock for financial concurrency safety.
    - [x] **Outbox Pattern** for guaranteed event delivery (Transactional Integrity).
    - [ ] **OpenTelemetry** for distributed tracing & observability.
- [ ] **Phase 4: Financial Excellence & Reporting**
    - [ ] **Immutable Ledger** posting logic & Fiscal Year closing.
    - [ ] High-performance reporting using optimized SQL/Dapper.
    - [ ] Automated PDF statement generation via background workers.



## 🚦 Getting Started
1. `docker-compose up -d`
2. Run `FinLedger.Modules.Ledger.Api`.
3. Explore Swagger at `http://localhost:5000/swagger`.

---
**Status:** 🟢 *Phase 2 Complete. Dynamic Tenant Onboarding & CQRS Pipeline Operational.*
```

---



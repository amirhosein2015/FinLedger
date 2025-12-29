
# 🏦 FinLedger SaaS
**Cloud-Native Double-Entry Accounting Engine**

> A high-performance, audit-ready financial ledger system designed for multi-tenant SaaS platforms. Built with **.NET 9**, **PostgreSQL 16**, and **Domain-Driven Design (DDD)**.

![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen) ![Runtime](https://img.shields.io/badge/.NET-9.0-blue) ![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-orange)

## 🎯 Problem Statement
Most modern SaaS applications handle financial data using "Anemic Domain Models", leading to **Data Integrity** issues. In high-stakes FinTech, systems often fail to enforce double-entry invariants or ensure strict tenant isolation. **FinLedger** bridges this gap by combining deep **Accounting Domain expertise** with **Robust Engineering patterns** to provide an immutable, compliant, and highly scalable financial engine.

---

## 🏗️ Architectural Overview (C4 Model)
FinLedger follows a **Modular Monolith** architecture to ensure strict domain boundaries while maintaining deployment simplicity and high consistency.

### System Context & Container Diagram
```mermaid
C4Container
    title Container Diagram for FinLedger SaaS

    Person(accountant, "Accountant / CFO", "Manages financial records & reports")
    System_Boundary(c1, "FinLedger Platform") {
        Container(spa, "Dashboard", "React + TS", "Financial visualization panel")
        Container_Boundary(backend, "Core Engine (.NET 9)") {
            Component(api, "API Gateway", "Versioning & Rate Limiting", "Entry point for all tenants")
            Component(mod_ledger, "Ledger Core", "Domain Module", "Double-Entry Logic & Invariants")
            Component(mod_report, "Reporting", "CQRS Read Model", "High-performance analytics")
            Component(worker, "Outbox Worker", "Background Service", "Event publishing & reliability")
        }
        ContainerDb(db, "Database", "PostgreSQL 16", "Schema-per-Tenant Strategy")
        ContainerDb(redis, "Cache & Lock", "Redis", "Distributed Locking (RedLock)")
        ContainerQueue(bus, "Event Bus", "RabbitMQ", "Integration Events")
    }
    Rel(accountant, spa, "Uses")
    Rel(spa, api, "API Calls (v1.0)")
    Rel(mod_ledger, db, "ACID Transactions")
    Rel(mod_ledger, bus, "Publishes via Outbox")
    Rel(worker, mod_report, "Materializes Views")
```

> **Strategic Note:** We chose a Modular Monolith over Microservices to maintain **Transactional Integrity** and reduce operational complexity while remaining "Microservices-ready". 
> **[Read more about our Strategic Decisions (ARCHITECTURE.md)](./ARCHITECTURE.md)**

---

## 🚀 Core Features & Technical Excellence

### 🏦 Advanced Financial Engine
- **Immutable Ledger (Zero-Delete Policy):** Implements a high-integrity accounting system where journal entries are finalized (Posted) and cannot be modified or deleted. All corrections are handled through **Automated Reversal Logic**, ensuring a 100% reliable audit trail.
- **Double-Entry Integrity:** The domain layer strictly enforces the fundamental accounting equation (`Sum(Debit) == Sum(Credit)`) as a system invariant, preventing out-of-balance transactions at the core level.

### 🏗️ Enterprise Architecture Patterns
- **CQRS with MediatR:** Clear separation of read and write concerns. Write operations use EF Core for complex business logic, while Read operations use **Dapper** for sub-second reporting performance.
- **Outbox Pattern:** Guarantees transactional consistency. Financial events are captured within the same atomic transaction as the business data, ensuring **Exactly-once processing** and preventing data loss during network failures.

### 🔐 Infrastructure & Resilience
- **Automated Multi-Tenancy:** Uses a sophisticated **Schema-per-Tenant** isolation strategy. The system dynamically creates and migrates database schemas for new tenants on-the-fly, ensuring maximum data privacy and regulatory compliance (GDPR/SOC2).
- **Distributed Locking (RedLock):** Leverages Redis to prevent race conditions during concurrent financial operations, ensuring deterministic states in a horizontally scaled environment.
- **Observability:** 
    - **Structured Logging:** Powered by **Serilog** with JSON formatting for cloud-native log analysis.
    - **Health Monitoring:** Integrated ASP.NET Core Health Checks for PostgreSQL and Redis connectivity.

### 📊 Financial Reporting & Insights
- **Professional PDF Export:** Integrated **QuestPDF** engine to generate audit-ready Trial Balance reports with enterprise-grade layouts and automatic pagination.
- **Demo Data Seeding:** Built-in automated seeding engine to generate complex, balanced financial scenarios for instant testing and demonstration.


### 🛡️ Automated Quality Assurance
FinLedger is guarded by a multi-layered testing suite to ensure financial accuracy and architectural purity:
- **Domain Invariant Protection:** Rigorous testing of the `JournalEntry` aggregate root to prevent unbalanced transactions or illegal state transitions.
- **Architectural Guardrails:** Automated tests that verify modular boundaries, ensuring that the Domain layer remains pure and no prohibited dependencies (e.g., Infrastructure -> Domain) are introduced during development.
- **Application Logic Verification:** Mocking external concerns with **NSubstitute** to verify command handlers, concurrency locks, and reliable messaging (Outbox).



---

## 🕹️ End-to-End Scenario: The Life of a Transaction
To see the system's robustness, consider this flow:
1. **Request:** A Tenant initiates a transfer via the Versioned API.
2. **Concurrency:** A **Redis Lock** is acquired to ensure serialized access to specific accounts.
3. **Validation:** The **MediatR Pipeline** triggers **FluentValidation** followed by Domain-level invariant checks.
4. **Persistence:** The Ledger record and an **Outbox Message** are saved in a single **ACID Transaction**.
5. **Reliability:** The **Background Worker** ensures the event is published even if the primary API process crashes.
6. **Insight:** The **Reporting Engine** extracts data from the isolated schema to produce a professional PDF report.

---

- [ ] **Phase 5: Automated Quality Assurance**
    - [x] **Unit Testing:** 100% coverage of core accounting invariants using **xUnit** and **FluentAssertions**.
    - [x] **Architecture Testing:** Automated enforcement of Clean Architecture (Onion) boundaries and naming conventions using **NetArchTest**.
    - [ ] **Integration Testing:** End-to-End verification of Multi-tenancy and persistence logic using **TestContainers** (PostgreSQL/Redis in Docker).
- [ ] **Phase 6: Advanced Identity & RBAC**
    - Multi-tenant Role-Based Access Control (Admin, Accountant, Auditor).
- [ ] **Phase 7: Observability & Resilience**
    - Distributed Tracing with **OpenTelemetry** and advanced Circuit Breakers.


## 🛠️ Tech Stack 

- **Framework:** .NET 9 (C# 13), MediatR, FluentValidation, Serilog.
- **Testing:** xUnit, FluentAssertions, **NetArchTest**, NSubstitute.
- **Data:** PostgreSQL 16 (Schema Isolation), EF Core 9, Dapper, Redis (RedLock).
- **DevOps:** Docker Compose, QuestPDF, Health Checks.

---
**Status:** 🏆 *Production-Grade Ledger Engine Operational.*
```

---

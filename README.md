# 🏦 FinLedger SaaS
**Cloud-Native Double-Entry Accounting Engine**

> A high-performance, audit-ready financial ledger system designed for multi-tenant SaaS platforms. Built with **.NET 9**, **PostgreSQL 16**, and **Domain-Driven Design (DDD)**.

![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen) ![Runtime](https://img.shields.io/badge/.NET-9.0-blue) ![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-orange)

## 🎯 Problem Statement
Most modern SaaS applications handle financial data using "Anemic Domain Models", leading to **Data Integrity** issues. **FinLedger** bridges the gap by combining deep **Accounting Domain expertise** with **Robust Engineering patterns** to provide an immutable, compliant, and highly scalable financial engine.

## 🏗️ Architectural Excellence
FinLedger is designed as a **Modular Monolith** to ensure strict domain boundaries and high maintainability.

- **[Read the Full Architecture Deep Dive (ARCHITECTURE.md)](./ARCHITECTURE.md)**
- **[View Implementation Trade-offs & Decisions](./ARCHITECTURE.md#2-multi-tenancy-schema-per-tenant)**

### 🕹️ End-to-End Transaction Flow
1. **Request:** Tenant initiates a transfer via Versioned API (v1.0).
2. **Concurrency:** A **Redis Lock** is acquired to ensure serialized access to accounts.
3. **Logic:** The **Domain Layer** validates the Double-Entry balance (Debit == Credit).
4. **Persistence:** The Ledger record and an **Outbox Message** are saved in a single **ACID Transaction**.
5. **Worker:** A background service processes the Outbox to notify external systems.
6. **Reporting:** Optimized **Dapper** queries generate sub-second financial reports.

## 🚀 Key Features & Production Guarantees
- **Immutable Ledger:** No hard deletes. Every correction is an automated **Reversal Entry**.
- **Dynamic Multi-Tenancy:** Automated PostgreSQL schema generation per tenant.
- **Transactional Integrity:** Zero-event-loss guarantee via the **Outbox Pattern**.
- **Observability:** Standardized JSON logging (Serilog) and built-in Health Monitoring.
- **Enterprise Reporting:** Professional PDF exports (QuestPDF) and high-speed JSON analytics.

## 🗺️ Project Roadmap
- [x] **Phase 1-4:** Core Financial Engine, Multi-tenancy, Resilience, and Reporting.
- [ ] **Phase 5: Automated Quality Assurance**
    - Unit Tests for accounting invariants & Integration Tests with **TestContainers**.
- [ ] **Phase 6: Advanced Identity & RBAC**
    - Multi-tenant Role-Based Access Control (Admin, Accountant, Auditor).
- [ ] **Phase 7: Cloud-Native Observability**
    - Distributed tracing with **OpenTelemetry** & Jaeger.

## 🛠️ Tech Stack
- **Backend:** .NET 9 (C# 13), MediatR (CQRS), FluentValidation.
- **Data:** PostgreSQL 16 (Schema Isolation), EF Core 9, Dapper.
- **Resilience:** Redis (RedLock), Outbox Pattern, Serilog.
- **DevOps:** Docker Compose, Health Checks.

## 🚦 Getting Started
1. `docker-compose up -d`
2. Run `FinLedger.Modules.Ledger.Api`.
3. Use `/api/v1/ledger/Reports/seed-demo-data` to generate test data.
4. Access Swagger at `http://localhost:5000/swagger`.

---
**Status:** 🏆 *Core Ledger Engine Operational. Ready for Enterprise integration.*

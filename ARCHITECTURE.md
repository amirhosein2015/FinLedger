# FinLedger Architecture & Engineering Decisions

## 1. Modular Monolith vs. Microservices
**Decision:** We chose a **Modular Monolith** over a distributed Microservices architecture.
**Reasoning:** 
- **Operational Complexity:** For a core ledger system, maintaining ACID transactions across microservices (using Sagas) introduces significant overhead and failure modes.
- **Team Velocity:** Modular boundaries allow teams to work independently on different contexts (Identity, Ledger, Reporting) without the network latency and deployment complexity of microservices.
- **Future-Proof:** Each module is physically and logically decoupled, making it "Microservices-ready" if the scale eventually justifies the cost.

## 2. Multi-Tenancy: Schema-per-Tenant
**Decision:** We implemented isolation at the **PostgreSQL Schema level**.
**Trade-offs:**
- **Shared DB (Low Cost):** More cost-effective than Database-per-tenant.
- **Strict Isolation (Security):** Provides much stronger data privacy than simple `TenantId` columns (Row-Level Security), which is critical for financial compliance (GDPR/SOC2).
- **Maintenance:** Handled via automated migrations and a custom `IModelCacheKeyFactory` to ensure EF Core manages schema-specific models correctly.

## 3. Reliability: The Outbox Pattern
**Decision:** All integration events are persisted via the **Outbox Pattern**.
**Why:**
- Avoids the **"Dual Write" problem**. In financial systems, we cannot afford to save a transaction but fail to notify the downstream services (like Tax or Auditing).
- Ensures **At-least-once delivery** even if the Message Broker (RabbitMQ) is temporarily unavailable.

## 4. Concurrency: Distributed Locking (RedLock)
**Decision:** Redis-based distributed locking for sensitive operations.
**Impact:** 
- Prevents **Race Conditions** when multiple API instances attempt to modify the same account balance simultaneously.
- Ensures deterministic system state in a horizontally scaled environment.

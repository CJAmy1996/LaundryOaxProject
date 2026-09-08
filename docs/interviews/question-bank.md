# LaundryOax interview question bank

173 questions and answer guides for software engineering and infrastructure operations interviews.

Code baseline: [545e6a3](https://github.com/CJAmy1996/LaundryOaxProject/commit/545e6a3521c6b8d744e8393e9784b478251a7859). Review date: 2026-09-05.

Use these as speaking guides, not a script claiming experience you have not had. **Implemented** means code exists; **verified** is limited to the cited local evidence. **Deployment design**, **proposed**, and **hypothetical** are not completed production work. No finite question list can anticipate every interviewer: use the unknown-question method in the study plan.

[Start here](README.md) · [Claim audit](claim-audit.md) · [Architecture](architecture.md) · [Operations runbook](operations-runbook.md) · [Study plan](study-plan.md)

## Topic index

- [Project story and honest scope](#project-story-and-honest-scope) — 15 questions
- [Architecture and distributed systems](#architecture-and-distributed-systems) — 15 questions
- [Edge processing and recovery](#edge-processing-and-recovery) — 14 questions
- [C sharp ASP NET Core and APIs](#c-sharp-asp-net-core-and-apis) — 13 questions
- [Data modeling transactions and concurrency](#data-modeling-transactions-and-concurrency) — 14 questions
- [Authentication authorization and security](#authentication-authorization-and-security) — 15 questions
- [AWS MQTT and SQS](#aws-mqtt-and-sqs) — 13 questions
- [Infrastructure networking Docker and Terraform](#infrastructure-networking-docker-and-terraform) — 14 questions
- [Operations observability incidents and recovery objectives](#operations-observability-incidents-and-recovery-objectives) — 15 questions
- [Angular user experience and notification semantics](#angular-user-experience-and-notification-semantics) — 11 questions
- [Testing CI CD and quality](#testing-ci-cd-and-quality) — 10 questions
- [System design calculations and technical challenges](#system-design-calculations-and-technical-challenges) — 14 questions
- [Behavioral questions and practical exercises](#behavioral-questions-and-practical-exercises) — 10 questions

## Project story and honest scope

### Q001 What is LaundryOax and what problem does it solve?

**Evidence status: Implemented plus deployment design.**

It is a portfolio IoT fleet platform for observing washer and dryer cycles when connections are unreliable. It captures telemetry, projects the latest ordered state, estimates remaining work, and displays tenant-scoped fleet information. The working demonstration uses simulated machines, an ASP.NET Core API, Angular, and SQLite. AWS deployment configuration is present but has not been exercised in a real account.

**Code or follow-up:** [README.md](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/README.md) · [IoT/DemoEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/DemoEndpoints.cs)

### Q002 Give me a thirty second summary.

**Evidence status: Verified local scope.**

LaundryOax demonstrates reliable telemetry processing across an unreliable network boundary. An edge SQLite outbox retains events until an application acknowledgment. The API persists an inbox, and a worker applies events in device sequence while committing notifications with state. I can show local recovery and isolation tests; I would describe the AWS infrastructure as an undeployed design, not a production fleet.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q003 What would you emphasize for software engineering?

**Evidence status: Implemented.**

I would explain the event contract, transaction boundaries, deterministic sequencing, duplicate rejection, dependency lifetimes, API authorization, and test design. The clearest code walkthrough is Capture → Accept → Process. I would also explain the costs: serialized local writers, missing schema versioning, a single Angular component, and no complete cycle transition model.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q004 What would you emphasize for infrastructure operations?

**Evidence status: Deployment design.**

I would trace ownership of an event through edge disk, transport, queue, durable inbox, and processed state. Then I would discuss health checks, IAM boundaries, TLS termination, queue age alarms, deployment rollback, and restore reconciliation. I would distinguish the checked Terraform configuration from an actually provisioned and monitored AWS environment.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf) · [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs)

### Q005 Was this running in production or on real appliances?

**Evidence status: Verified local scope.**

The evidence supports a local simulated demonstration, not a production deployment or real sensor integration. Eight seeded machine records are demo data. No production traffic, customer count, availability history, appliance certification, measured savings, or physical laundry installation has been established. I would not attach business or scale claims to that demonstration.

**Code or follow-up:** [IoT/DemoEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/DemoEndpoints.cs) · [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs)

### Q006 What did you personally do and did you use AI?

**Evidence status: Personal answer to adapt.**

Separate the existing project history from the recent AI-assisted extension. A truthful answer is that this is my project and I used AI assistance to extend it, then identify exactly what I personally reviewed, ran, changed, and can explain. Do not claim manual authorship of every line or personal execution of tests you have only read about. Demonstrate understanding with a walkthrough and your own reproduction.

**Code or follow-up:** [README.md](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/README.md) · [git history](https://github.com/CJAmy1996/LaundryOaxProject/commits/545e6a3521c6b8d744e8393e9784b478251a7859) · [claim-audit.md](claim-audit.md)

### Q007 What is the strongest measurable result?

**Evidence status: Verified local scope.**

Ten automated tests passed locally, and a smoke test accepted and processed eight simulated telemetry events with zero pending at that observation. API, edge, and Angular builds succeeded. These are validation results, not throughput benchmarks or uptime guarantees. Explain the test conditions before giving the numbers.

**Code or follow-up:** [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs) · [validation-record.md](validation-record.md)

### Q008 Why did you choose this technology stack?

**Evidence status: Implemented plus design rationale.**

ASP.NET Core supplies hosting, dependency injection, middleware, and authentication integration; EF Core maps the fleet data to local SQLite or configured SQL Server. Angular provides the operator console. SQLite avoids requiring a database service at the edge, while SQS provides a proposed cloud buffering boundary. This is a defensible design rationale, not a measured comparison against every alternative.

**Code or follow-up:** [LaundryOaxWebAPI.csproj](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/LaundryOaxWebAPI.csproj) · [client/package.json](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/package.json) · [edge/LaundryOax.Edge.csproj](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/LaundryOax.Edge.csproj)

### Q009 What was inherited and what was added?

**Evidence status: Implemented.**

The repository already had a .NET 6 laundry business API and legacy EF migrations. The extension targets .NET 8 and adds the IoT models, pipeline, edge agent, Angular console, infrastructure, and tests. The broken order service was replaced with validated persistence; no payment processor was implemented. Legacy controllers remain disabled by default.

**Code or follow-up:** [Services/OrderServices/OrderService.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Services/OrderServices/OrderService.cs) · [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [Migrations/](https://github.com/CJAmy1996/LaundryOaxProject/tree/545e6a3521c6b8d744e8393e9784b478251a7859/Migrations/)

### Q010 What is the most important tradeoff?

**Evidence status: Implemented.**

The implementation favors durable, ordered processing over immediate progress when an event is missing. A later event stays pending rather than silently skipping a sequence. That protects ordering but creates a recovery obligation: operators need the missing event, a carefully designed resynchronization procedure, and monitoring that distinguishes sequence gaps from simple worker backlog.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)

### Q011 Is this a microservices system?

**Evidence status: Implemented.**

It has separate edge and server processes, but the API, inbox worker, and SQS receiver share one ASP.NET Core host. It is more accurate to call it an event-driven application with distributed boundaries than a fully separated microservice fleet. Splitting workers would require explicit deployment ownership, workload partitioning, and integration tests.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q012 What would you build next?

**Evidence status: Proposed.**

First validate production boundaries: real OIDC, SQL Server concurrent workers, Docker runtime, and AWS message delivery. Then add FleetDb migrations, schema versioning, gap recovery, per-user notification modeling if needed, retention, and backup/restore drills. Prioritize correctness and operability before scaling claims or predictive analytics.

**Code or follow-up:** [claim-audit.md](claim-audit.md)

### Q013 What did this project teach you?

**Evidence status: Personal answer to adapt.**

A useful answer is that an acknowledgment must name the guarantee it represents, and that a transaction does not span an entire distributed workflow. The edge retains responsibility until the API commits; the worker separately commits state. Adapt this reflection to your own learning and be prepared to prove it from code rather than recite it as a personal experience you have not had.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q014 How would you explain it to a nontechnical stakeholder?

**Evidence status: Implemented.**

Think of each machine keeping a numbered notebook during an outage. When the connection returns, the server signs for each entry only after saving it, ignores exact copies, and reads entries in order. The analogy has limits: damaged storage, missing entries, and expired backups still require recovery planning. The project demonstrates this workflow locally.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q015 What claims would you deliberately avoid?

**Evidence status: Evidence boundary.**

Avoid zero data loss under all failures, exactly-once delivery, proven horizontal scale, deployed AWS operations, automated backups, calibrated prediction, customer-specific messaging, complete observability, or a production security certification. Use the narrower claims in the claim audit, with explicit evidence and limits.

**Code or follow-up:** [claim-audit.md](claim-audit.md)


## Architecture and distributed systems

### Q016 Walk through the complete working path.

**Evidence status: Implemented.**

Capture allocates a device sequence and writes its event in one SQLite transaction. Replay sends the stored JSON to /api/telemetry. Device authentication binds the caller to a tenant and device. Accept saves an inbox event and audit entry before returning a receipt. Process applies the next sequence and commits machine state, audit, and any completion or fault notification together. The dashboard polls the resulting projection.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q017 Where are the durability boundaries?

**Evidence status: Implemented.**

There are three distinct commits: the edge outbox transaction, the API inbox SaveChanges call, and each worker processing transaction. A successful receipt means the inbox committed; it does not mean the machine projection or notification is already updated. The SQS adapter uses that same inbox commit as its delete boundary.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q018 Do you provide exactly-once processing?

**Evidence status: Implemented with limits.**

I would say at-least-once delivery attempts with idempotent database effects within the implemented identity and transaction boundaries. The database prevents a retained event from applying twice, but that is not a universal exactly-once delivery guarantee. Restoring old state, removing deduplication records, or adding an external notification provider creates new reconciliation boundaries.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs)

### Q019 Why not process everything synchronously in the request?

**Evidence status: Implemented.**

Persisting first keeps acknowledgment separate from projection work, and a worker can retry after the request finishes. However, Accept and Process share a process-wide semaphore, so requests can still wait behind a processing batch. The current design improves responsibility separation without proving independent resource isolation or low latency under load.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q020 What is eventually consistent here?

**Evidence status: Implemented.**

The API can acknowledge an event before the worker updates the machine. The browser refreshes later, so receipt, projection, and display have different observation times. The snapshot endpoint also makes several separate queries without a shared read transaction; under concurrent writes, its counters and lists can briefly reflect slightly different database states.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q021 How are events ordered?

**Evidence status: Implemented.**

The worker joins pending events to their machine and only selects an event whose sequence equals LastSequence + 1. Ordering is per tenant and device, not global. ReceivedUtc chooses among eligible events; it is not the correctness mechanism. A gap on one machine does not logically block another machine's eligible sequence.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q022 Why use sequence numbers instead of timestamps?

**Evidence status: Implemented.**

Capture order is explicit in a persisted integer counter. Device clocks can drift or be corrected, and delayed events can arrive out of order, so timestamps are unsuitable as the only ordering key. ObservedAt remains useful for freshness and analysis, but it is not used to decide the next event to apply.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q023 How are duplicates detected?

**Evidence status: Implemented and tested.**

The inbox primary key is TenantId plus EventId. Accept hashes the serialized typed event and compares that hash when the key already exists. Matching content gets a duplicate receipt; conflicting content produces a conflict. A separate unique index on tenant, device, and sequence prevents two distinct events from owning the same sequence.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs)

### Q024 Does the hash authenticate an event?

**Evidence status: Implemented.**

No. SHA-256 here detects content reuse under an existing event identity; it is not a signature or an HMAC. Device authentication and TLS provide separate trust boundaries. The hash is over reserialized typed data, not the original wire bytes, so it should not be described as byte-for-byte verification of the transport payload.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs)

### Q025 What happens if sequence two arrives before sequence one?

**Evidence status: Implemented and tested.**

Accept can persist sequence two, but Process finds no eligible event for that device until sequence one arrives. After one is accepted, a worker pass applies both in sequence. A test asserts that no processing occurs before the gap is filled and that completion creates one notification afterward.

**Code or follow-up:** [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q026 What happens if the missing sequence never arrives?

**Evidence status: Implemented limitation.**

Later events remain pending indefinitely in this version. There is no gap timeout, skip policy, resynchronization endpoint, or pending-event DLQ. I would investigate the edge and ingress history first. Any future snapshot/epoch recovery must preserve an audit trail and explicitly define what historical events were lost or superseded.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q027 Are these event sourcing and CQRS?

**Evidence status: Implemented.**

There is an event inbox and a separate machine read projection, which resembles a small CQRS pattern. It is not a complete event-sourcing platform: no event schema versioning, replay administration, retention contract, aggregate snapshotting, or notification-safe full rebuild procedure exists. Calling it an inbox with a projection is more precise.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q028 What consistency model applies during a network partition?

**Evidence status: Design reasoning grounded in code.**

An edge process can continue local capture if storage is healthy, while cloud views become stale. The design intentionally permits that divergence until replay. It is not appropriate to label the entire system simply AP or CP: local capture, cloud acknowledgment, and ordered projection have distinct availability and consistency boundaries.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q029 Can a fleet database restore lose already acknowledged events?

**Evidence status: Implemented limitation.**

Yes. An edge deletes after inbox acknowledgment, so restoring the fleet database to a point before that commit can remove the only remaining application copy. SQS may also have been deleted already. Point-in-time recovery, protected event archives, and a replay/reconciliation strategy are needed; local outbox persistence alone is not a cross-system backup guarantee.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q030 How would you scale to many tenants?

**Evidence status: Proposed.**

First measure ingestion, database lock waits, query costs, and queue lag. Then partition work by tenant/device, improve eligible-event indexes and claiming, separate receiver and projector deployments, and add shared quotas. SQL Server is configurable, but multi-replica behavior is not locally proven. Scaling SQLite instances against a shared network file is not the intended path.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)


## Edge processing and recovery

### Q031 Why choose SQLite at the edge?

**Evidence status: Implemented.**

It offers embedded transactional persistence without requiring a local database server. This edge uses WAL and synchronous FULL, and capture commits the counter and event atomically. That reduces crash-related inconsistency when the filesystem honors durability requests. It does not protect against destroyed disks, exhausted capacity, malicious writes, or an incorrect sensor driver.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)

### Q032 What is actually buffered?

**Evidence status: Implemented.**

Immutable serialized telemetry including EventId, tenant, device, sequence, observation time, phase, progress, temperature, moisture, and turbidity. The outbox also stores attempts and the next eligible retry timestamp. Separate persistent sequence rows survive acknowledgment deletes so the counter does not restart when the outbox becomes empty.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs)

### Q033 How are sequence numbers allocated safely?

**Evidence status: Implemented and tested.**

Capture starts a transaction, inserts or increments the sequence row using an upsert with RETURNING, validates the resulting event, then inserts its payload before committing. Failure rolls back both operations. A test proves invalid capture does not consume sequence one. The counter is scoped by tenant and device.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q034 When can an edge record be deleted?

**Evidence status: Implemented.**

Only after a successful HTTP response whose eventId matches the pending event. The server sends that response after durable inbox persistence. The agent does not delete merely because bytes were sent, a socket connected, or a broker acknowledged transport receipt. A malformed or mismatched acknowledgment leads to a retained event and retry.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q035 What if the API commits but its acknowledgment is lost?

**Evidence status: Implemented with test boundary.**

The edge retains the record and sends the same identity and payload again. Accept finds the matching committed event and returns Duplicate=true, allowing the edge to delete safely. Duplicate submission and idempotent processing are tested; an actual network-induced lost-ACK experiment is not part of the ten-test suite.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q036 What is the exact retry policy?

**Evidence status: Implemented.**

The edge delay is min(300000, 1000 times 2 raised to min(attempts,18)) milliseconds plus random jitter from zero through 999 milliseconds. The retry count and next timestamp are persisted. This is capped exponential delay with additive jitter, not full jitter, and the total delay can be slightly greater than five minutes.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)

### Q037 Can later events bypass an earlier retry?

**Evidence status: Implemented.**

Next excludes an event when an earlier sequence remains in that device's outbox, even if the later event itself is eligible. This deliberately preserves delivery order per device and can cause head-of-line blocking. Other devices sharing the outbox may still have eligible records, although the reference HTTP client uses one configured credential.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs)

### Q038 How do you handle permanent failures?

**Evidence status: Implemented limitation.**

The reference edge retains all unsuccessful HTTP responses and retries them with backoff, including 4xx responses. It logs that provisioning or payload repair is needed. There is no edge quarantine, retry ceiling, Retry-After handling, or operator repair UI. A production agent should distinguish transient transport failure from invalid identity or irreparable payloads.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs)

### Q039 What if the edge disk fills up?

**Evidence status: Proposed response.**

Capture cannot commit, so the new reading is not durably captured. The current CLI does not implement capacity thresholds, eviction, alternate storage, or operator alerting. I would monitor free space and outbox age, reserve headroom, and define a domain-approved overload policy. Silently dropping the oldest event would break sequence-based replay.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs)

### Q040 What if the device loses its sequence database?

**Evidence status: Implemented limitation.**

Reusing the same identity with sequence one will conflict with previously applied events. Do not delete cloud history or reset LastSequence casually. The current safe provisioning approach is a new device identity. A more mature protocol could include a server-authorized epoch and explicit state reconciliation, which this contract does not yet support.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q041 Can two capture processes share an edge database?

**Evidence status: Inference and test gap.**

SQLite transactions and the composite sequence key can serialize allocation, but that particular concurrency scenario is not tested. Two replay processes do not have an outbox lease and can select the same record. The server should deduplicate the duplicate, yet the supported operational assumption should remain one replay process per device store until claiming and concurrency tests are added.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q042 Does the edge agent use MQTT?

**Evidence status: Implemented boundary.**

No. The included executable uses HttpClient and application-level HTTP receipts. MQTT is represented by the AWS IoT rule and SQS receiver on the cloud side. A real MQTT edge publisher and downstream durable acknowledgment subscriber are absent. Describing the current edge as a tested MQTT client would be inaccurate.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q043 What does the edge restart test prove?

**Evidence status: Verified test boundary.**

It constructs a new Outbox object against the same file, checks that records remain, checks sequence continuation, persists retry timing, and acknowledges records. It verifies storage reopening and counter behavior. It does not kill a process during fsync, simulate a power cut, or exercise a real HTTP reconnect. Those are further failure tests.

**Code or follow-up:** [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q044 How are shutdown and timeouts handled?

**Evidence status: Implemented.**

The HTTP client uses a twenty-second timeout. Ctrl+C cancels a linked workflow via a CancellationTokenSource, and the loop exits without intentionally deleting the pending event. Next retry times use wall-clock milliseconds, so large clock jumps can delay or accelerate retries; a production service needs clock monitoring and a deliberate persisted scheduling policy.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)


## C sharp ASP NET Core and APIs

### Q045 Why use a record for Telemetry?

**Evidence status: Implemented.**

The positional C# record expresses a fixed event contract with value-oriented members and supports test copies using with expressions. Captured events are intended to be immutable after persistence. Records do not replace validation, authentication, schema evolution, or database constraints. The current contract lacks an explicit schema version and cycle identifier.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q046 What are the main dependency lifetimes?

**Evidence status: Implemented.**

EventPipeline is a singleton, allowing its semaphore to coordinate local pipeline operations. Hosted services are long-lived. They obtain short-lived FleetDb instances through IDbContextFactory rather than storing one scoped context forever. The legacy OrderService and its database context are scoped. A singleton DbContext would be unsafe for concurrent operations.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q047 Why create a fresh DbContext for work units?

**Evidence status: Implemented.**

Each Accept and each processing iteration gets a new context with a bounded change tracker and clear disposal. A separate verification context is also used after an insert race to inspect the winning event. This keeps failed or stale tracking state from contaminating a later unit of work.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q048 What does async buy you here?

**Evidence status: Implemented.**

Awaiting database and network I/O can release the request thread while the operation waits; it does not make a CPU-bound computation inherently faster. SemaphoreSlim.WaitAsync coordinates without blocking a thread on a synchronous lock. The edge's SQLite calls are synchronous, and the server's process-wide gate still limits concurrent pipeline work.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)

### Q049 How are BackgroundService failures handled?

**Evidence status: Implemented.**

IngestionWorker logs an exception and tries another processing pass after a 500 ms delay. SqsWorker retries an outer receive failure after five seconds and retains failed messages. This is not a circuit breaker, adaptive database retry strategy, or independent worker process. Cancellation is treated separately so shutdown can complete.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q050 Which telemetry status codes should a client expect?

**Evidence status: Implemented.**

Missing or invalid device credentials produce an authentication failure; a valid credential with a different tenant/device is forbidden. Invalid readings produce 400, unregistered devices 404, and recognized identity or sequence conflicts 409. Success is 200 with a receipt, and rate limiting is 429. Some cross-replica database conflicts can still surface as 500 because not every constraint exception is translated.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs)

### Q051 Why is the response 200 rather than 202?

**Evidence status: Design reasoning.**

The response acknowledges completed inbox persistence, not completed projection work. That makes a receipt-oriented 200 reasonable. A 202 design could also work if its contract clearly named the asynchronous operation and status lookup. The critical property is that neither code implies the browser projection already changed.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q052 Where is request validation performed?

**Evidence status: Implemented.**

Minimal API binding creates the Telemetry record; TelemetryRules checks required identity, positive sequence, allowed phase, finite sensor ranges, and observation time. Accept additionally checks appliance kind and registration. Machine registration has explicit string/type checks. Database checks cover selected invariants, not every sensor range or business rule.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs)

### Q053 What is the request size limit?

**Evidence status: Implemented.**

Kestrel is configured for a 32 KiB maximum request body. That protects the HTTP ingestion surface from oversized individual requests, but does not bound total stored history or SQS message processing in the same way. Queue-side payload contracts and retention limits need their own enforcement.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q054 How does middleware ordering affect security?

**Evidence status: Implemented.**

Forwarded scheme information is processed before redirect/HSTS behavior. Authentication precedes authorization and rate limiting, allowing tenant claims to key the limiter. Static dashboard assets are public, while data endpoints require policies. Production configuration must still be tested with the actual proxy and issuer; merely registering middleware is not proof of secure deployment.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs)

### Q055 How are API exceptions represented?

**Evidence status: Implemented.**

AddProblemDetails and UseExceptionHandler provide a generic error path, while selected endpoint exceptions map to specific results. There is no custom error taxonomy or complete correlation scheme. The client falls back to a generic status message when a response does not include its expected error property. Improve diagnostics without exposing payloads or credentials.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q056 Is every endpoint described by Swagger safe to use?

**Evidence status: Implemented limitation.**

No. Swagger is enabled only in Development, and the legacy controller code is separate from the fleet endpoints. Legacy mapping is disabled unless Legacy:Enabled is set and then requires Admin. Existing placeholder business/auth flows must not be inferred to work merely because their classes or descriptions exist.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [Controllers/](https://github.com/CJAmy1996/LaundryOaxProject/tree/545e6a3521c6b8d744e8393e9784b478251a7859/Controllers/)

### Q057 What are the current API design limitations?

**Evidence status: Implemented limitation.**

There is no explicit API version, no event schema version, no pagination for the full machine list, and latest events/notifications are capped at thirty. Unknown extensionless paths can fall through to the SPA fallback, which can return HTML where a caller expected an API 404. A production API should make these contracts explicit.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)


## Data modeling transactions and concurrency

### Q058 What are the fleet tables?

**Evidence status: Implemented.**

Machines stores the current projection keyed by tenant/device. Events stores the durable inbox keyed by tenant/event ID, with unique tenant/device/sequence. Notifications stores tenant-scoped cycle messages keyed by tenant/event ID. Audit stores accepted, processed, and registration actions. The older customer/order schema remains in a separate DbContext.

**Code or follow-up:** [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs) · [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [Data/LaundryOaxDBContext.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Data/LaundryOaxDBContext.cs)

### Q059 Which writes must be atomic?

**Evidence status: Implemented.**

At capture, counter allocation and payload insertion are atomic. At receipt, inbox and accepted-audit rows are saved together. At processing, machine update, processed marker, processed audit, and an optional notification share one explicit serializable transaction. HTTP receipt delivery and SQS deletion remain outside that database transaction and rely on idempotent retry.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q060 Why use a unique constraint instead of checking first?

**Evidence status: Implemented.**

A preliminary query cannot alone prevent two independent API replicas from inserting the same identity concurrently. The database unique keys arbitrate the race. The code performs prechecks for helpful errors, then inspects a matching winning row if SaveChanges fails. A conflicting winner can still produce a generic database error rather than a polished 409.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs)

### Q061 Is SemaphoreSlim a distributed lock?

**Evidence status: Implemented boundary.**

No. The semaphore only coordinates one EventPipeline instance inside one process. Shared SQL Server constraints, transaction isolation, and LastSequence concurrency checks provide the intended cross-replica protection. The local parallel-delivery test runs against one singleton and does not establish distributed locking behavior.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q062 What does the LastSequence concurrency token do?

**Evidence status: Implemented.**

EF uses the originally read LastSequence when updating the machine. A competing change causes the update to fail rather than silently overwrite a newer projection. The explicit serializable transaction also constrains competing work. The worker retries on a later pass, but SQL Server deadlock and contention behavior still needs integration testing.

**Code or follow-up:** [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q063 Why serializable isolation?

**Evidence status: Implemented plus proposed optimization.**

The worker reads eligibility and then updates related rows, so serializable isolation is an understandable correctness-first choice. The tradeoff is lock contention and possible deadlocks under concurrent SQL Server workers. Keep transactions short and benchmark before choosing a more specialized claiming or partitioning scheme. The current loop does not have a distributed lease or SKIP LOCKED strategy.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q064 Does the same EF code prove SQLite and SQL Server behave identically?

**Evidence status: Test boundary.**

No. Provider support is configured, but locking, execution plans, migrations, type behavior, and concurrency differ. The tests use SQLite only. Before deployment, run the same invariants against SQL Server and add concurrent accept/process, failure injection, and transaction retry tests with actual production-like settings.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q065 How are migrations handled?

**Evidence status: Implemented limitation.**

FleetDb currently uses EnsureCreated for an empty database when explicitly initialized or in demo mode. It has no versioned migration sequence. The existing migrations target the legacy context and cannot be treated as fleet migrations. Establish reviewed FleetDb migrations and deployment sequencing before changing an already deployed fleet schema.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [Migrations/](https://github.com/CJAmy1996/LaundryOaxProject/tree/545e6a3521c6b8d744e8393e9784b478251a7859/Migrations/) · [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs)

### Q066 Can the event log be rebuilt into state?

**Evidence status: Proposed.**

Conceptually the immutable payloads allow a deterministic projection rebuild in sequence. The repository has no supported rebuild command. Resetting processed markers directly can collide with existing notifications and repeat side effects, so a safe rebuild should use a new projection version or isolated database and an explicit notification policy.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs)

### Q067 What indexes exist and what might be missing?

**Evidence status: Implemented.**

There are identity/sequence keys, an index on ProcessedUtc, tenant/time notification indexing, and tenant/time audit indexing. The worker joins by device and orders eligible events by receipt time. The best additional composite or filtered index depends on real query plans and cardinalities; no benchmark here demonstrates an optimal index set.

**Code or follow-up:** [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q068 How would you manage retention?

**Evidence status: Proposed.**

Define retention for raw events, deduplication identities, audit records, and notifications separately. If old events can still replay, deleting their deduplication history changes the acceptance contract. Archive by tenant and age, preserve the required identity horizon or device epochs, and test late replay before enabling deletion. There is no retention job in this implementation.

**Code or follow-up:** [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q069 Is the API protected against SQL injection?

**Evidence status: Implemented.**

EF-generated queries and the edge's parameterized commands avoid constructing SQL from sensor values. That is the concrete implemented protection. It does not address authorization bugs, overly privileged database credentials, unreviewed future raw SQL, or malicious data interpreted by another system such as a CSV viewer.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)

### Q070 What happens when a reading is older than the last timestamp but has the next sequence?

**Evidence status: Implemented limitation.**

The worker applies it because sequence is authoritative. LastSeenUtc is assigned ObservedAt, so apparent freshness can move backward. The code does not enforce monotonic observation time. That may be acceptable for ordered capture, but the UI should distinguish observed time, last receipt time, and actual connectivity.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q071 How would you evolve the event contract?

**Evidence status: Proposed.**

Add a schemaVersion and define compatibility rules before changing meaning. Preserve the immutable original event and hash strategy; upcast older events in a controlled processor rather than mutating existing rows. Test old edge versions and new server versions together. Rejecting or reinterpreting historical payloads can stall a device's sequence stream.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)


## Authentication authorization and security

### Q072 How do user and device identities differ?

**Evidence status: Implemented.**

Users normally authenticate through JWT bearer tokens from a configured OIDC issuer, with tenant_id and roles claims. Devices use a separate DeviceKey scheme that maps a random key hash to one tenant/device pair. The telemetry endpoint explicitly requires that device scheme, so a normal user token is not enough to impersonate a machine.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q073 Explain OAuth OIDC JWT and PKCE in this project.

**Evidence status: Configured but unverified externally.**

The SPA is configured for an authorization-code flow through oidc-client-ts, which supplies PKCE behavior. The API validates bearer tokens using issuer metadata and an expected audience. OIDC handles the sign-in identity layer, while API authorization uses access-token claims. This wiring exists, but no external issuer integration has been validated in the recorded local tests.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts) · [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs)

### Q074 Where are browser tokens stored?

**Evidence status: Implemented.**

The user store is an InMemoryWebStorage instance, and the access token is copied into an in-memory component field. The configured OIDC transaction state store uses the library default rather than the same explicit user store. Reloading loses the in-memory user token and starts a new sign-in flow; automatic silent renewal is disabled.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q075 How do you prevent cross-tenant reads?

**Evidence status: Implemented and tested.**

Fleet endpoints derive TenantId from the authenticated principal and filter queries or use composite keys that include it. Registration takes tenant identity from the principal rather than a submitted tenant field. There are negative tests for another tenant's machine list and notification access. This is application-enforced isolation, not database row-level security or a global query filter.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q076 What roles exist?

**Evidence status: Implemented.**

FleetRead accepts Viewer, Operator, or Admin with an authenticated tenant claim. Registration, demo tick, and audit access require Operator or Admin. Notification read acknowledgment uses FleetRead, so Viewers can mark tenant notifications read. Device authorization uses its separate scheme. LegacyAdmin is a separate policy without a tenant requirement because the legacy schema is not tenant-aware.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q077 Can one user mark another user's notification read?

**Evidence status: Implemented limitation.**

Notifications have no user recipient field. Read is stored on the tenant notification itself, so an authorized tenant Viewer can mark it read for the shared tenant view. This is a tenant operations inbox, not personal delivery tracking. Per-user recipients and acknowledgments would require a separate notification/user association.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q078 Why store device key hashes and compare in constant time?

**Evidence status: Implemented plus operational work.**

The server can verify a high-entropy random key without storing its cleartext value, and FixedTimeEquals avoids a simple variable-duration byte comparison. A fast hash is appropriate only when keys have sufficient entropy; this is not a password hashing scheme. Key provisioning, rotation, revocation, and secure edge storage still need operational controls.

**Code or follow-up:** [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs) · [README.md](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/README.md)

### Q079 Does a valid device key prove that sensor data is physically true?

**Evidence status: Implemented boundary.**

No. It proves possession of a credential mapped to a device identity. Range checks reject malformed or implausible values, but a compromised or faulty device can submit plausible false readings. Hardware attestation, anomaly detection, firmware trust, and cross-sensor validation are not implemented.

**Code or follow-up:** [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs) · [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs)

### Q080 Is everything encrypted?

**Evidence status: Implemented and deployment-dependent.**

No blanket claim is justified. Remote edge HTTP is restricted to HTTPS, production config includes HSTS/HTTPS redirection, and Terraform defines KMS encryption for queues and encrypted ECR storage. Local SQLite files are not application-encrypted, ALB-to-task traffic is configured as HTTP, and SQL encryption depends on the supplied connection string and database configuration.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q081 What does TLS termination change?

**Evidence status: Deployment design.**

The proposed ALB accepts HTTPS on 443 and forwards HTTP to task port 8080. ASP.NET Core must trust forwarded scheme information only from the configured proxy/network so it interprets the original request correctly. This design does not provide TLS on the internal ALB-to-task hop. Real proxy behavior and health checks need a deployment test.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q082 Is CORS an authentication boundary?

**Evidence status: Implemented.**

No. CORS controls browser cross-origin access behavior; non-browser clients can still issue requests. The API explicitly allows the local Angular development origin. Real authorization remains JWT/device authentication plus policies and tenant filtering. A separately hosted production SPA also needs deliberate origin configuration rather than an assumption that localhost settings suffice.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs)

### Q083 How safe is demo authentication?

**Evidence status: Implemented.**

It requires Development and a loopback remote address and grants a fixed demo tenant/operator identity. Startup rejects Demo enabled outside Development. A local forwarding proxy could make remote requests appear loopback, so do not expose demo mode. Test-only X-Test-Tenant and X-Test-Role authentication exists only in the test assembly, not the production handler.

**Code or follow-up:** [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs) · [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q084 Is the audit log tamper-proof?

**Evidence status: Implemented limitation.**

No. Audit entries are ordinary database rows written alongside selected operations. They do not record every read or denied request, and no immutable sink, signing, independent retention, or comprehensive actor attribution is implemented. For stronger accountability, include authenticated actor/correlation identifiers and export to a separately controlled append-only destination.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q085 What are the rate limiting limitations?

**Evidence status: Implemented.**

The fixed window is 600 requests per minute per tenant or fallback remote address, per API process, with no queue. Devices within a tenant share that budget, and each additional replica adds its own limiter. It is not a global quota or per-device fairness policy. SQS ingestion bypasses the HTTP limiter altogether.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q086 What would you improve before a security review?

**Evidence status: Proposed.**

Validate issuer/audience/role mapping with real tokens; add threat-driven authorization tests, operational key rotation, strict deployment TLS settings, content security policy, least-privilege database access, and immutable audit export as required. Review fallback routing, oversized queue payloads, tenant fairness, and accidental publication of data. A clean dependency scan is only one input, not proof of overall security.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs) · [.github/workflows/iot-ci.yml](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/.github/workflows/iot-ci.yml)


## AWS MQTT and SQS

### Q087 What AWS resources are defined?

**Evidence status: Undeployed infrastructure.**

Terraform defines an encrypted standard telemetry queue and DLQ, IoT rule/roles/device policy attachments, queue alarms, ECR repository, log group, ECS cluster, and IAM roles. An optional deployment object adds Fargate service/task definitions, security groups, and an HTTPS ALB. The database, VPC, certificate issuance, OIDC provider, and alert destinations are supplied externally.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q088 Why a standard SQS queue rather than FIFO?

**Evidence status: Deployment design.**

The configured integration is the native AWS IoT SQS rule action, which does not support FIFO queues. The application therefore handles duplicates and ordering using event identities and per-device sequences. Switching to FIFO would require a different ingress integration and would not eliminate the need to make database effects idempotent.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [official AWS IoT SQS action documentation](https://docs.aws.amazon.com/iot/latest/developerguide/sqs-rule-action.html)

### Q089 How does the MQTT topic enforce identity?

**Evidence status: Undeployed infrastructure.**

Per-device IoT policies allow a specific client ID and publication to an exact tenant/device telemetry topic. The IoT rule reconstructs TenantId from topic segment two and DeviceId from segment four instead of trusting those JSON fields. This relies on certificates, policy attachment, and no broader conflicting publish permissions being configured correctly in AWS.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q090 What MQTT QoS is implemented?

**Evidence status: Not implemented at edge.**

There is no MQTT client in the reference edge executable and no tested QoS setting to report. QoS is a transport-level choice for a future publisher. Even a broker PUBACK would not establish durable fleet inbox persistence, so a future MQTT edge transport needs a downstream receipt protocol before removing its local event.

**Code or follow-up:** [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q091 What exactly does the SQS worker do?

**Evidence status: Implemented but AWS-unverified.**

If Aws:QueueUrl is configured, it creates an SDK SQS client, long-polls for up to ten messages, deserializes each typed event, calls Accept, and deletes each successfully persisted message. It handles the batch sequentially. It does not apply cycle state itself; the separate inbox worker does that later.

**Code or follow-up:** [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q092 What is the visibility timeout and why does it matter?

**Evidence status: Configured but unverified.**

The worker requests sixty seconds and the queue defines the same value. Receipt temporarily hides a message, while deletion removes it after inbox commit. There is no ChangeMessageVisibility extension in this code. Slow sequential handling of a batch can let later messages reappear before they are handled, so sizing and retry behavior need load tests.

**Code or follow-up:** [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [official SQS visibility documentation](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-visibility-timeout.html)

### Q093 What happens to poison messages?

**Evidence status: Deployment design.**

An invalid SQS payload remains undeleted and becomes eligible for another delivery. The queue redrive policy sets maxReceiveCount to five and names the DLQ. The application logs the failure; it has no automatic repair/redrive UI. Inspect and fix the root cause before controlled redrive, keeping the original event identity unchanged when replaying the same event.

**Code or follow-up:** [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q094 Does a sequence gap go to the SQS DLQ?

**Evidence status: Implemented boundary.**

Not automatically. A structurally valid future-sequence event is accepted into the inbox and its SQS message is deleted. It then waits inside the database until the missing sequence arrives. Queue alarms therefore cannot alone detect all processing stalls; pending-inbox age and missing-sequence metrics are needed separately.

**Code or follow-up:** [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q095 How are AWS credentials supplied?

**Evidence status: Implemented and deployment design.**

AmazonSQSClient uses the SDK credential chain; the ECS task role is the intended deployed identity. The task role grants queue receive/delete/attributes and KMS decrypt on the relevant resources. No access key is embedded in the worker. Local AWS profiles and account permissions would need to be configured separately for an integration test.

**Code or follow-up:** [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q096 How do the IoT role task role and execution role differ?

**Evidence status: Undeployed infrastructure.**

The IoT role lets the IoT rule send to the queue and use its encryption key. The task role supplies permissions to application code consuming SQS. The execution role supports container startup operations such as image/log integration and the configured Secrets Manager lookup. A customer-managed secret encryption key can require additional scoped decrypt permission.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf) · [official ECS execution-role documentation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_execution_IAM_role.html)

### Q097 Which alarms exist?

**Evidence status: Undeployed infrastructure.**

A DLQ visible-message maximum greater than zero over one sixty-second period, and telemetry oldest-message age greater than 120 seconds for three sixty-second periods. Actions are the supplied SNS topic ARNs; the default list is empty. Missing data is treated as not breaching. No paging evidence or application-level lag alarm has been demonstrated.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q098 What are the queue retention values?

**Evidence status: Undeployed infrastructure.**

Both queue resources set 1,209,600 seconds, which is fourteen days. That is configured retention, not an indefinite archive or a guarantee of fourteen additional days after dead-lettering. Expiry and replay recovery need an explicit operating policy, especially if the edge has already deleted acknowledged data.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q099 How would you troubleshoot IoT publication without inbox events?

**Evidence status: Proposed runbook.**

Verify certificate status and exact client/topic policy, IoT rule match and action failures, queue/KMS permissions, queue visibility, ECS task role, Aws:QueueUrl and region resolution, and finally Accept validation logs. Preserve payloads and identities during investigation. Do not infer database receipt from a successful MQTT publish alone.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)


## Infrastructure networking Docker and Terraform

### Q100 Describe the proposed network path.

**Evidence status: Undeployed infrastructure.**

A browser reaches the HTTPS ALB in supplied public subnets. The ALB targets API tasks on port 8080 in supplied private subnets with no public IP. Task ingress is restricted to the ALB security group. Tasks need outbound access for AWS/issuer services and SQL Server connectivity. The Terraform does not create the VPC, routing, NAT, or endpoints.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q101 What is a security group versus a network ACL?

**Evidence status: Infrastructure reasoning.**

In the proposed design, security groups control allowed traffic at the ALB/task boundary and track connections. Subnet network ACLs are an additional VPC control with separate inbound/outbound rule considerations. No custom NACL is created here. In an interview, describe both as possible failure points without claiming that this repository configured every network layer.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q102 Is outbound access least privilege?

**Evidence status: Implemented infrastructure limitation.**

The task definition's security group permits HTTPS to any IPv4 destination and SQL port 1433 to broad private CIDR ranges. That is a starting policy, not a final least-privilege network design. Narrow database access to the actual destination and evaluate private endpoints or an egress proxy for required services before production.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q103 How would you diagnose an ALB 503?

**Evidence status: Proposed runbook.**

Check whether there are registered healthy targets, then ECS desired/running counts, task stop reasons, readiness responses, port mappings, and ALB-to-task security rules. Also inspect SQL readiness dependencies and proxy/redirect behavior. The current /health/ready checks the fleet table, so a schema or database problem can make the service unavailable even if the process is alive.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q104 How would you diagnose DNS or TLS failures?

**Evidence status: Proposed runbook.**

Separate name resolution, network reachability, and certificate validation. Check the hostname's DNS answer, route to port 443, certificate chain/expiry/hostname, then application status. The Terraform creates no DNS record and expects an existing ACM certificate. Do not bypass certificate validation as a production fix; identify the trust or hostname mismatch.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf) · [edge/Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Program.cs)

### Q105 Why Fargate instead of managing EC2 hosts?

**Evidence status: Design rationale.**

Fargate is the target in the task/service configuration and reduces the need to manage container host capacity and patching directly. It still leaves image maintenance, permissions, networking, database operations, and application scaling to the operator. This project does not benchmark Fargate versus EC2 or define an operational cost comparison.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q106 How is the Docker image built?

**Evidence status: Source implemented runtime unverified.**

A Node 22 stage installs the locked frontend dependencies and builds Angular. A .NET 8 SDK stage restores and publishes the API with the frontend output. The final .NET 8 runtime image runs under APP_UID and exposes 8080. This is a multi-stage definition; a local container run was not verified because Docker's engine was unavailable.

**Code or follow-up:** [Dockerfile](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Dockerfile)

### Q107 What container hardening is actually present?

**Evidence status: Configured but unverified.**

Docker selects a non-root user. Compose sets a read-only root filesystem, writable /tmp, no-new-privileges, and drops Linux capabilities. The ECS JSON sets readonlyRootFilesystem, but does not reproduce every Compose option or mount. Runtime compatibility of temporary writes and permissions remains a deployment validation step.

**Code or follow-up:** [Dockerfile](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Dockerfile) · [compose.yaml](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/compose.yaml) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q108 Does Terraform validate mean deployment will succeed?

**Evidence status: Verified configuration only.**

No. It checks configuration syntax and internal/provider schema consistency; it does not prove AWS permissions, quotas, networking, supplied resource IDs, image startup, or application health. The recorded validate succeeded after init -backend=false. A contextual plan and an isolated deployment test remain necessary before claiming deployed infrastructure.

**Code or follow-up:** [infra/](https://github.com/CJAmy1996/LaundryOaxProject/tree/545e6a3521c6b8d744e8393e9784b478251a7859/infra/) · [official Terraform validate documentation](https://developer.hashicorp.com/terraform/cli/commands/validate)

### Q109 Where is Terraform state stored and secured?

**Evidence status: Not configured.**

No remote backend is declared in this repository. A real team deployment should configure encrypted remote state, access control, locking appropriate to the backend, and version recovery. Sensitive values can appear in state or saved plans; labeling variables sensitive alone is not encryption. State, tfvars, and .terraform files are ignored locally, but backend operations are not automated here.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [.gitignore](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/.gitignore)

### Q110 Is the infrastructure highly available?

**Evidence status: Deployment design limitation.**

The deployment object accepts multiple subnet IDs and a desired task count, but no particular count or AZ distribution is validated here. Database HA is external, and there is no tested regional failover. A design can target multi-AZ operation without proving a service-level availability result. Never translate a list of AWS services into a claimed uptime percentage.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q111 Does the service autoscale?

**Evidence status: Not implemented.**

No Application Auto Scaling target or policy exists. Desired count is supplied as a deployment input. Horizontal expansion is a design option, and current worker/database contention must be characterized first. A useful future policy could combine queue age, application lag, and utilization rather than CPU alone.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q112 How would you roll back a deployment?

**Evidence status: Configured but unverified.**

The ECS service defines a deployment circuit breaker with rollback. Operationally I would retain the prior image digest/task definition, validate backward-compatible schemas, and verify queue/inbox recovery after reverting. There is no complete release workflow or tested rollback record. Database migrations require a distinct rollback or forward-fix strategy.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf) · [.github/workflows/iot-ci.yml](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/.github/workflows/iot-ci.yml)

### Q113 What is missing from infrastructure provisioning?

**Evidence status: Evidence boundary.**

Database creation/backups, VPC routes and endpoints, DNS, certificate issuance, OIDC, SNS subscriptions, remote Terraform state, autoscaling, WAF, access-log storage, secret lifecycle, and a deployed integration test are not all provided. The existing files define a useful application footprint, not a complete enterprise landing zone.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)


## Operations observability incidents and recovery objectives

### Q114 What is the difference between liveness and readiness?

**Evidence status: Implemented.**

/health/live returns a process-level healthy response. /health/ready creates a FleetDb and queries the Machines table, returning 503 on failure. Readiness therefore tests a limited storage dependency, not worker freshness, SQS reachability, issuer health, or every business endpoint. A green health response cannot alone prove telemetry is progressing.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs)

### Q115 Which operational metrics would you add?

**Evidence status: Proposed.**

Edge outbox count/oldest age/free disk; HTTP accept latency/errors and duplicate/conflict rates; inbox pending age and missing-sequence counts; worker apply rate/last progress; database lock waits; and notification delivery lag. The current implementation provides counters in snapshots, structured logs, and Terraform queue alarms, not a complete metrics/tracing system.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q116 Does the system have distributed tracing?

**Evidence status: Not implemented.**

No OpenTelemetry instrumentation, trace propagation, or trace backend is configured. Logs include tenant/device/sequence for persisted telemetry, and SQS failures include a message ID. Those fields help investigation but do not create an end-to-end trace. Propagate a non-secret correlation identifier across capture, accept, processing, and notification if tracing is added.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q117 How would you define an SLI and SLO?

**Evidence status: Proposed.**

For this system, useful SLIs include eligible-event commit-to-projection latency, valid-ingestion success rate, and edge capture success. An SLO would set a target and window for a defined population. No production SLO is measured here. Exclude or separately classify offline-device time and sequence-gap waits so a cloud processing metric does not hide a different failure domain.

**Code or follow-up:** [operations-runbook.md](operations-runbook.md)

### Q118 What are your RPO and RTO?

**Evidence status: Not measured.**

There are no measured recovery objectives. RPO concerns how much committed data a disaster can lose, and RTO concerns time to restore a usable service. These depend on database backup/PITR, event archives, credential recovery, and reconciliation. The edge outbox improves transient outage tolerance but cannot by itself establish a zero RPO after a cloud restore.

**Code or follow-up:** [operations-runbook.md](operations-runbook.md)

### Q119 How do you back up SQLite safely?

**Evidence status: Proposed procedure.**

Use the SQLite online backup API or a controlled stopped-writer backup, and verify restoration in isolation. Copying only an active database file can miss WAL content. No scheduled backup or restore tool is included. Protect backup permissions/encryption and reconcile the restored cloud last sequence against retained edge data before claiming recovery.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [README.md](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/README.md) · [official SQLite backup documentation](https://www.sqlite.org/backup.html)

### Q120 How do you know a backup works?

**Evidence status: Proposed validation.**

A successful backup command is insufficient. Restore to an isolated target, check integrity and schema, verify known event identities and sequence continuity, compare reconciliation checkpoints, and measure elapsed recovery time. Record the results and repeat after schema/runtime changes. This repository documents that need but has no completed restore drill to cite.

**Code or follow-up:** [operations-runbook.md](operations-runbook.md)

### Q121 What would you do if pending events grow while SQS is empty?

**Evidence status: Proposed runbook.**

Inspect the database pending events by device and compare their minimum sequence with LastSequence + 1. An accepted event has already left SQS, so the issue can be a sequence gap, stopped projector, repeated transaction failure, or resource contention. Do not redrive an empty queue or skip sequence numbers without establishing the cause.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/SqsWorker.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/SqsWorker.cs)

### Q122 What if the dashboard says a machine is offline after successful replay?

**Evidence status: Implemented limitation.**

The dashboard's heuristic compares the observation time to the browser clock and requires a last sequence. Replaying old observations updates state but can still look stale. It is not a real network heartbeat. Distinguish device observation age from last successful server receipt and from browser-to-API connectivity.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q123 How would you handle a sudden replay storm?

**Evidence status: Proposed runbook.**

Protect edge storage and avoid throwing away identity history. Measure receive/accept/apply rates separately, rate-limit fairly, scale validated workers within database capacity, and monitor oldest age. The edge has backoff, but there is no fleet-wide replay admission controller; the HTTP tenant limiter can also cause one tenant's devices to compete with its dashboard.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)

### Q124 What would an incident update say?

**Evidence status: Hypothetical practice.**

State observed impact and scope, the time of the last known good state, what is confirmed versus suspected, mitigation underway, and the next update time. For example: telemetry is persisted but projections for one device await a missing sequence; avoid claiming data is safe until the durable records are confirmed. No real production incident is asserted by this example.

**Code or follow-up:** [operations-runbook.md](operations-runbook.md)

### Q125 What should a postmortem contain?

**Evidence status: Proposed practice.**

A factual timeline, user impact, failure and detection mechanisms, contributing conditions, recovery steps, and owned preventive actions with verification criteria. Separate application defects from missing operational controls. Use the project’s actual local build failure or dependency finding as a documented development example; do not invent a customer outage to make the story sound larger.

**Code or follow-up:** [study-plan.md](study-plan.md)

### Q126 How would you investigate high CPU or memory in the API?

**Evidence status: Proposed diagnosis.**

Compare request volume, event backlog, projection rate, database time, GC/allocation indicators, and process/container limits. Look for large unpaginated machine snapshots, repeated serialization and counts, or aggressive polling before guessing. The Terraform task specifies 0.5 vCPU and 1 GiB; there is no profiler evidence establishing a current production bottleneck.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q127 What if KMS or Secrets Manager access fails?

**Evidence status: Proposed runbook.**

Determine whether failure occurs before container startup or during message processing. Execution-role secret retrieval and task-role queue decryption are different paths. Check the exact ARN, resource policy/key policy, region, and principal before widening permissions. A missing scoped permission should not be addressed by permanently granting administrator access.

**Code or follow-up:** [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q128 How would you keep operational changes safe?

**Evidence status: Proposed procedure.**

Prefer read-only diagnosis first, record current state, make the smallest reversible mitigation, and verify the original symptom and durable invariants afterward. For a destructive recovery such as replay-marker reset, queue purge, or database restore, require a specific approved plan and preserved evidence. There is no automatic repair tool that makes these actions safe by default.

**Code or follow-up:** [operations-runbook.md](operations-runbook.md)


## Angular user experience and notification semantics

### Q129 How is the Angular application structured?

**Evidence status: Implemented.**

It is a standalone root component with signals for server data, UI state, and dialogs, plus CommonModule and FormsModule. It uses zoneless change detection and browser fetch rather than Angular HttpClient. This keeps the demo compact, but extracting API/auth services and feature components would improve maintainability as the product grows.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q130 How does the dashboard update live?

**Evidence status: Implemented.**

It fetches a snapshot at startup and every three seconds, skipping overlapping refreshes. The server also exposes an SSE count feed every two seconds with a two-minute connection lifetime, but the dashboard does not consume that feed. There is no SignalR or WebSocket implementation to claim.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q131 What is the SSE payload?

**Evidence status: Implemented.**

It contains a processed-event count for the authenticated tenant, intended as an invalidation signal. It is not the full telemetry payload, a durable ordered notification stream, or a Last-Event-ID replay mechanism. A consumer would refetch state and reconnect with authentication when the connection ends.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q132 What if the API request fails?

**Evidence status: Implemented limitation.**

The UI preserves the previous snapshot, sets its connection flag false, and displays an error. A 401 triggers the sign-in-required view. The refresh path does not automatically clear a previous error after a successful retry, and browser timeouts/cancellation are not fully managed. These are known UX/reliability improvements, not evidence of perfect recovery behavior.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q133 Are notifications actually sent to customers?

**Evidence status: Implemented.**

The worker creates persistent tenant-scoped in-app records when phase changes into complete or fault. The browser reads those records on refresh. No SMS, email, mobile push, customer recipient mapping, or external provider delivery acknowledgment exists. Describe this as live in-app cycle notifications for the operations tenant.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q134 How do you avoid repeated completion notifications?

**Evidence status: Implemented.**

Process compares the previous phase with the incoming phase, and notification identity is based on the event. The event processed marker and notification share a transaction. Exact duplicate processing does not create a second notification. However, there is no CycleId, so repeated or erroneous phase transitions are not deduplicated at a richer business-cycle level.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs)

### Q135 Is the cycle state machine complete?

**Evidence status: Implemented limitation.**

No. Allowed phase names and appliance-kind compatibility are checked, but legal transition sequences and monotonic progress are not enforced. A washer could move from complete back to washing with the next sequence, which may represent a new cycle or bad input. A CycleId and explicit transition model would make that distinction enforceable.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q136 How are rinse and drying estimates calculated?

**Evidence status: Implemented.**

Drying minutes are ceil(max(0, moisturePercent - 5) / 2). Wash/rinse/spin remaining minutes are ceil((100 - progress) × 0.45). Extra rinses during wash/rinse are clamp(ceil((turbidityNtu - 20) / 50), 0, 3). These are transparent baseline heuristics, not trained models or validated appliance/fabric predictions.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs)

### Q137 What accessibility support exists?

**Evidence status: Implemented but unverified in browser.**

The templates include labeled controls, dialog roles, focus-visible styles, a Tab trap, Escape dismissal, and reduced-motion CSS. That is source-level support, not a completed accessibility audit. Initial dialog focus, focus restoration, screen-reader behavior, contrast, and responsive interactions still need browser and assistive-technology testing.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts) · [client/src/app.html](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/app.html) · [client/src/styles.css](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/styles.css)

### Q138 What does CSV export contain?

**Evidence status: Implemented limitation.**

It exports the latest snapshot's event rows, capped by the API at thirty, with event ID, device, sequence, and timestamps. Values are quoted and double quotes are escaped; certain leading formula characters are prefixed. This is not a complete telemetry archive or a full tested spreadsheet-injection defense across all whitespace/control-character cases.

**Code or follow-up:** [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q139 Why is there no frontend performance claim?

**Evidence status: Test boundary.**

The production bundle built successfully, but build size alone does not prove runtime responsiveness, accessibility, or low-end device performance. No browser automation, frontend unit suite, Lighthouse measurement, or large-fleet rendering benchmark is recorded. Client-side filtering and all-machine snapshots should be revisited if tenant fleets become large.

**Code or follow-up:** [client/package.json](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/package.json) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)


## Testing CI CD and quality

### Q140 What do the ten automated tests cover?

**Evidence status: Verified local tests.**

They cover matching duplicates and conflicts, out-of-order processing, a new worker instance over retained data, parallel same-instance duplicates, device impersonation, tenant/viewer isolation, invalid readings, cross-tenant notification access, edge reopen/backoff/sequence continuation, and invalid-capture rollback. Name these accurately rather than claiming full end-to-end cloud or hardware coverage.

**Code or follow-up:** [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q141 How are integration tests isolated?

**Evidence status: Implemented.**

WebApplicationFactory configures a unique temporary SQLite file and test-only authentication. Hosted ingestion and SQS workers are removed so tests explicitly invoke processing and avoid timing races. Each test creates its own fixture. This tests API/database behavior deterministically while intentionally bypassing real issuer and background scheduling behavior.

**Code or follow-up:** [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q142 What does the parallel test not prove?

**Evidence status: Test boundary.**

Twenty Accept calls share one singleton EventPipeline and therefore one semaphore. The test shows idempotency for those concurrent submissions, but does not run independent processes, SQL Server transactions, or competing SQS consumers. It is not evidence of a distributed throughput limit or multi-replica race freedom.

**Code or follow-up:** [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q143 Which tests would you add first?

**Evidence status: Proposed.**

Real JWT issuer/audience/role failures, two independent SQL Server workers, commit/ACK-loss injection, notification transaction rollback, missing-event recovery, disk-full capture, long-running SQS visibility, and process-kill restart. Add browser tests for authentication, filters, dialogs, and error recovery. Prioritize tests of boundaries where a bug can lose data or leak a tenant.

**Code or follow-up:** [tests/ReliabilityTests.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/tests/ReliabilityTests.cs)

### Q144 What does the GitHub Actions workflow do?

**Evidence status: Configured workflow.**

On push and pull request it sets up .NET 8 and Node 22, runs backend tests and edge build, installs/builds Angular, checks dependencies, validates Terraform, builds Docker, and invokes Trivy. It is CI, not CD: no ECR push, ECS deployment, Terraform apply, or production approval gate is defined. A workflow file is not itself a passing run.

**Code or follow-up:** [.github/workflows/iot-ci.yml](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/.github/workflows/iot-ci.yml)

### Q145 Do all vulnerability scans fail the build on findings?

**Evidence status: Implemented limitation.**

npm audit uses a high threshold and Trivy is configured to fail on high/critical findings while ignoring unfixed items. The dotnet list package --vulnerable command reports findings, but the workflow does not explicitly parse them into a failure gate. Local scans found no reported vulnerabilities after a native SQLite dependency update; that result is time-bound.

**Code or follow-up:** [.github/workflows/iot-ci.yml](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/.github/workflows/iot-ci.yml) · [LaundryOaxWebAPI.csproj](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/LaundryOaxWebAPI.csproj)

### Q146 Is the dependency build reproducible?

**Evidence status: Partial implementation.**

npm ci uses the committed lockfile, Terraform has a provider lockfile, and .NET direct package versions are explicit. Docker base tags and GitHub Action tags are not pinned to immutable digests/SHAs, and no NuGet lockfile is present. Reproducibility can be improved with digest pinning, locked restore, artifact provenance, and planned patch updates.

**Code or follow-up:** [client/package-lock.json](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/package-lock.json) · [infra/.terraform.lock.hcl](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/.terraform.lock.hcl) · [Dockerfile](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Dockerfile)

### Q147 How did you address the original build errors?

**Evidence status: Verified build change.**

The old order service referenced absent user/payment members and an undefined total. It was replaced with validated EF order persistence, its controller dependency was changed to IOrderService, and missing constructor/null initialization issues were repaired. This makes the project compile; it does not establish an implemented user validation or payment workflow.

**Code or follow-up:** [Services/OrderServices/OrderService.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Services/OrderServices/OrderService.cs) · [Controllers/LaundryOaxController.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Controllers/LaundryOaxController.cs) · [Controllers/UsersController.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Controllers/UsersController.cs)

### Q148 How should test results be reported in an interview?

**Evidence status: Verified local scope.**

State the command, environment, passed count, and boundaries. Ten tests passed locally on .NET 8 using SQLite and test identities. Builds passed, Terraform validated, and an eight-event smoke test completed. Do not turn these into a coverage percentage, stress benchmark, full security audit, or deployed system reliability figure.

**Code or follow-up:** [validation-record.md](validation-record.md)

### Q149 How would you deploy a schema and code change safely?

**Evidence status: Proposed.**

Use reviewed FleetDb migrations, test mixed-version compatibility, apply expand/contract changes where possible, and keep old/new event readers compatible during rollout. Separate schema administration from runtime permissions. This is a proposed procedure because FleetDb currently has only initial EnsureCreated bootstrapping and no production migration history.

**Code or follow-up:** [Program.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Program.cs) · [IoT/FleetDb.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetDb.cs)


## System design calculations and technical challenges

### Q150 How would you size an offline buffer?

**Evidence status: Hypothetical calculation.**

Define event rate r, outage duration T, serialized size S, and overhead factor h; estimate r × T × S × h, then add WAL, indexes, filesystem headroom, and safety margin. As a hypothetical example, one 1 KiB event every ten seconds for twenty-four hours is about 8.44 MiB raw per device. This is a sizing exercise, not a measured payload size or supported outage duration.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [architecture.md](architecture.md)

### Q151 How do you calculate cloud ingestion rate?

**Evidence status: Hypothetical calculation.**

With N devices sending every t seconds, nominal rate is N/t events per second. Ten thousand devices at ten-second intervals would be 1,000 events per second, but this repository has no test supporting that load. Model reconnect bursts and duplicate retries separately, and validate database write amplification before selecting capacity.

**Code or follow-up:** [architecture.md](architecture.md)

### Q152 How long would a backlog take to drain?

**Evidence status: Hypothetical calculation.**

If backlog is B, sustainable processing rate is mu, and new arrival rate is lambda, the approximate drain time is B/(mu-lambda), provided mu exceeds lambda. For 60,000 events, mu=500/s and lambda=100/s gives 150 seconds under ideal assumptions. Missing sequences or database bottlenecks invalidate that simple calculation. These rates are illustrative only.

**Code or follow-up:** [architecture.md](architecture.md)

### Q153 What is the current write amplification?

**Evidence status: Code-derived reasoning.**

For an ordinary event, the API adds an inbox and accepted-audit row; processing updates the inbox and machine and adds a processed-audit row, with a notification insert on certain phase changes. The edge also writes its outbox/counter and later deletes the outbox row. Physical I/O additionally includes indexes and transaction logs, so one sensor event is not one disk write.

**Code or follow-up:** [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q154 What is the first likely scaling concern?

**Evidence status: Inference.**

The process-wide semaphore spans a bounded batch of up to one hundred processing transactions as well as acceptance, so it is a plausible throughput/latency limiter. Snapshot counts and an unbounded machine list are other candidates. These are hypotheses from code, not measured bottlenecks; profiling and load tests should decide optimization order.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs) · [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs)

### Q155 How would you reduce database polling load?

**Evidence status: Proposed.**

Introduce bounded/paginated read models, measure indexes and query plans, cache carefully with tenant-aware keys, and consider actual change-driven notifications. The current SSE endpoint still polls the database, so switching the browser to it alone would not eliminate database work. Preserve catch-up behavior and avoid using an in-memory event bus as the only source of truth.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q156 Can a duplicate with a changed property order be rejected?

**Evidence status: Code-derived reasoning.**

The server hashes JsonSerializer output of the typed Telemetry object, not the original JSON text, so wire property order is not the intended distinguishing feature. Changes to typed values can change the hash. Contract/serializer evolution and timestamp representations need compatibility tests before treating the hash format as a long-lived canonical standard.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q157 Can a large sequence number break the frontend?

**Evidence status: Implemented limitation.**

C# uses long, while the TypeScript interface uses number. JavaScript integers beyond its safe integer range can lose precision when represented as numbers. The prototype does not restrict sequence to that range or serialize it as a decimal string. It is a contract issue to address even if realistic appliance counters take a long time to approach it.

**Code or follow-up:** [IoT/Models.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Models.cs) · [client/src/main.ts](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/client/src/main.ts)

### Q158 Does registration also provision credentials?

**Evidence status: Implemented.**

No. Registration creates a Machine and audit entry scoped to the user tenant. HTTP device key mappings are separate configuration, and AWS certificate policy attachments are separate Terraform inputs. A complete onboarding workflow would coordinate both and handle rollback/revocation; merely seeing a machine in the dashboard does not authorize it to submit events.

**Code or follow-up:** [IoT/FleetEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/FleetEndpoints.cs) · [IoT/Security.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/Security.cs) · [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf)

### Q159 Why should the demo and an edge agent not share a device ID?

**Evidence status: Implemented limitation.**

They allocate sequences independently: the demo looks at latest inbox sequence, while the edge uses its own local counter. Using both writers for one device can produce sequence conflicts. Register a dedicated edge device and keep a single authoritative capture stream. A production design should enforce that ownership rather than rely only on operator discipline.

**Code or follow-up:** [IoT/DemoEndpoints.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/DemoEndpoints.cs) · [edge/Outbox.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/edge/Outbox.cs)

### Q160 How would you support multiple regions?

**Evidence status: Proposed.**

Define authoritative device partition ownership, replication semantics, conflict resolution, identity/key distribution, and how a failover endpoint reconciles acknowledged events. Neither Terraform nor application code implements multi-region replication or failover. I would choose an explicit recovery model and test it before claiming active-active availability.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q161 How would you estimate costs without inventing a number?

**Evidence status: Proposed.**

Enumerate task hours, database/licensing and backups, ALB usage, NAT or endpoints, IoT/SQS requests, KMS operations, logs, transfer, and storage retention. Apply measured workload assumptions to current region-specific pricing. There is no actual monthly bill or cost optimization result to cite, and the database topology can dominate the estimate.

**Code or follow-up:** [infra/main.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/main.tf) · [infra/ecs.tf](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/infra/ecs.tf)

### Q162 What design would replace the global semaphore?

**Evidence status: Proposed.**

Possibilities include per-device partitions, durable claims/leases with expiration, or database-specific queue selection patterns. Each must retain uniqueness and atomic projection/notification updates under crashes and multiple workers. A per-key in-memory lock alone would still not coordinate replicas. I would introduce the smallest measured change and prove it with independent-process tests.

**Code or follow-up:** [IoT/EventPipeline.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/IoT/EventPipeline.cs)

### Q163 What if an interviewer finds a flaw in this implementation?

**Evidence status: Interview technique.**

Acknowledge the observed behavior, locate the invariant that is or is not enforced, and distinguish a bug from an intentional tradeoff. Propose a bounded fix and a regression test, without pretending it already exists. Concrete examples are tenant-wide read state, missing cycle IDs, sequence gaps, unverified SQL concurrency, and incomplete dependency failure gating.

**Code or follow-up:** [claim-audit.md](claim-audit.md)


## Behavioral questions and practical exercises

### Q164 Tell me about a difficult bug you solved.

**Evidence status: Personal answer to adapt.**

Use the documented build failure only if you can personally explain and reproduce the fix: absent order/payment members made the old API fail compilation, and the repair reduced it to validated persistence. Explain why inventing successful payment behavior would be wrong. If discussing AI assistance, identify what you reviewed and learned. Do not describe an invented production outage.

**Code or follow-up:** [Services/OrderServices/OrderService.cs](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/Services/OrderServices/OrderService.cs) · [study-plan.md](study-plan.md)

### Q165 Tell me about a security improvement.

**Evidence status: Personal answer to adapt.**

A documented development example is the dependency scan that exposed a transitive SQLite native-library advisory, followed by a bundle update and rerun of tests/scans. Explain the distinction between package hygiene and system security. Say personally performed only if you actually reproduce that work; otherwise describe the repository's validation record and your subsequent review.

**Code or follow-up:** [LaundryOaxWebAPI.csproj](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/LaundryOaxWebAPI.csproj) · [validation-record.md](validation-record.md)

### Q166 Describe a disagreement over architecture.

**Evidence status: Hypothetical only.**

There is no recorded team disagreement to recount as fact. You can instead discuss a hypothetical tradeoff: skipping missing telemetry improves apparent freshness but weakens ordered history. Present stakeholder goals, the alternatives, and a decision criterion. If the interviewer specifically wants a real interpersonal example, use a genuine experience from elsewhere.

**Code or follow-up:** [study-plan.md](study-plan.md)

### Q167 How do you prioritize reliability work?

**Evidence status: Proposed prioritization.**

Rank by data loss, unauthorized access, inability to recover, and operational detectability. For this code, validate real identity and SQL concurrency, then migrations and restore/replay reconciliation, before advanced analytics. Explain the consequence each change prevents and the test that would demonstrate improvement. Avoid equating visible dashboard polish with production readiness.

**Code or follow-up:** [claim-audit.md](claim-audit.md)

### Q168 How would you demonstrate this project in five minutes?

**Evidence status: Rehearsal.**

Open the local dashboard, send a demo batch, inspect a machine and event ledger, and explain acknowledgment versus processing. Run the reliability tests and show the out-of-order and duplicate cases. End with the source-only AWS design and its remaining validation steps. Prepare the app before the interview and never depend on live cloud provisioning.

**Code or follow-up:** [study-plan.md](study-plan.md) · [README.md](https://github.com/CJAmy1996/LaundryOaxProject/blob/545e6a3521c6b8d744e8393e9784b478251a7859/README.md)

### Q169 What coding exercise might follow this project?

**Evidence status: Practice exercise.**

Expect a request to write an idempotent consumer, detect missing sequences, implement exponential backoff, filter data by authenticated tenant, or test a duplicate/conflict case. Explain your failure assumptions before coding. A good exercise includes the commit/ACK crash window and asserts database effects, not only a returned success flag.

**Code or follow-up:** [study-plan.md](study-plan.md)

### Q170 What SQL exercise might follow?

**Evidence status: Practice exercise.**

Query pending events by device, compare the earliest available sequence with LastSequence + 1, or find devices whose observation timestamp is stale. Explain which query answers receipt versus processing versus freshness. Read-only queries are in the runbook; do not use a bulk update to processed markers as an interview shortcut to fix a backlog.

**Code or follow-up:** [operations-runbook.md](operations-runbook.md)

### Q171 What Linux operations questions should I prepare for?

**Evidence status: Practice exercise.**

Be ready to trace listening ports, process/container status, logs, disk pressure, DNS, routes, and TLS, then relate each observation to the system's boundaries. The local development environment was Windows; do not claim hands-on Linux production incident experience from the Dockerfile alone. Rehearse commands in a disposable Linux environment before presenting them as practiced skills.

**Code or follow-up:** [operations-runbook.md](operations-runbook.md)

### Q172 What if I do not know an answer?

**Evidence status: Interview technique.**

State what you know from this implementation, identify the missing assumption, and explain the authoritative source or experiment you would use to resolve it. For example, say SQL Server contention is unmeasured and propose an independent-worker test. A precise boundary with a validation plan is stronger than inventing a guarantee.

**Code or follow-up:** [study-plan.md](study-plan.md)

### Q173 What should I ask the interviewer?

**Evidence status: Interview technique.**

Ask how their team defines ingestion correctness, handles offline devices and missing sequences, separates incident ownership, measures recovery objectives, reviews infrastructure changes, and tests restore/rollback. Tailor questions to the role rather than assuming their stack matches this project. These questions show interest in operating the whole system, not only implementing endpoints.

**Code or follow-up:** [study-plan.md](study-plan.md)

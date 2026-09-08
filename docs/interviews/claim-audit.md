# LaundryOax project claim audit

This audit converts the proposed resume description into claims supported by the actual implementation at [545e6a3](https://github.com/CJAmy1996/LaundryOaxProject/commit/545e6a3521c6b8d744e8393e9784b478251a7859). A strong interview answer can explain limitations precisely; it does not need to pretend that a prototype has production operating history.

## Claim by claim

| Original phrase or likely interpretation | What the repository supports | Accurate wording |
| --- | --- | --- |
| Architected a distributed IoT platform | Edge and server boundaries; API and workers share one server host | Built a local IoT prototype with a distributed event-processing design |
| Ingests washer and dryer telemetry | Typed ingestion and eight simulated machine records | Implemented validated washer/dryer telemetry ingestion; demonstrated with simulated devices |
| Tracks cycle state | Next-sequence projection with allowed phases; no CycleId or full transition graph | Tracks ordered phase and progress readings |
| Estimates rinse and drying requirements | Explicit arithmetic heuristics, no calibration dataset | Added baseline rinse-count and remaining-time heuristics |
| Live user notifications | Polling UI and persistent tenant-wide notifications, not personal recipients | Added persistent in-app tenant notifications for complete/fault transitions |
| Shared multi-tenant infrastructure | Claims-based filtering and composite identity keys; no production tenants | Implemented tenant-scoped fleet access and isolation tests |
| Offline-first processing | SQLite edge capture and HTTP replay | Implemented durable edge capture and acknowledgment-based HTTP replay |
| Prevents all data loss or corruption | Transactions and retained retry data protect defined failures, not disaster recovery | Designed to retain captured events across connectivity failures and avoid duplicate effects |
| Unique IDs, sequences, ACKs and exponential backoff | Present in Outbox, replay, and EventPipeline | Implemented per-device sequencing, immutable identities, commit-before-ACK, and persisted backoff |
| MQTT ingestion | IoT rule and SQS worker exist; reference edge uses HTTP | Added an AWS IoT MQTT-to-SQS ingestion path in code and Terraform; cloud integration remains unverified |
| Horizontal scaling | Shared SQL Server provider option and ECS desired count; local gate remains | Designed a shared-database deployment path; multi-replica correctness and capacity remain to be tested |
| Stateless APIs | Authoritative fleet state can live in shared SQL; local limiter/gate are process state | Externalized durable state for a proposed replicated deployment; local demo uses one SQLite store |
| Authenticated device identities | Per-device HTTP key hashes; cloud certificate policies as configuration | Implemented device-scoped HTTP authentication and defined AWS certificate/topic policies |
| OAuth OIDC RBAC | JWT/PKCE wiring and role policies; no real issuer validation | Implemented OIDC integration points and role/tenant authorization; tested policies using local test identities |
| Least-privilege IAM | Scoped queue/key roles; broad egress and managed execution policy remain | Defined scoped application/IoT permissions with deployment-specific hardening still required |
| Encryption | Remote edge HTTPS requirement, configured ALB/KMS/ECR; SQLite unencrypted | Configured transport and cloud encryption boundaries; local disk and SQL encryption require operating controls |
| Secrets management | Secret-backed config points and ECS secret ARN injection | Added secret injection support; no automated provisioning or rotation lifecycle |
| Audit logging | Ordinary database rows for selected writes and structured logs | Recorded selected ingestion/processing/registration actions; not immutable comprehensive auditing |
| Schema validation and database constraints | Typed validation plus selected checks, unique keys, FK, concurrency token | Implemented event validation and database constraints for key invariants |
| Vulnerability scanning | Local scans and CI scan steps; .NET findings not an explicit failure gate | Added dependency/container scan configuration and resolved a locally identified SQLite dependency issue |
| Observability | Health endpoints, logs, snapshot counts, queue alarms in Terraform | Added basic health/logging and defined queue alarms; no deployed tracing or complete SLI coverage |
| Backups | Written instructions only | Documented backup and restore requirements; automation and drills remain future work |
| Failure testing | Ten tests with specific local scenarios | Added ten local reliability/authorization tests; no power-cut, real-cloud, load, or hardware test evidence |

## Resume version for software engineering applications

**LaundryOax IoT Platform — C#, ASP.NET Core, Angular, EF Core, SQLite/SQL Server, AWS, Terraform, Docker**

- Built an IoT fleet prototype with validated telemetry ingestion, ordered machine-state projections, an Angular operations dashboard, and persistent in-app cycle notifications.
- Implemented a SQLite edge outbox with atomic sequence allocation, acknowledgment-based replay, duplicate/conflict detection, and persisted exponential backoff.
- Added tenant/device authorization and ten local tests covering duplicate delivery, out-of-order events, restart persistence, invalid readings, and cross-tenant access.
- Defined an AWS IoT-to-SQS and ECS deployment path using Terraform, with shared SQL Server configuration, scoped IAM, encryption, and queue alarms; cloud/runtime integration remains deployment work.

Use “built” only in a way that accurately describes your involvement. If asked, explain AI assistance and name your own review, modifications, and validation. Do not add throughput, uptime, customer, savings, or production incident metrics without evidence.

## Resume version for infrastructure operations applications

- Developed a local IoT reliability prototype that separates durable edge capture, cloud receipt, asynchronous processing, and operator visibility.
- Defined AWS infrastructure for encrypted SQS/DLQ ingestion, IoT device-topic permissions, ECS Fargate, HTTPS load balancing, log collection, and queue-age/dead-letter alarms.
- Added process/storage health checks and documented diagnosis of connectivity failures, sequence gaps, queue backlogs, IAM errors, and restore reconciliation.
- Verified local builds, ten reliability/authorization tests, and Terraform configuration validity; identified remaining work for deployed monitoring, backup automation, and cloud recovery exercises.

## Specific implementation gaps to discuss proactively

| Gap | Why it matters | Next validation or improvement |
| --- | --- | --- |
| One process-wide semaphore | Accept waits behind processing batches; no distributed lock | Measure contention and test independent workers before partitioning |
| No FleetDb migrations | EnsureCreated does not evolve a deployed schema | Add reviewed, provider-specific deployment migrations |
| No CycleId or transition graph | Phase changes cannot reliably identify distinct physical cycles | Define cycle identity, legal transitions, reset behavior, and tests |
| No gap repair | One missing sequence can stall later events indefinitely | Add missing-sequence age metrics and an audited recovery protocol |
| Tenant-wide notification Read | Read state is shared across users | Model recipient delivery/read rows if personal notifications are required |
| Observation time used as freshness | Old replay looks offline despite recent receipt | Separate observed time, receipt time, and device heartbeat |
| No real OIDC/SQL/cloud integration test | Local behavior does not prove production boundaries | Build isolated integration environment and negative test suite |
| Queue receiver does not extend visibility | A slow sequential batch can be redelivered | Test long handling and add bounded concurrency/visibility extension if needed |
| No versioned event schema | Old stored/replayed events can conflict with a changed reader/hash contract | Define compatibility and upcasting before schema changes |
| Broad/private egress and HTTP internal hop | Network policy is not final least privilege or end-to-end TLS | Review actual destinations, internal encryption needs, and ingress trust |
| No backup scheduler/archive | Cloud restore can lose already acknowledged events | Add verified PITR/archive and replay reconciliation |
| Test-only auth and object-reopen restart tests | Real issuer and crash boundaries are not exercised | Add process-kill, lost-ACK, storage-failure, and external-auth tests |
| .NET scan reports without explicit fail gate | A CI run may print a vulnerability and still pass that step | Parse scan results or enforce appropriate restore audit failure thresholds |
| One Angular component and no frontend test suite | Maintainability and browser correctness remain lightly validated | Extract services/components and add user-flow tests |

These are engineering findings, not claims of an active production exploit. The project has no established production deployment or customer operating history.

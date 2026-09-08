# LaundryOax interview study plan

The goal is to understand and demonstrate the implementation, not memorize impressive claims. First-person examples below are speaking drafts to adapt to your own verified involvement, including AI assistance.

## Software engineering opening

“LaundryOax is my IoT fleet prototype for reliable telemetry under intermittent connectivity. The edge commits a numbered event to SQLite, and the server only acknowledges it after durable inbox persistence. A background worker applies contiguous sequences and commits machine state and notifications together. The console reads the resulting state. The local tests exercise duplicates, gaps, persistence, and tenant/device authorization. The AWS path is defined but not deployed; my next validation work is real OIDC, SQL Server concurrency, and cloud delivery.”

Expand by showing Capture, Accept, and Process. Explain the failure window between each pair. Replace “my next validation work” with actual next steps you intend to pursue.

## Infrastructure operations opening

“This project gives me a concrete way to reason about telemetry ownership and recovery. I separate edge outbox, SQS transport, fleet inbox, and browser freshness because they fail differently. The infrastructure defines scoped IoT/SQS roles, encrypted queues, a DLQ, Fargate, HTTPS load balancing, and queue alarms. I can demonstrate local builds and reliability tests and explain the operating runbook. I do not yet have an AWS production deployment, tested backups, or an uptime result to claim.”

Expand by drawing the network path and identifying which role accesses which resource. Then diagnose a hypothetical empty-SQS but nonempty-inbox scenario.

## Five minute demo

| Time | Action | Explanation |
| --- | --- | --- |
| 0:00–0:40 | Open the already-running dashboard | Eight simulated machines, tenant workspace, no production claim |
| 0:40–1:30 | Click Simulate telemetry and open details | Typed readings and baseline estimates; not calibrated prediction |
| 1:30–2:10 | Open Event stream | Receipt and processed state are distinct; latest thirty rows only |
| 2:10–3:15 | Show duplicate and out-of-order tests | Same identity is safe to replay; sequence two waits for one |
| 3:15–4:10 | Show Capture and Process transactions | Name the durable owner and what happens after a crash |
| 4:10–5:00 | Show Terraform and limitations | Validated source, not deployed AWS or measured availability |

Do a full rehearsal yourself before an interview. Use a dedicated edge device identity if demonstrating replay. Do not create real cloud resources during the interview, expose keys, or reset the primary demo database without a deliberate recovery plan.

## Seven session study plan

1. **Scope and evidence:** read claim-audit.md and validation-record.md; rewrite the two introductions in your own voice.
2. **Event lifecycle:** draw the three commits without notes; explain lost ACK, duplicate, and gap scenarios.
3. **C# and data:** walk through DI lifetimes, validation, context factory, keys, transactions, and concurrency token.
4. **Security:** distinguish user/device auth, tenant filtering, role permissions, key rotation, and proxy trust. Reproduce negative tests.
5. **Infrastructure:** trace DNS/TLS/ALB/task/DB and IoT/queue/IAM. Explain what Terraform does not create.
6. **Operations:** work through five incident matrix rows and calculate a hypothetical buffer/drain estimate.
7. **Mock interview:** answer ten randomly selected questions aloud; show evidence for two, acknowledge a limitation for two, and practice one unfamiliar-question response.

## Mock interview agendas

**Software engineering, forty-five minutes:** five-minute introduction; ten-minute Capture/Accept/Process review; ten-minute concurrency and data model discussion; ten-minute security/Angular/testing questions; five-minute coding extension; five-minute reflection and questions.

**Infrastructure operations, forty-five minutes:** five-minute introduction; ten-minute network/IAM/deployment discussion; fifteen-minute failure scenario; five-minute restore/rollback discussion; five-minute monitoring/capacity discussion; five-minute candidate questions.

Score each answer from zero to two for technical correctness, code evidence, explicit limits, and clarity. An answer that invents deployment or performance results should be rewritten regardless of its score elsewhere.

## Practical exercises

| Exercise | What a good answer must demonstrate | Current implementation reference |
| --- | --- | --- |
| Idempotent receiver | Same ID/same typed content succeeds; same ID/different content conflicts; DB key arbitrates race | EventPipeline.Accept |
| Missing sequence | Persist later event, wait, process when missing event arrives | EventPipeline.Process |
| Backoff | Persist attempts and schedule, cap delay, explain additive jitter | Outbox.Retry |
| Tenant isolation | Use authenticated tenant, never trust request tenant alone | FleetEndpoints |
| Transaction failure test | Assert state, marker, and notification roll back together | Proposed additional test |
| Independent worker test | Separate pipeline instances/processes against SQL Server | Proposed additional test |
| CycleId extension | Define transitions and old-event compatibility first | Proposed schema change |
| Linux diagnosis | Separate listener, disk, DNS, TLS, and application failure | Operations runbook |

Implement proposed exercises on a new branch or disposable copy, not by overwriting the documented baseline during an interview.

## Honest STAR prompts

**Build failure, supported development scenario:** Situation: inherited service code did not compile. Task: make the project build without pretending unfinished payments worked. Action: explain the actual service/controller repairs and your own review. Result: local build passed. Mention AI assistance if asked and do not claim customer impact.

**Dependency finding, supported development scenario:** Situation: a local package scan reported a native SQLite dependency advisory. Task: update the dependency without breaking persistence behavior. Action: inspect the dependency chain, update the bundle, rerun tests and scans; identify what you personally performed. Result: recorded local tests passed and the final scan reported no known packages with vulnerabilities at that time.

**Architecture tradeoff, reasoning exercise:** Discuss waiting for missing sequences versus skipping them. This is a technical design discussion, not evidence of a real team disagreement. For a behavioral question requiring a genuine conflict, use an actual experience from your own history.

## Handling a question you did not anticipate

1. Restate the boundary: edge capture, HTTP acceptance, queue delivery, database processing, or browser display.
2. State the invariant and the authoritative store at that boundary.
3. Name the failure assumptions and unknowns.
4. Explain the tradeoff and the smallest safe design or diagnostic step.
5. State what evidence or test would resolve the uncertainty.

Example: “I have not measured that under independent SQL Server workers. The intended protection is unique keys plus the processing transaction and concurrency token. I would run two workers against the same eligible event, inject a commit/response failure, and assert one projection transition and one notification before claiming the behavior.”

## Glossary in project terms

| Term | Meaning here |
| --- | --- |
| Telemetry | A timestamped device reading with phase, progress, and sensor values |
| Outbox | Edge rows retained until application-level receipt |
| Inbox | Server rows durably accepted before projection |
| ACK | A receipt naming the event after server persistence; not proof of display |
| Idempotency | Replaying the same retained identity does not repeat database effects |
| Sequence gap | The next required number is absent while later numbers exist |
| Projection | Current machine state derived by applying eligible events |
| Transaction | A bounded group of database changes that commits or rolls back together |
| Concurrency token | LastSequence original-value check used to detect a competing update |
| Serializable | The selected transaction isolation level for processing, with contention tradeoffs |
| WAL | SQLite write-ahead log used by the edge store |
| Backoff and jitter | Increasing retry delay plus random spread |
| Tenant | Organization boundary used in identity and query filtering |
| RBAC | Role-based policy checks for viewers/operators/admins |
| OIDC | Sign-in integration with an identity provider |
| OAuth access token | Bearer credential validated for API access |
| PKCE | Protection used with the SPA authorization-code exchange |
| IAM role | AWS permissions assumed by a specific workload/service boundary |
| Visibility timeout | Time a received SQS message is hidden before deletion or redelivery |
| DLQ | Queue receiving repeatedly failing messages under redrive policy |
| IaC | Infrastructure represented in Terraform source and managed through state |
| Drift | Real deployed resources differing from declared/recorded infrastructure |
| Liveness | Whether the application process responds |
| Readiness | Whether the limited checked fleet-storage dependency is usable |
| SLI | A precisely defined reliability measurement |
| SLO | A target for an SLI over a defined window, not established here |
| RPO | Maximum acceptable data loss measured in recovery-point terms |
| RTO | Target time to restore the required service |
| PITR | Database recovery to a chosen supported point in time |
| Blast radius | Scope of users/devices/resources affected by one failure or change |

## Questions to ask the hiring team

- How do you define and test acknowledgment and data ownership boundaries?
- Who owns device failures, application failures, and infrastructure incidents?
- Which recovery objectives are measured through restore drills?
- How do you handle noisy tenants and large reconnect bursts?
- How are infrastructure plans, permissions, migrations, and rollback reviewed?
- What evidence would you expect before approving a reliability or scaling claim?

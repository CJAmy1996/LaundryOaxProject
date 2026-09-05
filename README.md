# LaundryOax · IoT fleet platform

An ASP.NET Core 8 API, Angular 20 operations console, durable SQLite edge agent, and AWS IoT → SQS ingestion adapter. Built in the existing LaundryOaxWebAPI repository. The existing business tables and migrations are retained separately from the new fleet store.

## Run locally

Requirements: .NET 8 SDK or newer, Node 22 LTS, npm. This repository targets .NET 8; the original .NET 6 target was upgraded.

```powershell
# Terminal 1, repository root. Development profile explicitly enables local demo auth.
dotnet run --project LaundryOaxWebAPI.csproj

# Terminal 2
cd client
npm ci
npm start
```

Open http://localhost:4200. The API also exposes Swagger at https://localhost:7106/swagger. Click **Simulate telemetry** to advance the eight seeded machines. Several batches complete a cycle and create durable notifications. Search, status/location filters, machine details, registration, event export, and notification read state use the live API. Demo data is labeled and persists in `fleet.db`. A machine is considered recently reporting for two minutes after its observation time.

`npm run build` writes the Angular bundle to `wwwroot`; after building, the API serves the console directly at http://localhost:5214. Demo auth only works for loopback callers in Development. It does not accept a caller-supplied tenant. Do not expose a Development instance through a forwarding proxy.

## Tests and build

```powershell
dotnet test tests/LaundryOax.Tests.csproj -c Release
dotnet build edge/LaundryOax.Edge.csproj -c Release
cd client
npm run build
```

Tests cover duplicate delivery, conflicting IDs/sequences, out-of-order arrivals, processor restart, parallel duplicate ingestion, tenant/device impersonation, viewer permissions, invalid sensor input, cross-tenant notification access, edge restart, backoff ordering, and sequence rollback. Test databases are isolated temporary files. The CI workflow also builds the container, validates Terraform, and runs vulnerability scans.

## How events move

1. Edge capture atomically allocates a monotonically increasing sequence and inserts an immutable event into SQLite (`WAL`, `synchronous=FULL`). No network is required.
2. The reference edge agent POSTs the stored payload over HTTPS with a per-device credential. It removes the record only after a matching event-ID ACK. Failed deliveries persist a retry count and capped exponential backoff with jitter. A missing ACK safely causes duplicate delivery.
3. The API derives tenant/device identity from the credential and checks it against the event. It validates sensor ranges and appliance phase, verifies device registration, and commits an inbox event plus audit row before acknowledging receipt.
4. The asynchronous worker only applies `lastSequence + 1`. Gaps remain pending. Machine state, completion/fault notifications, audit entry, and processed marker commit in one serializable transaction. Unique keys and a concurrency token protect repeat processing.
5. The dashboard refreshes every three seconds. `/api/fleet/stream` additionally exposes an authenticated SSE invalidation feed backed by the shared database. Notifications persist, so a disconnected browser catches up on refresh.

The AWS path accepts MQTT publications through AWS IoT Core, forwards them to a **standard** SQS queue, and uses the same durable inbox. IoT rules construct identity from the authorized topic instead of trusting payload tenant/device fields. SQS messages are deleted only after inbox persistence; failures retry and eventually reach the DLQ. MQTT PUBACK alone is not a database ACK. The reference edge implementation intentionally uses the HTTPS application ACK; a production MQTT edge transport needs a durable downstream receipt topic before deleting its outbox.

## Exercise offline recovery

Use a dedicated registered device, e.g. `W-EDGE`, so the demo simulator and physical edge agent do not allocate the same sequence stream.

```powershell
# Register W-EDGE with the dashboard. Generate a strong random key; never commit it.
$deviceKey = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$env:Devices__0__TenantId = 'oaxaca-central'
$env:Devices__0__DeviceId = 'W-EDGE'
$env:Devices__0__KeySha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($deviceKey)))
# Restart API from this environment so it receives the hashed key configuration.

$env:EDGE_DEVICE = 'W-EDGE'
$env:EDGE_KEY = $deviceKey
$env:EDGE_API = 'http://localhost:5214/api/telemetry' # Loopback only; remote endpoints require HTTPS.
dotnet run --project edge/LaundryOax.Edge.csproj -- capture
dotnet run --project edge/LaundryOax.Edge.csproj -- capture
dotnet run --project edge/LaundryOax.Edge.csproj -- replay
```

Stop the API before capture/replay to simulate an outage; restart it and watch the outbox drain. Stop and restart the edge process to verify disk persistence. `capture` currently generates a documented sample reading; replace it with a real sensor driver. Never reset an existing device's sequence database; provision a new device identity if its sequence state is irrecoverably lost. 4xx failures are retained with backoff and require operator intervention, not blind deletion.

## Production configuration

Production fails closed without `Auth__Authority` and `Auth__Audience`. Supply a real OIDC provider and `Auth__ClientId` for the SPA's authorization-code + PKCE login. Register the exact console origin `/` as its redirect URI and configure issuer CORS. Set `Auth__Scope` to include the API scope your provider requires. Access tokens must contain signed `tenant_id` and `roles` claims (`Viewer`, `Operator`, or `Admin`) with the configured API audience. Tokens are held in memory by the client. Refresh/reload requires a new sign-in redirect; provider SSO may make it immediate. Device ingestion uses separate `DeviceKey` authentication with constant-time SHA-256 comparisons; supply the key hashes through secret-backed configuration and rotate them per device.

Use `Fleet__Provider=SqlServer` and secret-backed `ConnectionStrings__Fleet` for shared state across API replicas. SQLite is for a single local instance; do not put it on network storage or scale SQLite containers horizontally. Create a **separate empty fleet database** during provisioning: `Fleet__Initialize=true` performs initial schema creation, then disable it. `EnsureCreated` is initial bootstrapping only, not an upgrade migration strategy. Existing legacy EF migrations belong to `LaundryOaxDBContext`; never apply them to the fleet database. Establish reviewed FleetDb migrations before evolving a deployed schema.

Deploy behind TLS ingress. Configure `Proxy__KnownProxy` for a trusted reverse-proxy IP or `Proxy__KnownNetwork` + `Proxy__PrefixLength` for the private ALB subnet CIDR; the API only trusts forwarded scheme information from this explicit source. Keep the API security group restricted to the load balancer. Database connections should enforce `Encrypt=True;TrustServerCertificate=False`. Use a database principal restricted to the fleet schema, encrypted volumes, secret rotation, and a dedicated deployment identity for schema changes.

## AWS and Docker

`docker build -t laundryoax .` builds Angular and the API into a non-root Linux container. `compose.yaml` requires real OIDC and a shared SQL Server connection string; it is not an anonymous demo. Put TLS termination in front of it and configure the trusted proxy. The root filesystem is read-only; `/tmp` is writable in Compose.

`infra/` provisions KMS-encrypted standard SQS + DLQ, restricted IoT device policies, topic rules, IAM, alarms, ECR scanning, and optional ECS Fargate + HTTPS ALB in an **existing** VPC. Set the `deployment` object to enable ECS; provide private subnets with outbound access, public ALB subnets, ACM certificate, immutable image, OIDC values, and a Secrets Manager secret containing the entire SQL Server connection string. Provision SQL Server separately and restrict its ingress to the API security group. For customer-managed encryption on the secret, add scoped `kms:Decrypt` to the execution role. The infrastructure is source-only until you run `terraform init`, `terraform plan`, review, and `terraform apply`; no AWS resources are deployed automatically.

The IoT rule uses the documented standard SQS action: https://docs.aws.amazon.com/iot/latest/developerguide/sqs-rule-action.html. Angular compatibility: https://angular.dev/reference/versions.

## Operations and known boundaries

- `/health/live` checks the process; `/health/ready` queries fleet storage. Structured logs and persistent audit rows record accepted/applied events. Queue age/DLQ alarms need your SNS topic ARNs to notify anyone. Rate limits are per API instance and tenant; place a global limiter/WAF upstream for shared quotas.
- Sequence gaps are deliberately not skipped. Alert on persistent pending events, inspect the missing sequence, and recover it from the edge. No automatic gap repair or retention cleanup is included. Add capacity/retention policies before sustained operation; deleting deduplication records too early compromises replay guarantees.
- Enable encrypted SQL backups and point-in-time recovery in your database service. For SQLite use its online backup API or stop writers before copying the DB and WAL; never copy only an active `.db` file. Test a restore in an isolated environment and reconcile last sequence with edge outboxes. Backup scheduling, restore automation, and recovery objectives remain deployment work.
- Rinse count and drying time are transparent baseline heuristics, not calibrated predictions or appliance control/safety logic. There is no remote machine actuation.
- Notifications are in-app and persistent. SMS, email, push-provider delivery, billing, real sensor drivers, and MQTT downstream receipt transport are not implemented.
- Legacy business controllers are disabled by default (`Legacy__Enabled=false`); explicitly enabling them still restricts access to `Admin` and retain the original single-tenant schema. They are **not** the multi-tenant fleet API. Existing placeholder business/auth endpoints and payment logic are not production features; do not expose them to tenant users. OrderService's broken references were replaced with validated EF persistence, not a pretend payment processor.
- SQL Server multi-replica contention, real AWS delivery, OIDC integration, and container deployment require integration testing in your environment. Local automated tests exercise SQLite and test-only authenticated identities, not an external issuer.

# LaundryOax infrastructure operations runbook

Use this as interview preparation and a local diagnostic guide. AWS procedures are proposed operating steps for the undeployed infrastructure, not a record of production incidents. Run commands only in an environment you administer. Start with observation; do not purge queues, reset sequences, or mutate processed markers as a shortcut.

## The four separate backlogs

| Location | Meaning | First evidence |
| --- | --- | --- |
| Edge outbox | Captured events not yet application-acknowledged | Outbox rows, oldest sequence, attempts and next_attempt |
| SQS queue | AWS messages not yet deleted after inbox commit | Visible/in-flight count, oldest-message age, DLQ |
| Fleet inbox | Persisted events not applied to projection | Events.ProcessedUtc is null, compared to machine LastSequence |
| Browser view | Last successful snapshot may be stale | Browser/API response, serverTime, connection state |

A queue can be empty while the fleet inbox is stuck. A replay can succeed while an old observation still makes a machine look offline. A healthy process can coexist with a failed worker. Always identify the boundary before choosing a mitigation.

## Local read-only checks

Run from the repository root. The demo API's HTTP endpoint is loopback-only; production requests require the configured user/device identity.

```powershell
Invoke-RestMethod 'http://localhost:5214/health/live'
Invoke-RestMethod 'http://localhost:5214/health/ready'
$snapshot = Invoke-RestMethod 'http://localhost:5214/api/fleet/snapshot'
$snapshot | Select-Object tenant, serverTime, received, pending
$snapshot.machines | Select-Object deviceId, phase, lastSequence, lastSeenUtc
Get-NetTCPConnection -LocalPort 5214 -State Listen
Get-PSDrive -PSProvider FileSystem
```

These calls do not prove external OIDC, SQS, or worker health. Readiness only checks a fleet table. For the local demo, the dashboard is at http://localhost:5214 after the frontend build, or http://localhost:4200 when using the Angular dev server.

## Read-only SQL for sequence gaps

Use an authorized operational connection. This query is intentionally tenant/device scoped in its grouping but views all returned tenants; it is for an administrator, not a tenant-facing API.

```sql
SELECT m.TenantId, m.DeviceId, m.LastSequence,
       MIN(e.Sequence) AS FirstPendingSequence,
       COUNT(e.EventId) AS PendingCount,
       MIN(e.ReceivedUtc) AS OldestPendingReceivedUtc
FROM Machines AS m
JOIN Events AS e
  ON e.TenantId = m.TenantId AND e.DeviceId = m.DeviceId
WHERE e.ProcessedUtc IS NULL
GROUP BY m.TenantId, m.DeviceId, m.LastSequence;
```

If FirstPendingSequence exceeds LastSequence + 1, investigate the missing sequence. If it equals the next sequence, inspect worker exceptions, database locks, and process activity. Do not assume every pending row is caused by a gap. SQL Server and SQLite timestamps/tools differ; verify the schema and execution plan in the environment you are inspecting.

```sql
-- Edge database, separate from FleetDb.
SELECT tenant, device, MIN(sequence) AS oldest_sequence,
       COUNT(*) AS pending_count, MAX(attempts) AS max_attempts
FROM outbox
GROUP BY tenant, device;

SELECT id, tenant, device, sequence, attempts, next_attempt
FROM outbox
ORDER BY tenant, device, sequence
LIMIT 25;
```

The second query uses SQLite LIMIT and is for the edge store. next_attempt is Unix milliseconds. A future timestamp can mean normal backoff; it is not necessarily a stuck process.

## AWS diagnostic examples

These are templates for a configured account with the AWS CLI, not commands executed during local validation. Set identifiers from your actual deployment. Never paste secret values into command history or interview screenshots.

```powershell
aws sqs get-queue-attributes --queue-url "$env:QUEUE_URL" --attribute-names ApproximateNumberOfMessages ApproximateNumberOfMessagesNotVisible VisibilityTimeout RedrivePolicy MessageRetentionPeriod
aws ecs describe-services --cluster "$env:ECS_CLUSTER" --services "$env:ECS_SERVICE"
aws elbv2 describe-target-health --target-group-arn "$env:TARGET_GROUP_ARN"
aws logs tail "$env:LOG_GROUP" --since 15m
Resolve-DnsName "$env:API_HOST"
Test-NetConnection "$env:API_HOST" -Port 443
```

Oldest-message age is a CloudWatch metric, not one of the GetQueueAttributes values above. Use the deployed alarm/metric view or a specific CloudWatch query for it. The current Terraform does not create DNS records or SNS subscriptions.

## Linux practice commands

Practice these in a disposable Linux environment before claiming hands-on experience with them. The implementation was built locally on Windows.

```bash
ss -ltnp
df -h
df -i
ps aux
docker ps
docker logs --tail 100 <container-name>
curl --fail --show-error https://<api-host>/health/live
openssl s_client -connect <api-host>:443 -servername <api-host>
```

Angle-bracket values are placeholders to replace, not literal commands to run. Review certificate/hostname verification with an appropriate client; successful TCP connection alone does not prove TLS validation or application authorization.

## Incident diagnosis matrix

| Symptom | Likely boundaries to investigate | First safe action | Verify after mitigation |
| --- | --- | --- | --- |
| API not listening | Process startup/config/runtime/port | Read startup errors and listener state | Liveness and real endpoint respond |
| Liveness 200 readiness 503 | Fleet connection/schema/permissions | Inspect connection target and DB errors without printing credentials | Readiness plus tenant snapshot succeeds |
| ALB 503 | No healthy targets/task crash/SG/port | Describe service and target health; read stopped-task reason | Healthy targets and authenticated API request |
| Redirect loop | Forwarded scheme/trusted proxy/HTTPS port | Compare original and backend scheme handling | HTTPS succeeds without repeated redirects |
| Device 401 | Missing/wrong/rotated key | Verify key hash mapping and device environment privately | Same retained event receives a valid receipt |
| Device 403 | Valid key bound to different tenant/device | Compare authenticated mapping to payload identity | Correct scoped device accepts; impersonation still fails |
| Device 404 | Machine unregistered | Verify registration in the correct tenant | Registration exists and original event is accepted |
| Device 409 | Reused identity/content or sequence owner | Compare retained event ID, hash, and sequence record | Resolve producer ownership; do not rewrite committed history |
| Repeated 429 | Shared tenant limiter/replay storm | Measure tenant request load and retry schedule | Controlled recovery without dashboard starvation |
| Growing edge outbox | Network/API/auth/backoff/disk | Inspect earliest retained event and logs | ACK count advances and outbox drains in order |
| SQS grows | Receiver off/config/IAM/KMS/DB | Check QueueUrl, task identity, receiver logs | Receive/delete rates resume and age decreases |
| DLQ gets messages | Invalid payload/registration/permission failures | Inspect representative failure and fix root cause | Controlled redrive persists original identities |
| SQS empty inbox pending | Sequence gap/projector failure | Run the gap query and inspect worker errors | Eligible sequences process; no duplicate notification |
| UI stale but API current | Browser request/auth/cache/client clock | Inspect response/error and serverTime | Next snapshot reflects the authoritative state |
| Machine looks offline after replay | ObservedAt is old | Compare receipt and observation timestamps | Report freshness accurately; no claim of lost events |
| Container fails to start | Image/config/secret access/runtime writes | Read task stop reason and execution-role errors | Startup plus readiness and ingestion work |
| SQL deadlocks or long waits | Serializable processing/concurrent hosts | Gather lock waits, transaction timing and workload | Retry recovers and measured contention decreases |
| Edge disk full | Storage capacity/retention | Preserve existing outbox; restore safe free capacity | New capture commits without resetting sequence |
| Credential compromise suspected | Device/user identity boundary | Revoke/rotate through the appropriate identity owner | Unauthorized identity fails; legitimate replacement succeeds |
| Restored DB is behind edge ACKs | Backup/replay retention boundary | Stop risky replay and compare recovery checkpoints | Missing committed history is reconciled or explicitly declared lost |

## Sequence gap recovery procedure

1. Identify tenant, device, LastSequence, earliest pending sequence, and last successful processing time.
2. Search the inbox and edge store for the missing identity; inspect receiver/DLQ errors if the AWS path is in use.
3. If the original event still exists, replay it unchanged through the normal ingestion path after fixing the transport/provisioning issue.
4. Verify application order and that later pending events drain. Confirm notifications were not duplicated.
5. If the event is irrecoverable, document the loss. The current application has no supported resynchronization endpoint. Design an explicit audited epoch/snapshot recovery before bypassing ordering; do not silently increment LastSequence.

## Backup restore and reconciliation

**Current status:** a proposed operating procedure, not implemented automation or a completed recovery drill.

- Define the authoritative store for each acknowledged event and the required replay horizon before setting RPO/RTO.
- Use database-supported backups/PITR and protected storage. For SQLite, use its [online backup interface](https://www.sqlite.org/backup.html) or stop writers safely; copying only a live .db can omit WAL content.
- Test restore into an isolated database with the exact application/schema version. Verify integrity, machine sequence checkpoints, event identities, and notifications.
- Reconcile acknowledged events after the restored timestamp. Edge ACK deletes and SQS deletes mean neither transport is necessarily still a recovery source.
- Keep a record of actual recoverable point, measured recovery time, and unresolved gaps. Do not declare zero loss without evidence.
- Restore access/configuration safely, resume producers in a controlled order, monitor duplicates/conflicts/lag, and preserve an incident audit trail.

## Proposed deployment and rollback checklist

1. Validate the immutable image, real OIDC, separate SQL database, schema migration plan, and exact network/IAM inputs in a nonproduction environment.
2. Configure protected Terraform state and review a contextual plan. `validate` does not contact all remote services or prove an apply will succeed. [Terraform validation reference](https://developer.hashicorp.com/terraform/cli/commands/validate).
3. Retain the previous task definition/image and confirm backward-compatible event/schema behavior.
4. Deploy with health checks, observe receiver and projector progress, and run a scoped synthetic event through the actual boundary.
5. If rollback is needed, use the previous known-good task/image and verify database compatibility. The configured ECS circuit breaker is not a database rollback mechanism.
6. Confirm queued and pending events recover without manual deletion, then record the outcome.

## Observability plan to propose

| SLI or signal | Why it matters | Current evidence |
| --- | --- | --- |
| Capture success and free disk | Events cannot recover if never captured | No custom metric |
| Oldest edge pending age | Identifies delivery/provisioning failure | Outbox rows/log output only |
| Valid accept success and latency | Measures receipt boundary | Structured persistence logs; no histogram |
| Pending eligible-event age | Detects projector stalls independent of SQS | Snapshot count only, no age alarm |
| Missing-sequence age | Detects blocked device histories | No explicit metric/repair workflow |
| SQS oldest age and DLQ depth | Detects receiver backlog/poison messages | Terraform alarms, undeployed |
| DB lock waits and transaction latency | Exposes contention before scaling | No integrated dashboard |
| Notification availability lag | Measures user-visible result | Persistent records, no delivery histogram |
| Dependency/auth failures | Finds external service problems | Logs/status responses, no complete tracing |

Propose targets only after defining the eligible event population and measuring a baseline. An SLO example is a target to evaluate, not an achieved project metric. An error budget is allowed unreliability over the defined window; no actual error budget policy is configured here.

## Interview incident update template

“At [time], [observed impact] affects [confirmed scope]. We have confirmed [durable evidence] and are investigating [uncertain boundary]. The current mitigation is [reversible action]. We will verify [invariant and user-visible result] and provide the next update at [time].”

Use real observations during a real incident. For a practice exercise, explicitly label the scenario hypothetical.

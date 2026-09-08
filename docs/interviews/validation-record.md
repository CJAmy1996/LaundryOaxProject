# LaundryOax validation and evidence record

**Review date:** September 5 2026. **Source baseline:** [545e6a3521c6b8d744e8393e9784b478251a7859](https://github.com/CJAmy1996/LaundryOaxProject/commit/545e6a3521c6b8d744e8393e9784b478251a7859). **Purpose:** separate observed evidence from implemented source and proposed operating behavior.

## Recorded results

| Check | Result | What it establishes | What it does not establish |
| --- | --- | --- | --- |
| API Release build | Passed, zero warnings/errors in recorded build | .NET 8 project compiles | Production behavior or complete business functionality |
| Edge Release build | Passed, zero warnings/errors | Edge executable compiles | Real sensor or MQTT integration |
| Angular production build | Passed | Type/template compilation and bundle generation | Browser correctness, accessibility, or performance benchmark |
| Ten local tests | Passed; rerun during this documentation task | The cases below on SQLite/test identities | Real OIDC, SQL Server replicas, power-loss or cloud behavior |
| Terraform init -backend=false then validate | Passed with Terraform 1.12.2 and AWS provider 5.100.0 | Local configuration/schema consistency | A successful plan/apply or real resource operation |
| Local API smoke test | HTTP page 200, readiness ready, eight machines, eight accepted/received, zero pending after processing | One simulated end-to-end batch | Throughput, uptime, physical device count, or durable disaster recovery |
| npm audit | Zero reported vulnerabilities at recorded scan | Current registry advisory result for installed packages | Complete application or supply-chain security |
| .NET dependency scan | No reported vulnerable packages after SQLite bundle 2.1.13 update | Current source advisory result | Complete security assessment or future vulnerability absence |
| Docker engine | Unavailable locally | Container runtime validation could not be performed locally | A passing container build/run |
| GitHub Actions | Workflow defined; consult the specific pushed commit's Actions run | CI configuration exists | A passing run until actually observed |

The build host has .NET SDK 9.0.305 and .NET 8.0.20 runtime installed; the application targets net8.0. The local frontend build used Node 25.9.0, which emitted a non-LTS warning; CI and Docker specify Node 22. Do not present that local environment as a validated production runtime policy.

## Exact local test cases

1. `Duplicate_event_is_acknowledged_once_and_conflicting_reuse_rejected`
2. `Out_of_order_events_wait_then_apply_in_sequence`
3. `Worker_restart_preserves_pending_events_and_does_not_repeat_notifications`
4. `Parallel_duplicate_delivery_only_creates_one_inbox_row`
5. `Device_identity_cannot_impersonate_another_device_or_tenant`
6. `Tenant_reads_are_isolated_and_viewers_cannot_register_machines`
7. `Invalid_sensor_readings_do_not_enter_the_inbox`
8. `Unknown_notification_cannot_be_acknowledged_cross_tenant`
9. `Edge_restart_retains_events_and_allocates_monotonic_sequences`
10. `Invalid_edge_capture_rolls_back_sequence_allocation`

The fixture uses WebApplicationFactory, a unique temporary SQLite database, and a test-only authentication scheme. Hosted workers are removed for deterministic processing. The restart tests instantiate new objects over a persisted file; they do not terminate a process at an arbitrary write instruction. The parallel duplicate test uses the same singleton pipeline gate.

## Reproduction commands

```powershell
dotnet build LaundryOaxWebAPI.csproj -c Release
dotnet test tests/LaundryOax.Tests.csproj -c Release
dotnet build edge/LaundryOax.Edge.csproj -c Release
dotnet list LaundryOaxWebAPI.csproj package --vulnerable --include-transitive
```

```powershell
cd client
npm ci
npm run build
npm audit --audit-level=high
```

```powershell
cd infra
terraform init -backend=false
terraform validate
```

Record exact versions, date, environment, and output when you rerun these yourself. The commands are reproducible instructions; future results can differ as tooling/advisory sources change.

## Claim promotion checklist

- **Configured OIDC → verified OIDC:** complete a real issuer login and negative issuer/audience/role/tenant tests.
- **SQL provider option → verified SQL deployment:** apply a reviewed fleet schema and run invariants under independent concurrent workers.
- **Dockerfile → verified container:** build and run the image with production-like config, filesystem permissions, and ingress.
- **Terraform source → deployed infrastructure:** review plan, apply in an authorized environment, verify resource and application behavior.
- **Backup procedure → verified recovery:** restore into isolation, reconcile acknowledged events, and measure recovery point/time.
- **Scaling design → supported capacity:** conduct representative load/replay tests with measured latency, resource use, contention, and recovery behavior.
- **Basic logs/alarms → operational observability:** deploy metrics/alerts, route notifications, and run detection exercises.

## Source fingerprints

[source-fingerprints.json](source-fingerprints.json) records SHA-256 digests of the reviewed implementation files. These identify the local text snapshot used for this audit; line endings can differ across platforms. The Git commit is the canonical cross-platform source identity.

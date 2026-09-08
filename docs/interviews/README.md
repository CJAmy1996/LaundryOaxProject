# LaundryOax interview preparation

A source-backed study pack for discussing LaundryOax in software engineering and infrastructure operations interviews. Start with the claim audit, then learn the event lifecycle and practice answering aloud. The defensible project story is a working local IoT prototype with tested durability and authorization behaviors plus an undeployed AWS design.

**Reviewed source:** [545e6a3](https://github.com/CJAmy1996/LaundryOaxProject/commit/545e6a3521c6b8d744e8393e9784b478251a7859) · **Review date:** September 5 2026 · **Question count:** 173

## Read in this order

| Document | What it prepares you to explain |
| --- | --- |
| [Claim audit](claim-audit.md) | What is true today, safer resume wording, and claims to avoid |
| [Architecture and code map](architecture.md) | Event flow, acknowledgment boundaries, transactions, source locations, sizing |
| [173 interview questions](question-bank.md) | Short, defensible answers across engineering, infrastructure, security, and operations |
| [Operations runbook](operations-runbook.md) | Diagnostic commands, incident scenarios, backlog/gap analysis, restore and rollback reasoning |
| [Study plan and speaking practice](study-plan.md) | Role-specific introductions, demonstrations, exercises, STAR prompts, glossary |
| [Validation record](validation-record.md) | Exact verified evidence and what the tests do not establish |
| [Portable study guide](LaundryOax-Interview-Guide.html) | Self-contained reading and printing version of the whole pack |

## Two routes through the material

**Software engineering:** question-bank sections on architecture, edge processing, C# APIs, data/concurrency, security, Angular, and testing. Be able to explain why a lost ACK causes a safe duplicate, why a missing sequence waits, and why one singleton test is not a distributed concurrency proof.

**Infrastructure operations:** sections on AWS, networking/Docker/Terraform, operations/recovery, security, and sizing. Be able to distinguish edge backlog, SQS backlog, inbox backlog, and browser staleness, then explain the permissions and network dependencies at each boundary.

## Five rules for accurate answers

1. Say **implemented**, **verified locally**, **configured but undeployed**, or **proposed** before giving a strong guarantee.
2. Explain what an ACK means and where a transaction stops.
3. Give measured numbers only with their test conditions. Eight simulated machines is not a production fleet size.
4. Separate personal experience from AI-assisted implementation and hypothetical incident responses.
5. When you do not know, identify the missing assumption and the experiment or authoritative documentation that would resolve it.

The GitHub-rendered Markdown is the maintained source. The HTML is a portable copy; open it locally to read or print. Code links in the guide are pinned to the audited implementation revision, so later changes do not silently change the evidence behind an answer.

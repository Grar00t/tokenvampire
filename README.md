# TokenVampire

> **AI can sound intelligent and still fail the job. TokenVampire measures what actually happened.**

TokenVampire is a local-first evidence, billing, subscription, and AI-delivery assurance system for individuals first and organizations second.

It is built around a simple idea:

**Intelligence is not the confidence of the answer. It is whether the requested work was delivered, supported by evidence, reconciled with what was charged, and reproducible by someone other than the model that produced it.**

A fluent answer can still be incomplete.  
A successful API response can still produce a failed task.  
A citation can be syntactically valid and still fail to support the claim.  
A usage limit can be consumed without telling you whether the resulting work was useful.  
A credit top-up is not measured spend.  
Missing records are not zero.

TokenVampire exists to preserve those distinctions.

## Governing principle

> **No evidence, no finding. No consent, no export. No verified amount, no loss total. No human approval, no submission.**

The software does not care how famous the vendor is, how persuasive the model sounds, or how large the company valuation is.

It asks:

1. **What did the user request?**
2. **What did the system claim it did?**
3. **What observable artifact proves delivery?**
4. **What evidence supports the answer?**
5. **What usage and money were actually recorded?**
6. **What rework was required?**
7. **What remedy, credit, reset, refund, or response actually occurred?**

## One standard for every AI vendor

The audit method is vendor-neutral:

**OpenAI · Google · Anthropic · xAI · Microsoft · Meta · Perplexity · and any other provider**

The company name does not change the test. Payment does not change the score.

![TokenVampire evidence-first audit](research/anthropic/2026-10-06/TokenVampire_Anthropic_Evidence_Legal_20261006.svg)

## The reality of "intelligence"

TokenVampire does not use an LLM's self-description as proof that a task succeeded.

| What an AI system may say | What TokenVampire asks |
| --- | --- |
| "I completed the task." | Where is the artifact, and can it be independently inspected? |
| "The source supports this." | Does the source semantically support the exact claim? |
| "The request succeeded." | Did the user's acceptance criteria succeed? |
| "Your usage was reset." | Was usage capacity reset, was credit restored, or was cash refunded? These are different events. |
| "No usage is shown." | Is the value actually zero, or is it unknown? |
| "The file hash matches." | Does it prove file identity/integrity only, or is someone incorrectly treating it as proof that every claim inside the file is true? |

The core distinction is:

> **Generated language is a claim. Observable evidence is a different thing.**

## Public evidence example — Anthropic / Claude

TokenVampire includes a dated public-source study because audit rules should be demonstrated against real, named vendors rather than abstract examples.

### 23 April 2026 — Anthropic postmortem

Anthropic published a Claude Code quality postmortem describing three product changes affecting Claude Code, Claude Agent SDK, and Cowork. It described a context-management bug that repeatedly dropped prior reasoning and caused cache misses, and said it **believes this drove reports of usage limits draining faster than expected**. Anthropic also said subscriber usage limits were reset and that the API/inference layer was not impacted.

Source: [Anthropic — April 23 postmortem](https://www.anthropic.com/engineering/april-23-postmortem)

### Public review snapshot — 6 October 2026

The research snapshot recorded:

- **claude.ai:** 2,029 Trustpilot reviews; **80% 1-star**
- **anthropic.com:** 499 Trustpilot reviews; **88% 1-star**

These are preserved as public review counts. TokenVampire does not silently convert self-selected reviews into a population-wide failure rate.

Sources: [claude.ai on Trustpilot](https://www.trustpilot.com/review/claude.ai) · [anthropic.com on Trustpilot](https://www.trustpilot.com/review/anthropic.com)

### Financial scale

Anthropic announced on **28 May 2026** that it raised **$65 billion** in Series H funding at a **$965 billion post-money valuation**. Reuters later reported that Anthropic's annual revenue run-rate topped **$65 billion by the end of July 2026**.

A valuation is not cash. A run-rate is not audited full-year revenue. Those distinctions are preserved because financial scale does not replace evidence.

Sources: [Anthropic Series H](https://www.anthropic.com/news/series-h) · [Reuters syndicated report](https://www.investing.com/news/stock-market-news/anthropic-revenue-run-rate-tops-65-billion-source-says-4864031)

### Productivity is measurable too

METR's July 2025 randomized study covered **16 experienced open-source developers and 246 tasks**. In that study setting, early-2025 AI tooling—primarily Cursor Pro with Claude 3.5/3.7 Sonnet in the AI-allowed condition—made the measured tasks take **19% longer**.

That result is not a timeless score for every model. It demonstrates why productivity should be measured rather than assumed.

Source: [METR — early-2025 AI experienced open-source developer study](https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/)

The full research package, public issue register, methodology, and source register are preserved under:

- [research/anthropic/2026-10-06/](research/anthropic/2026-10-06/)
- [INFOGRAPHIC_SOURCES.md](research/anthropic/2026-10-06/INFOGRAPHIC_SOURCES.md)
- [public_issue_register.csv](research/anthropic/2026-10-06/public_issue_register.csv)
- [source_register.csv](research/anthropic/2026-10-06/source_register.csv)

## Six audit controls

1. **Delivery truth** — Was the requested work actually delivered as claimed?
2. **Semantic support** — Are material claims supported by verifiable sources and data?
3. **Cost and rework** — What usage, cost, retries, and repeat work actually occurred?
4. **Editorial consistency** — Did the system materially change facts, named entities, or conclusions without instruction?
5. **User agency and privacy** — Does the user retain control over evidence, data, and export?
6. **Challenge and remedy** — Is there a traceable way to dispute a result and record the provider's response or remedy?

## Evidence rules

TokenVampire keeps accounting and evidence states explicit:

- **Missing data = unknown, never zero.**
- **Credit top-up ≠ measured spend.**
- **Usage-limit reset ≠ cash refund.**
- **A failed probe ≠ a failed project.**
- **Valid citation syntax ≠ semantic support.**
- **A cryptographic hash proves the identity/integrity of a file version, not the truth of every statement inside it.**
- **No LLM may be the sole judge of a material finding.**

Canonical evidence states include: **Verified, ProviderReported, UserAsserted, Estimated, ScenarioOnly, and Unknown.**

## Legal and governance basis

TokenVampire records facts first and keeps legal conclusions outside the automatic scoring engine.

For Saudi use cases, relevant sources include:

- [Saudi E-Commerce Law](https://laws.boe.gov.sa/BoeLaws/Laws/LawDetails/360de590-0286-4fa5-a243-aa9100c31979/1)
- [Saudi Ministry of Commerce guidance on electronically provided services](https://mc.gov.sa/ar/mediacenter/News/Pages/10-07-19-01.aspx)
- [Saudi Personal Data Protection Law / National Data Governance Platform](https://dgp.sdaia.gov.sa/wps/portal/pdp/knowledgecenter/details/GPDPL)

Governance references include [NIST AI RMF](https://www.nist.gov/itl/ai-risk-management-framework) and ISO/IEC 42001. They are governance references, not substitutes for applicable law.

## What is implemented

The repository currently contains implemented building blocks for:

- domain truth models and evidence-backed verdicts;
- measured / assumed / unknown value handling;
- billing and usage reconciliation;
- evidence hashing and manifests;
- encrypted local evidence storage and hostile-file ingest controls;
- persisted case data;
- localized reporting and CLI report generation;
- Arabic / English i18n foundations;
- unit and architecture tests;
- cross-platform CI.

Desktop and broader organization/server experiences remain under development. Passing tests prove the tested behavior at a specific revision; they do not prove that the entire product is complete or that every external claim is true.

## Architecture

- .NET 10 and C#
- Avalonia desktop direction
- SQLite
- encrypted local evidence vault
- optional self-hosted organization mode
- English and Arabic as release-blocking languages
- explicit architecture boundaries and automated verification

## Repository contracts

- [Product contract](docs/architecture/product-contract.md)
- [Repository map](docs/architecture/repository-map.md)
- [Execution contract](docs/operations/copilot-execution-contract.md)
- [License decision](docs/decisions/LICENSE-DECISION.md)
- [Milestones](docs/operations/milestones.md)
- [Security policy](SECURITY.md)
- [Contribution guide](CONTRIBUTING.md)

## The point

TokenVampire is not another AI personality.

**It is a meter.**

It preserves the request, the output, the evidence, the usage, the money, the missing data, and the remedy—then lets a human reviewer see the difference between **what intelligence claimed** and **what reality can prove**.

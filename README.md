# TokenVampire

<p align="center">
  <strong>Evidence before inference. One audit standard for every AI vendor.</strong>
</p>

<p align="center">
  <img src="research/anthropic/2026-10-06/TokenVampire_Anthropic_Evidence_Legal_20261006.svg" alt="TokenVampire evidence-first AI audit framework" width="100%">
</p>

> **A fluent system can sound intelligent and still fail the job. TokenVampire records what was requested, what was delivered, what was charged, and what the evidence can actually prove.**

TokenVampire is a local-first evidence, billing, subscription, and AI-delivery assurance system for individuals first and organizations second.

It does not grade AI by personality, confidence, brand, valuation, or marketing language. It grades observable delivery.

## The Wrapper Ledger

Conversational systems can present a human-like surface: first-person language, confidence, memory cues, reassurance, and apparent certainty.

TokenVampire deliberately ignores that surface.

It asks a smaller set of harder questions:

1. **What did the user request?**
2. **What did the system claim it did?**
3. **What observable artifact proves delivery?**
4. **What evidence supports the material claims?**
5. **What usage and money were actually recorded?**
6. **What repeat work or rework was required?**
7. **What remedy, credit, reset, refund, or provider response actually occurred?**

The result is a ledger, not a personality judgment.

> **Generated language is a claim. Observable evidence is a different thing.**

## Governing principle

> **No evidence, no finding. No consent, no export. No verified amount, no loss total. No human approval, no submission.**

## One standard for every vendor

**OpenAI · Google · Anthropic · xAI · Microsoft · Meta · Perplexity · any other provider**

The company name does not change the test. Payment does not change the score.

### Six audit controls

| Control | Question |
| --- | --- |
| **Delivery truth** | Was the requested work actually delivered as claimed? |
| **Semantic support** | Do the cited sources and data support the material claim? |
| **Cost & rework** | What usage, charges, retries, and repeat work actually occurred? |
| **Editorial consistency** | Did the system materially alter names, facts, scope, or conclusions without instruction? |
| **User agency & privacy** | Does the user retain control over evidence, data, consent, and export? |
| **Challenge & remedy** | Is there a traceable dispute path and a recorded provider response or remedy? |

## Reality check: what "intelligence" does not prove

| What a system may say | What the audit requires |
| --- | --- |
| **"I completed the task."** | An observable artifact that can be independently inspected. |
| **"The source supports this."** | Semantic support for the exact material claim. |
| **"The request succeeded."** | The user's acceptance criteria actually passing. |
| **"Your usage was reset."** | A distinction between limit reset, restored credit, and cash refund. |
| **"No usage is shown."** | Proof that the value is zero rather than unknown or unavailable. |
| **"The hash matches."** | File identity/integrity only; not automatic truth of every statement inside the file. |

## Public evidence example — Anthropic / Claude

The repository includes a dated public-source study using a real, named vendor so the audit method can be inspected against public evidence rather than hypothetical examples.

### Documented facts

| Record | Documented figure or event | Source |
| --- | --- | --- |
| **Anthropic postmortem — 23 Apr 2026** | Anthropic described three product changes affecting Claude Code, Claude Agent SDK, and Cowork. It reported a context-management bug that repeatedly dropped prior reasoning and caused cache misses, and said it believes this drove reports of usage limits draining faster than expected. Subscriber usage limits were reset. | [Anthropic](https://www.anthropic.com/engineering/april-23-postmortem) |
| **claude.ai review snapshot — 6 Oct 2026** | **2,029 reviews; 80% 1-star** | [Trustpilot](https://www.trustpilot.com/review/claude.ai) |
| **anthropic.com review snapshot — 6 Oct 2026** | **499 reviews; 88% 1-star** | [Trustpilot](https://www.trustpilot.com/review/anthropic.com) |
| **Series H — 28 May 2026** | **$65B raised; $965B post-money valuation** | [Anthropic](https://www.anthropic.com/news/series-h) |
| **Revenue run-rate — end Jul 2026** | **>$65B annual revenue run-rate** reported by Reuters | [Reuters syndicated report](https://www.investing.com/news/stock-market-news/anthropic-revenue-run-rate-tops-65-billion-source-says-4864031) |
| **METR study — Jul 2025** | **16 experienced developers, 246 tasks; AI-allowed work took 19% longer in that study setting** | [METR](https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/) |

TokenVampire preserves each item at the level the source supports. A review count stays a review count. A valuation stays a valuation. A run-rate stays a run-rate. A provider statement stays a provider statement. A user report stays a user report until independently reproduced.

**The repository presents the record. Readers, auditors, regulators, customers, and courts can draw their own conclusions from the evidence.**

### Research files

- [Anthropic / Claude research package](research/anthropic/2026-10-06/)
- [Infographic sources](research/anthropic/2026-10-06/INFOGRAPHIC_SOURCES.md)
- [Public issue register](research/anthropic/2026-10-06/public_issue_register.csv)
- [Source register](research/anthropic/2026-10-06/source_register.csv)
- [Methodology](research/anthropic/2026-10-06/METHODOLOGY.md)

## Rules of evidence

- **Missing data = UNKNOWN, never zero.**
- **Credit top-up ≠ measured spend.**
- **Usage-limit reset ≠ cash refund.**
- **A failed probe ≠ a failed project.**
- **Valid citation syntax ≠ semantic support.**
- **A cryptographic hash proves file identity/integrity, not claim truth.**
- **No LLM may be the sole judge of a material finding.**

Canonical evidence states include:

`Verified` · `ProviderReported` · `UserAsserted` · `Estimated` · `ScenarioOnly` · `Unknown`

## Legal and governance basis

TokenVampire records technical and financial facts first. Legal classification is kept separate from the automatic scoring engine.

For Saudi use cases, relevant public sources include:

- [Saudi E-Commerce Law](https://laws.boe.gov.sa/BoeLaws/Laws/LawDetails/360de590-0286-4fa5-a243-aa9100c31979/1)
- [Saudi Ministry of Commerce — electronically provided services and E-Commerce Law](https://mc.gov.sa/ar/mediacenter/News/Pages/10-07-19-01.aspx)
- [Saudi Personal Data Protection Law / National Data Governance Platform](https://dgp.sdaia.gov.sa/wps/portal/pdp/knowledgecenter/details/GPDPL)

Governance references include [NIST AI RMF](https://www.nist.gov/itl/ai-risk-management-framework) and ISO/IEC 42001. These are governance references, not substitutes for applicable law.

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

Desktop and broader organization/server experiences remain under development. Passing tests prove the tested behavior at a specific revision; they do not prove that the entire product is complete.

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

**It is an evidence meter.**

It preserves the request, output, artifacts, usage, money, missing data, provenance, and remedy—so the difference between **what a system said** and **what reality can prove** remains inspectable.

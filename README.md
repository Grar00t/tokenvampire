# TokenVampire

TokenVampire is a local-first evidence, billing, subscription, and AI-delivery assurance application for individuals first and organizations second.

It preserves original evidence, separates verified facts from assertions and scenarios, reconciles charges with invoices and usage, checks whether claimed digital work produced observable artifacts, and generates reviewable remedy packages.

## Governing principle

> No evidence, no finding. No consent, no export. No verified amount, no loss total. No human approval, no submission.

## Boundaries

TokenVampire does not automatically accuse any party of theft, fraud, illegality, or non-compliance. It is not a lawyer, regulator, bank, payment processor, or accredited certification body. Personal mode will remain local-first, offline-capable, and independent from the optional organization server.

## Planned stack

- .NET 10 LTS and C#
- Avalonia desktop UI
- SQLite and an encrypted content-addressed evidence vault
- Optional self-hosted ASP.NET Core organization mode
- English and Arabic as release-blocking languages
- ICU/CLDR, BCP 47, RTL, UAX #9, and UTS #39 foundations

## Repository contracts

- [Product contract](docs/architecture/product-contract.md)
- [Repository map](docs/architecture/repository-map.md)
- [Assurance capability roadmap](docs/architecture/assurance-roadmap.md)
- [Execution contract](docs/operations/copilot-execution-contract.md)
- [License decision](docs/decisions/LICENSE-DECISION.md)
- [Milestones](docs/operations/milestones.md)
- [Security policy](SECURITY.md)
- [Contribution guide](CONTRIBUTING.md)

## Status

M0 foundation closure is in progress. Early deterministic domain, evidence, billing, storage, reporting, i18n, and CLI components have landed, but the M1 Personal MVP is not complete and the Desktop surface remains non-production.

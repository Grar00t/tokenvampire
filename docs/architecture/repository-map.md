# Repository Map

## Dependency direction

Presentation -> Application -> Domain.

Deterministic engines implement evidence, billing, subscription, AI assurance, Unicode security, jurisdictions, compliance, and reporting. Infrastructure implements SQLite, encrypted storage, file-system, signing, local IPC, and optional network adapters. The optional organization server must never become a Personal dependency.

## Target projects

```text
src/
  TokenVampire.Domain/
  TokenVampire.Application/
  TokenVampire.Evidence/
  TokenVampire.Billing/
  TokenVampire.Assurance/
  TokenVampire.Remedies/
  TokenVampire.Jurisdictions/
  TokenVampire.Compliance/
  TokenVampire.I18n/
  TokenVampire.Reporting/
  TokenVampire.Infrastructure/
  TokenVampire.Plugin.Abstractions/
  TokenVampire.Presentation/
  TokenVampire.Desktop/
  TokenVampire.Cli/
  TokenVampire.Server/
workers/
  TokenVampire.ParserWorker/
tests/
  TokenVampire.UnitTests/
  TokenVampire.IntegrationTests/
  TokenVampire.ArchitectureTests/
  TokenVampire.ContractTests/
  TokenVampire.SecurityTests/
  TokenVampire.I18nTests/
  TokenVampire.GoldenTests/
  TokenVampire.FuzzTests/
docs/
  adr/ architecture/ threat-model/ data-flows/ controls/
  jurisdictions/ operations/ decisions/
```

## Enforced boundaries

- Domain has no UI, persistence, networking, file-system, ORM, or vendor dependency.
- Application depends on Domain and port contracts only.
- Personal Desktop has no Server dependency.
- UI contains no billing arithmetic or legal classification logic.
- Plugins have no direct database or keystore access.
- Parser workers have no network or vault key and are resource bounded.
- No LLM is an authoritative material judge.

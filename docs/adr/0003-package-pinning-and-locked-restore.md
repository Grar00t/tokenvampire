# ADR 0003: Central package pinning and locked restore

- Status: Accepted
- Scope: M0 foundation

## Context

Build and test results must not silently change because a dependency resolver selected a newer package.

## Decision

Pin direct NuGet package versions centrally in `Directory.Packages.props`, commit project lock files, and use locked restore in verification and CI.

New package versions require explicit review and lock-file updates. Do not guess versions.

## Consequences

- CI fails when project declarations and committed lock files disagree.
- Dependency upgrades are visible code-review events.
- This ADR covers NuGet resolution. GitHub Actions provenance and broader supply-chain hardening remain separate work.

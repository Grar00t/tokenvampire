# ADR 0003: Dependency pinning and locked restore

- Status: Accepted
- Scope: M0 foundation

## Context

Build and test results must not silently change because a dependency resolver or mutable CI action tag selected different code.

## Decision

For NuGet dependencies:

- Pin direct package versions centrally in `Directory.Packages.props`.
- Commit project lock files.
- Use `dotnet restore --locked-mode` in verification and every CI build job.
- New package versions require explicit review and lock-file updates. Do not guess versions.

For GitHub Actions:

- Use reviewed stable releases.
- Pin each action invocation to the exact commit SHA corresponding to the reviewed release.
- Keep the human-readable release version in a comment beside the SHA.

At the time of this ADR, CI pins:

- `actions/checkout` v7.0.1 -> `3d3c42e5aac5ba805825da76410c181273ba90b1`
- `actions/setup-dotnet` v6.0.0 -> `a98b56852c35b8e3190ac28c8c2271da59106c68`

## Consequences

- CI fails when project declarations and committed NuGet lock files disagree.
- Dependency and CI-action upgrades are visible code-review events.
- A pinned SHA reduces mutable-tag risk; it does not by itself establish the security of the pinned upstream code.

# ADR 0001: Local storage shape

- Status: Accepted
- Scope: M0 foundation

## Context

Personal mode is local-first and must not depend on the optional organization server. The repository already stores structured metadata in SQLite and keeps evidence payloads behind a local vault abstraction.

## Decision

Use SQLite for local structured indexes and case metadata. Keep evidence payload bytes outside relational rows behind a vault abstraction. Personal mode remains usable without a cloud account or organization server.

The current file-backed vault implementation is an early implementation detail, not a claim that the M1 encrypted content-addressed vault is complete.

## Consequences

- SQLite schema changes require explicit migrations before release.
- The organization server cannot become a dependency of Personal mode.
- Large-file streaming, crash-safe writes, key lifecycle, and content addressing remain M1 work and require their own verification.

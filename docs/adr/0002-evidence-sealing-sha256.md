# ADR 0002: SHA-256 evidence sealing boundary

- Status: Accepted
- Scope: M0 foundation

## Context

Evidence needs a deterministic integrity check without implying more provenance than the system can prove.

## Decision

Use SHA-256 digests to seal and later verify exact evidence bytes. Record file length and sealing timestamp alongside the digest.

A SHA-256 digest is an integrity fingerprint, not a digital signature. A matching digest establishes that the checked bytes match the sealed bytes; it does not establish author identity, legal authenticity, truth of the content, or chain of custody by itself.

## Consequences

- Reports must describe SHA-256 as a hash/digest/seal, not as a signature unless an actual signing mechanism is added.
- Identity attestation and external timestamp authorities, if ever required, are separate features and decisions.

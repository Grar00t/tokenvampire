# Assurance capability roadmap

This document records high-value capabilities without moving them into an earlier milestone than their dependencies allow. None of the items below is claimed as implemented merely because it is listed here.

## M1 — Personal evidence workflow

### Immutable claim ledger
Append-only, content-addressed records for assertions, invoice lines, usage claims, and delivery claims. Existing entries are never edited in place; later entries may supersede them with a reason and a reference to the prior entry.

### Consent-gated export packages
Human-readable and machine-readable evidence/findings/remedy bundles require explicit consent for the intended recipient before export. A revocation receipt records that consent was withdrawn for future use; it cannot erase copies already received by a third party.

### Discrepancy timeline and human gate
Show mismatches chronologically. Suggested remedy packages remain local and blocked until a human explicitly approves export or submission.

## M2 — Developer assurance

### Usage ↔ invoice ↔ artifact reconciliation
Match metered usage, billed charges, and observable artifacts such as files, hashes, timestamps, and model identifiers. Emit verified deltas and explicit unknowns. Never infer a loss total from unverified amounts.

### AI provenance seals
For claimed AI-generated work, retain the model identifier, prompt hash, output hash, generation timestamp, and available generation parameters. Verification answers whether the exact recorded artifact and metadata match the seal; it does not by itself prove authorship, semantic quality, or provider-side execution.

## M3 — Organization alpha

### Local-only offline sync queue
When an optional organization server is configured, queue encrypted evidence references and consent records for later synchronization. Personal mode remains fully usable without that server.

## Governing boundaries

- No evidence, no finding.
- No consent, no export.
- No verified amount, no loss total.
- No human approval, no submission.
- No feature in this roadmap automatically accuses a provider of fraud, illegality, or non-compliance.

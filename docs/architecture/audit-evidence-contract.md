# Evidence-led audit implementation (v1)

This patch adds conservative audit domain records. Model-level narration, a valid SHA-256, an invoice, and a marketing statement **do not** independently establish factual accuracy or statutory liability.

| Control | Data carrier | Verification boundary |
| --- | --- | --- |
| Delivery truth | `DeliveryRecord`, evidence seal | Independently inspect file, hash, timestamp and exact status; a record alone is not delivery proof |
| Semantic support | `CitationMapping` | Exact text comparison is not semantic entailment; `SemanticSupportVerified=null` until human review |
| Cost and rework | `AuditMetrics` | Incomplete capture or unmeasured amounts remain unknown |
| Editorial consistency | `EditorialEvent` | Prompt hash, truncation and observed drift stay independently assessed |
| User agency and privacy | `ConsentReceipt`, `ConsentGate` | Export requires recipient-specific active consent; no automatic submission |
| Challenge and remedy | `SupportEvent` | Recorded bot replies are not proof of refund obligation |

The local vault already uses AES-256-GCM with 600,000 PBKDF2-SHA256 iterations; this change does **not** certify its full key lifecycle, access controls, or regulatory compliance. The open vault-hardening PR remains separate.

## Mapping only, not a legal conclusion

- Saudi E-Commerce Law: service, delivery and payment disclosures, contract evidence and invoice documentation are represented as `DisclosureRecord`. Applicable articles/requirements must be confirmed by a legal reviewer against current official text before a compliance verdict.
- Saudi PDPL: purpose-specific consent provenance and local storage controls are recorded. Local encryption alone does not establish PDPL compliance.
- NIST AI RMF: `GovernanceMapping` records source reference and `Govern/Map/Measure/Manage` function (NIST AI 100-1, 2023). A mapping is not a conformity assessment.

## Determinism and migration

`Manifest.Root` v2 binds Id, FileName, Length, SHA-256, UTC SealedAt and DirectoryId using framed UTF-8 bytes. Root hashes are integrity checks, **not signatures**. Legacy manifest roots are incompatible and must be re-sealed with explicit review; no automatic migration silently upgrades them.

`DirectoryId` defaults to the logical identifier `unspecified` to preserve the existing `SealedItem` constructor and SQLite schema. A future per-vault durable directory ID requires a separately reviewed migration. Absolute user paths are never part of the commitment.

Local conversation/HAR token estimates use an explicitly documented character heuristic, not a vendor tokenizer. Results have `Provenance.Assumed`, never measured authority. A missing official meter is unknown, not zero. Plan limits are external inputs, not invented facts.

# Product Contract

## Purpose

Build a local-first evidence, billing, subscription, and AI-delivery assurance application. Preserve original evidence, distinguish verified facts from assertions and scenarios, reconcile charges with invoices and usage, verify claimed digital work through observable artifacts, and generate reviewable remedy packages.

## Primary users

Individuals first; developers, households, small businesses, organizations, reviewers, and opt-in researchers second.

## Non-goals

- Do not automatically accuse any party of theft, fraud, illegality, or non-compliance.
- Do not act as a lawyer, regulator, accredited certification body, bank, payment processor, or automatic complaint sender.
- Do not publish personal evidence or self-selected cases as representative market statistics.
- Do not require a cloud account or server for Personal mode.
- Do not train a proprietary model, create a public plugin marketplace, connect permanently to email or bank accounts, or build a centralized personal-evidence SaaS before V1.
- Do not treat a credit top-up as token consumption, a failed probe as a failed project, valid citation syntax as semantic support, or missing data as zero.

## Canonical states

- Evidence: Verified, ProviderReported, UserAsserted, Estimated, ScenarioOnly, Unknown.
- Charge: Authorized, AuthorizationUnclear, UnauthorizedSuspected, DuplicateSuspected, RenewalUnwanted, PriceChanged, BillingMismatch, Unknown.
- Outcome: Delivered, PartiallyDelivered, NotDelivered, Rejected, Failed, Cancelled, Unknown.
- Damage ledgers: DirectCharge, AttributableRework, ServiceValueGap, ConsequentialHarm.

## Case lifecycle

Draft -> EvidenceCollecting -> EvidenceSealed -> Classified -> UserReview -> ReadyToExport -> Submitted -> ProviderResponded -> Escalated | Resolved | ClosedInsufficientEvidence.

## Governing principle

No evidence, no finding. No consent, no export. No verified amount, no loss total. No human approval, no submission.

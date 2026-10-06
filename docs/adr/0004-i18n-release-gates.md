# ADR 0004: I18n release gates

- Status: Accepted
- Scope: M0 foundation

## Context

TokenVampire may render financial and evidentiary material where missing translations, bidi confusion, or silent fallback can change meaning.

## Decision

English (`en`) and Arabic (`ar`) are the only release-blocking locales for V1. Both catalogs must be complete for required report keys.

Other built-in locales may be added and tested, but they do not become release-blocking without a new reviewed decision.

Arabic remains RTL and must preserve the Unicode-security boundaries already established by the I18n layer.

## Consequences

- Release checks may fail for incomplete English or Arabic catalogs.
- Additional locales cannot silently expand the release acceptance contract.
- Locale support is not a claim that every UI surface is translated until those surfaces exist and are verified.

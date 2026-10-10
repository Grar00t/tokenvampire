# Notion Desktop / Notion AI — redacted August 2026 case

**TokenVampire case ID:** `notion-desktop-2026-08`  
**Observation window:** 24–26 August 2026  
**Scope:** One reporting subscriber's Windows environment and four reported Notion AI interactions  
**Publication class:** REDACTED SUMMARY ONLY  
**Default evidence state:** `UserAsserted` (not independently reproduced in this public repository)

This public case is a **derivative of the reporting user's redacted narrative**, not a publication of the restricted forensic working archive. It does not contain the reporting user's personal identifiers, account IDs, support ticket identifiers, original database, credential-like bytes, raw telemetry logs, proprietary binaries, or extracted application bundles. Hashes and original timestamps of withheld files have **not** been manufactured.

The separately maintained private source archive is **not cleared for public distribution**. Its current README reports that credential material existed in a tracked configuration file and that historical cleanup and credential rotation remained unresolved; its working tree is not equivalent to the minimal public-safe layout described in the earlier summary. No archive source files have been imported into TokenVampire. Inspecting that private archive does not make individual product claims independently reproducible from this public case.

## Four reported interactions

| Record | Date | Surface | Reporter-described outcome | Classification |
| --- | --- | --- | --- | --- |
| N-INC-01 | 2026-08-24 | Notion AI workspace | Safety assessments differed across sessions | UserAsserted / reported failure |
| N-INC-02 | 2026-08-25 | Docs Assistant, developers.notion.com | Retrieved/tool output was attributed to the user | UserAsserted / reported failure |
| N-INC-03 | 2026-08-25 | Notion AI inferred memory | Incorrect behavioral inference was represented as fact | UserAsserted / reported failure |
| N-INC-04 | 2026-08-26 | Notion AI site analysis | Obfuscated injection was reportedly handled correctly | UserAsserted / positive counterexample |

The fourth record is intentionally retained: an observed-success report cannot be dropped merely because this is an incident dossier. There are no original prompts and full response transcripts in this public case. Neither a consistent underlying origin/context defect nor a particular cause for all four interactions has been proven.

## Telemetry and dependency observations

The reporting user recorded a Splunk HEC endpoint at `http-inputs-notion.splunkcloud.com:443/services/collector/raw`, configuration containing a HEC credential and a Google geolocation API credential, a local queue named `splunk-log-queue.jsonl`, and a reported last successful upload at **20:27 local time on 2026-08-25**. This public case reproduces **no keys, tokens or raw queue entries**. A reporter-observed telemetry configuration is not, by itself, proof of unauthorized data transfer or legal non-compliance.

The reporter's extracted package metadata identified `@sentry/electron` 7.11.0, `electron-log` 4.4.8, `electron-updater` 6.8.9, `better-sqlite3` 13.0.1, `uuid` 8.3.2, `node-fetch`, `undici`, `websocket-driver`, `sockjs`, and an internal `@common/ai` integration. Dependency **presence is not evidence of runtime execution** or a particular upload, update, or AI-model behavior.

## OAuth storage — unresolved classification

As reported, a locally readable SQLite file named `notion.db` contains a `microsoft_oauth` table with account-linked rows. The reporting user described a repeated 43-character Base64 value decoding to approximately 32 bytes, with a leading `0x00` byte. **No original value is published.**

The observed shape does **not** establish whether this is ciphertext, key material, token material, a digest, or non-sensitive application metadata. The absence of familiar JWT or Microsoft token prefixes does not establish the class of an opaque value. Readable SQLite is not proof that sensitive values are unencrypted. Storage security, recovery/replay risk and whether an OS-protected credential store is used remain **UNKNOWN** pending column-level and code-path inspection.

Requested mitigation: identify the stored value and its lifecycle; if sensitive, protect it with Windows DPAPI/Credential Manager or a comparably justified design, document key handling and provide user-visible purge/revocation controls.

## Reporter-observed application metadata (not current-version claims)

| Item | Reported local observation |
| --- | --- |
| Desktop version | Notion 7.31.2 |
| Runtime | Electron 42.4.1 / Chromium 148.0.7778.265 |
| Executable | Notion.exe, 221.64 MB, Authenticode-signed by Notion Labs, Inc. |
| Certificate | Microsoft ID Verified CS EOC CA 03; reported validity 23–26 Aug 2026 |
| SQLite file | notion.db, ~20.2 MB, 49 tables / 11 indexes |
| Data footprint | ~2.17 GB in the reporting user's application-data folder |
| Startup | Reported Windows HKCU Run registration with open-at-login |

These are historical, individual-machine observations, not product-wide baselines.

## Seven requests documented by the reporting user

1. Distinguish tool/retrieved content from the user's own messages.
2. Avoid attributing intent without evidence; describe ignored content rather than accusing the user.
3. Provide a correction/appeal path for disputed attribution.
4. Improve cross-session consistency and appeal handling.
5. Expose inferred memory with correction and deletion controls.
6. Label behavioral inference as inference, not verified biographical fact.
7. Minimize unnecessary behavioral-context inclusion.

The two **user-requested alternative outcomes** are (a) subscription-fee refund plus written acknowledgement/apology, or (b) full-access upgrade for the remainder of the term without extra charge. They are *requests*, not granted remedies, verified refund entitlements or legal findings. Prior private support references are not reproduced.

The cited architectural paper on unified-context deficits and deterministic enforcement is a hypothesis/framework for analysis, not proof of a single product-specific cause. Where relevant, review of source-versus-instruction separation should use bounded experiments and include the positive counterexample.

## Safe inspection procedure (when authorized local files are present)

These commands intentionally expose metadata only. They are not instructions to publish any secrets or user content:

```powershell
# Inspect names of configuration properties without printing their values
$cfg = Get-Content "$env:APPDATA\Notion\state.json" | ConvertFrom-Json
$cfg.PSObject.Properties.Name

# Check queue existence, not contents
Test-Path "$env:APPDATA\Notion\splunk-log-queue.jsonl"

# Check auto-start registry metadata
Get-ItemProperty "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" |
    Select-Object *notion*

# Inspect Authenticode status
Get-AuthenticodeSignature "$env:LOCALAPPDATA\Programs\Notion\Notion.exe"
```

```sql
-- Inspect schema and row count, not values or account identifiers.
PRAGMA table_info(microsoft_oauth);
SELECT COUNT(*) FROM microsoft_oauth;
```

**Reproduction limitation:** these commands reproduce inspection methods, not the historical result; files, software versions, accounts and permissions can differ.

## Deterministic local case validation

Run from the TokenVampire repository root, once the code in this change is built:

```sh
dotnet run --project src/TokenVampire.Cli -- audit-case --input cases/notion-desktop-2026-08/dossier.json
```

The CLI validates the schema, cross-references, chronological bounds, duplicate identifiers and provenance. It prints an input SHA-256 **for reproducibility of the redacted JSON only**; the digest does not validate the account's allegations or the hidden source archive. It does not connect to Notion or transfer case data.

**The parser intentionally refuses `Verified` and `SourceObserved` claims from this redacted JSON alone.** Original source logs, independent verification, and human review belong in a separate, consented evidence workflow.

## Audit boundary

- Confirmed in this public case: the **existence of a redacted user report** and consistency of its validated structured representation.
- Not established: unauthorized telemetry, specific OAuth credential exposure, a systemic origin/context defect, regulatory non-compliance, or refund entitlement.
- Access: private source artifacts remain private and are not copied or linked as downloadable files.
- Publication: this is a user-supplied allegation/observation register with a positive counterexample, not an adjudication or representative failure-rate sample.

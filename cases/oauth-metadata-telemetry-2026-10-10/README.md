# OAuth / metadata-service / telemetry allegation — redacted case

**Case:** `oauth-metadata-telemetry-2026-10-10`  
**Report received:** 10 October 2026  
**Affected provider:** UNATTRIBUTED (no verified host, service, account or owner)  
**Publication status:** Redacted summary; **not** a validated breach report.

This case preserves a user report of a credential-shaped string, an alleged
server-side request forgery (SSRF) against a metadata service, a reported local
event count greater than 87,000, and a mathematical claim about attention decay
during RAG saturation. The original token **was deliberately not imported** to
this public repository, its issues, commit history, attachments or CI. This
repository does not preserve the raw credential, its suffix, a digest of that
credential, any signed request, or a usable reproduction recipe.

The reporter's prior conversation mentioned Perplexity while discussing another
security header. That alone cannot attribute the separate OAuth/SSRF claims to
Perplexity, Google, AWS, Notion, or any other service. The `ya29`-style prefix
resembles Google OAuth access-token text; it does **not** prove token validity,
administrative scope, where the token was obtained or who owned it.

## Reported claims and decision states

| ID | What was reported | Audit classification | What is missing |
| --- | --- | --- | --- |
| U-OAUTH-01 | OAuth-shaped bearer string appeared in a user-submitted report | UserAsserted; redacted in Git | Issuer, service, token lifecycle, authorized review |
| U-OAUTH-02 | The token was active and administrative and was obtained by SSRF targeting metadata service | UserAsserted | Dated, authorized and redacted HTTP trace, request origin, independent server audit |
| U-OAUTH-03 | Administrative access or live compromise was achieved | **Unknown** | Owner-verified principal, scope and access logs |
| U-TEL-01 | More than 87,000 application events appear in `state.json` | UserAsserted | The file, schema, raw count procedure, event types and timestamp range |
| U-TEL-02 | These events prove eBPF-backed deterministic data exfiltration | UserAsserted as a claim; **not a verified finding** | eBPF attach point/program IDs, event records, authorized network capture and egress evidence |
| U-TEL-03 | External data exfiltration actually happened | **Unknown** | Observable outbound flow, destination, data class, consent and correlation |
| U-RAG-01 | A ratio of exponentiated scores tends to zero with increasing context length | UserAsserted hypothesis | Definitions, scoring assumptions, proof or controlled evaluation |
| U-RAG-02 | The proposed expression proves actual RAG attention decay | **Unknown** | Controlled model/score traces, experimental methods and metrics |
| U-ATTR-01 | Particular third-party vendor caused the event | **Unknown** | Origin hostname, request/session ownership and provider records |

The published machine-readable source classification remains
`reporterSuppliedSummary`. The `Verified` and `SourceObserved` states are
rejected by the public dossier validator because its source inputs have not
been independently inspected and sealed.

## Security handling

Treat a credential pasted into a conversation as **potentially exposed**, even
if its actual validity is unknown. The authorized owner or provider should
consider session revocation/rotation and investigate access logs using its
official security process. Do not replay, redeem, introspect or test the string
against any service from this report. Do not copy it into a public issue, chat,
shell command, CI variable, or benchmark fixture.

A token prefix is a format clue, not proof of administrative privilege. A
metadata-service SSRF claim requires an observed boundary crossing in a
permitted test or independent provider evidence. **No live exploitation or
credential verification was conducted for this public case.**

## The 87,000-plus claim is not an exfiltration measurement

A local `state.json` file can contain configuration, counters, cached state,
queued messages or other data. Without the file and field semantics, the
reported count is **unmeasured by TokenVampire**. It is neither accepted as an
eBPF event count nor as an outbound transfer count.

To verify an eBPF-based claim one would need authorized capture metadata such
as the program identity, attach point, event schema, monotonic/wall timestamps,
event source/PID and a correlation with network egress. To conclude exfiltration,
an auditor also needs an independently verified recipient and data flow outside
its authorized boundary. Event counts alone cannot prove any of those.

## RAG saturation claim — mathematical boundary

The reporter provided a softmax-like expression with a claimed zero limit as
the context length approaches infinity. Its symbols and summation boundaries
are not fully defined. Even for a normalized ratio

```text
    A(L) / ( A(L) + B(L) ),  A(L),B(L) > 0
```

the ratio tends to zero **only under additional assumptions**, for example
`B(L)/A(L)` tending to infinity. Increasing context length by itself does
not establish such a relationship; nor does a hypothetical ratio prove
observed failure of a particular model.

A valid empirical study needs frozen question and evidence sets, declared
retrieval/position methods, a fixed evaluation protocol, context-length
variants, confidence intervals and independently checked answer correctness.
Until then the theoretical and operational claims are **UNKNOWN**.

## Operator: local deterministic validation

After checking out the stacked case branch (it depends on PR #34's common
dossier parser), run:

```sh
dotnet run --project src/TokenVampire.Cli -- audit-case --input cases/oauth-metadata-telemetry-2026-10-10/dossier.json

dotnet test -c Release
```

The CLI checks references, chronology, provenance and redaction constraints.
Its SHA-256 output covers **the redacted JSON bytes only** and does not
authenticate the submitted narrative or unseen source captures.

Do not upload raw network traces, eBPF buffers, `state.json` contents, OAuth
headers or account identifiers into this public repository. Any future private
evidence-ingestion path must have explicit authorization and a separate
redaction/consent review.

**Case verdict:** No established unauthorized access, exfiltration, SSRF
exploit chain, administrative token scope, product attribution, or RAG
causality from this submitted material alone.

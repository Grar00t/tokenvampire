# Reasoning / thinking token billing audit

**Date:** 2026-10-10. **Scope:** vendor-neutral, local, no external API calls.

A user-submitted social post alleges that visible "Show Thinking" output is performative text and that every reasoning token is billed by every provider. The post supplies **no model-specific usage export, rate contract, invoice, experiment, or provider intent evidence**. Its qualitative evaluation and financial-incentive theory are **UserAsserted / Unverified**, not facts established by TokenVampire.

Provider API documentation does support a narrower, testable concern: on some API products, **billable reasoning tokens can differ from the thinking text visible to a user**. This does not establish that the reasoning is useless, that all providers use the same pricing rule, or that a consumer subscription's UI "Show Thinking" control changes a person's invoice.

## Official documentation (consult again for your exact model and billing date)

- OpenAI Chat Completions usage: `completion_tokens_details.reasoning_tokens` is a breakdown of completion output tokens; do **not** add that breakdown to `completion_tokens` a second time. https://platform.openai.com/docs/api-reference/chat
- Google Gemini API thinking (Generate Content): `usageMetadata.thoughtsTokenCount` and `usageMetadata.candidatesTokenCount`; its thinking documentation describes pricing as the sum of visible output and thinking tokens for supported models. https://ai.google.dev/gemini-api/docs/generate-content/thinking
- Anthropic Claude Thinking: the billed `output_tokens` count includes reasoning tokens and a reasoning breakdown may be provided; visible/summarized thinking is not the billable total. https://platform.claude.com/docs/en/build-with-claude/thinking-steering-and-cost

The *accounting convention* must be selected for each actual API endpoint, model and observation date using applicable documentation. Do not infer it from the vendor name alone. No rate table is hardcoded, and Perplexity has no invented default here.

## Run locally

```sh
dotnet run --project src/TokenVampire.Cli -- reasoning-audit --input examples/reasoning-billing-scenario.json
dotnet test -c Release
```

The example uses **fictional, assumed** counters and a fictional output rate (USD 10 per million). Its output is `ScenarioOnly`, not an actual API charge. The JSON audit output contains the SHA-256 of the validated **local scenario JSON**, not of an invoice or model usage export.

## JSON input schema (version 1)

```json
{
  "schemaVersion": 1,
  "provider": "example",
  "model": "synthetic-model",
  "currency": "USD",
  "accountingMode": "inclusiveOutput",
  "reportedOutputTokens": { "value": 300, "provenance": "assumed" },
  "reasoningTokens": { "value": 200, "provenance": "assumed" },
  "outputRatePerMillion": { "value": 10.00, "provenance": "assumed" }
}
```

Use `"accountingMode": "inclusiveOutput"` if reported output already **includes** reasoning; `"additiveOutput"` if the documented visible output counter **excludes** reasoning; or `"unknown"` if unestablished. Choosing incorrectly will invalidate the estimate. The provenance of each quantity can be `"measured"`, `"assumed"`, or `"unknown"`. Unknown values **omit** `value`; never fill gaps with zero. "Measured" is a caller claim, not an independent certificate. A known zero must use `"value": 0`.

When reasoning tokens are an **inclusive subset**, the reported output count is used as-is. When **additive**, the two measured/assumed counts are combined once. `reasoningPortionAlreadyIncluded` is a portion of `estimatedOutputCost` under the supplied accounting convention; **never add them together**.

The numeric estimate is based on declared per-million output rates and excludes input tokens, discounts, caching, tools, other usage, taxes, credits, invoices, exchange rates and contract-specific adjustments. No real billed charge, incremental charge caused by enabling thinking, or performance value can be established from this file. An actual cost attribution or before/after comparison needs provider usage exports, itemized invoices, applicable price terms, and matched model/effort/prompt/evaluation settings.

The command refuses malformed/duplicate JSON properties, raw prompt/request fields, negative values, opaque unrecognized fields, overlarge inputs and unsafe provider/model metadata. It reads only the chosen local JSON file and writes a JSON report to stdout; it neither sends requests nor changes subscriptions.

## Example without double counting

Synthetic `inclusiveOutput`: 300 total output tokens, of which 200 are reasoning, rate USD 10/M. Total output-rate estimate **USD 0.003**, reasoning portion **USD 0.002 included within that amount**. Incorrectly computing `(300 + 200) * rate` would overcount. In contrast, an `additiveOutput` field of 100 visible tokens plus 200 separate reasoning tokens yields a counted output total of 300.

## Claim limitations

The input text's statement that thinking is merely a "loading screen" is not a measured performance conclusion; generative models may produce internal reasoning tokens without showing raw traces. Costs, latency, and answer quality must be evaluated separately. No evidence here establishes intent to increase charges, financial wrongdoing, or that usage spikes necessarily result from reasoning mode.

# Contributing

## Workflow

1. Start from the current default branch.
2. Use one scoped issue, one branch, and one pull request.
3. Inspect existing code, ADRs, tests, issues, and pull requests before writing.
4. Implement code, tests, documentation, and migrations required by the issue.
5. Run restore, formatting verification, build, relevant tests, and secret scanning.
6. Record exact commands, results, security/privacy impact, i18n impact, and unresolved unknowns in the pull request.

## Non-negotiable rules

- Missing evidence remains Unknown and never silently becomes zero.
- Personal mode remains usable without a TokenVampire server.
- No LLM is the sole judge of a material finding.
- Arabic, English, Unicode safety, RTL, accessibility, and privacy are feature requirements.
- Never log or commit private evidence or secrets.
- Never select or change a license without a recorded human decision.

See `docs/operations/copilot-execution-contract.md` for the complete execution contract.

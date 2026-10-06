# Security Policy

## Reporting

Do not open a public issue for a suspected vulnerability or exposed secret. Use GitHub private vulnerability reporting when enabled. Until a dedicated security address is published, contact the repository owner privately through GitHub.

## Scope

Security-sensitive areas include evidence integrity, encryption and key handling, hostile-file parsing, export consent, Unicode spoofing, billing calculations, tenant isolation, authentication, updates, and the software supply chain.

## Handling rules

- Never include raw evidence, credentials, payment-card data, private messages, encryption keys, or decrypted vault data in an issue, pull request, log, screenshot, or test fixture.
- Use synthetic fixtures only.
- Do not claim a vulnerability is fixed without an executable regression test where feasible.
- Do not weaken tests, scanning, or authorization to make a build pass.

No production release exists yet; supported-version information will be added before the first release.

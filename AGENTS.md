# DisCatSharp repository instructions

These are the canonical repository-wide instructions for coding agents. Platform adapters must point here instead of maintaining competing copies. Load `skills/maintain-discatsharp/SKILL.md` for repository changes and `skills/use-discatsharp/SKILL.md` for consumer guidance.

## Sources of truth

- Work from the live checkout and current supplied upstream documentation.
- The public MCP at `https://docs.dcs.aitsys.dev/mcp` is authoritative for the current generated DisCatSharp and official DisCatSharp.Extensions documentation corpora.
- `discatsharp-ai.xml` is a generated Repomix snapshot for offline context and evaluation. It may be stale and must never override live files, installed packages, or current documentation.
- Use Discord's official documentation for underlying Discord payload, permission, Gateway, REST, and rate-limit contracts.

## Worktree safety

- Confirm the checkout and branch before editing.
- Preserve all existing tracked and untracked user changes. Never revert unrelated work.
- Re-read files immediately before editing when work may be concurrent.
- Inspect untracked files separately because `git diff --stat` omits them.
- Never publish, merge, enable repository settings, or mutate infrastructure without explicit authorization.

## Architecture and style

- The core library is in `DisCatSharp/`; feature packages use sibling `DisCatSharp.*` directories.
- Shared build targets and supported frameworks are in `DisCatSharp.Targets/`; repository tooling is in `DisCatSharp.Tools/`.
- Follow `CONTRIBUTING.md`, Microsoft C# conventions, existing `this.` usage, and file-scoped namespaces. Use current stable C# features when they improve clarity without obscuring behavior.
- Use `Optional<T>` and the existing JSON conventions for optional REST payload fields, including conditional serialization where needed.
- Logging flows through `Microsoft.Extensions.Logging`; missing-field diagnostics use the existing telemetry abstractions rather than direct Sentry coupling.
- Preserve public compatibility. Discuss breaking changes and use the existing `Deprecated` migration path where appropriate.
- Aim for complete XML documentation across the C# codebase. Every declaration is in scope regardless of visibility, including internal and private types, members, and fields. New and materially changed code must not add undocumented declarations; use meaningful documentation rather than placeholder text and improve adjacent gaps when practical.

## Connected-path review

For new or changed public data, inspect every applicable path: parsing/type mapping, validation/defaults, serialization, manual copies, localization reconstruction, cloning, equality, synchronization, cache updates, tests, analyzers, and docs. Do not validate only the requested line.

Gateway event models should faithfully expose the raw payload. Do not introduce REST enrichment when the event already carries the authoritative identity or state. Keep REST rate-limit bucket ordering separate from Gateway handler dispatch behavior.

## Validation

- Run the smallest relevant test project after a meaningful change.
- Validate all targeted frameworks (`net9.0`, `net10.0`, and `net11.0`) when applicable.
- Put agent-authored bug regression and diagnostic scenarios in `DisCatSharp.Tests/DisCatSharp.CopilotTests/`; put ordinary feature tests in the owning test project.
- Gateway tests that immediately inspect callbacks must select `SequentialHandlers` or explicitly wait for concurrent handlers.
- Build affected packages for public/API-shape changes.
- Finish with `git diff --check`, `git status --short`, and a focused diff review.

## Documentation and releases

- Run `docfx DisCatSharp.Docs/docfx.json`; do not manually edit generated API navigation.
- Keep DocFX's manifest authoritative for conceptual search coverage.
- `RELEASENOTES.md` is the cumulative active release-cycle note file. `CHANGELOG.md` is the stable release index; detailed history lives in DocFX changelogs.
- Public package/release publication happens only through the manually dispatched `Release DisCatSharp` workflow.

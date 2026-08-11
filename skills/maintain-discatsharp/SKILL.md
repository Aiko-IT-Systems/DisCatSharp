---
name: maintain-discatsharp
description: Review, modify, test, document, or release the DisCatSharp repository. Use for DisCatSharp source changes, API additions and migrations, Discord payload models, serialization, command synchronization, Gateway dispatch, REST behavior, analyzer work, multi-target testing, DocFX generation, release notes, and repository pull requests.
license: MIT
---

# Maintain DisCatSharp

Make evidence-backed changes against the live checkout. Preserve user work, trace every connected behavior that carries a public value, and validate the smallest relevant surface across the supported frameworks.

## Start safely

1. Read the repository's root `AGENTS.md` and `CONTRIBUTING.md`.
2. Confirm the exact checkout, branch, target issue or PR, and working-tree state.
3. Treat existing tracked and untracked changes as user-owned. Never discard, rewrite, or include unrelated work.
4. Re-read files immediately before patching when another person or agent may be editing concurrently.
5. Use live files and current supplied upstream documentation as authoritative. `discatsharp-ai.xml` is a read-only Repomix snapshot and may be stale.

Read [references/repository-map.md](references/repository-map.md) before changing an unfamiliar subsystem.

## Trace the behavior

Do not review a public member or payload field in isolation. Depending on the feature, inspect:

- Input parsing and type mapping.
- Constructors, property setters, validation, and default values.
- JSON serialization/deserialization and `Optional<T>` behavior.
- Manual copies, localization reconstruction, cloning, and builders.
- Equality, synchronization, overwrite, and cache-update decisions.
- REST request models and Gateway payload/event models.
- XML documentation for public, internal, and private symbols, plus conceptual guidance, analyzers, and migrations.
- Focused regression tests for the failure mode.

Mirror Discord's actual contract. Do not add speculative REST enrichment to a Gateway event when its payload already carries the authoritative data.

## Implement consistently

- Follow the repository's C# style, including file-scoped namespaces and existing `this.` usage.
- Preserve compatibility. Discuss breaking changes and prefer the repository's `Deprecated` migration path for public API removal.
- Use exact current upstream documentation when Discord behavior is revision-sensitive.
- Maintain the repository's 100% documentation goal. Every declaration is in scope regardless of visibility, including internal and private types, members, and fields. New and materially changed code must not add undocumented declarations; explain state invariants and ownership rather than restating names, and improve adjacent gaps when practical.
- Keep REST bucket ordering distinct from Gateway dispatch ordering.
- Avoid broad redesign during a focused fix unless a connected correctness path requires it.
- Use `apply_patch` or an equally reviewable edit path and inspect the resulting diff.

## Validate proportionally

Read [references/validation-matrix.md](references/validation-matrix.md), then:

1. Run the smallest relevant test project or filtered regression tests.
2. Cover `net9.0`, `net10.0`, and `net11.0` when the project targets all three.
3. Build the affected package or solution when public/API shape changes.
4. Run `git diff --check` and inspect untracked files separately from the diff stat.
5. For Gateway tests that immediately inspect callbacks, select `SequentialHandlers` or explicitly wait for concurrent handlers to complete.
6. Run broader tests only when the touched dependency surface justifies them.

## Documentation and release work

- Generate documentation with `docfx DisCatSharp.Docs/docfx.json`; do not manually maintain generated API navigation.
- Keep conceptual content discoverable through DocFX rather than hardcoded search-index folder lists.
- Update XML documentation across every affected visibility level and add migration guidance for behavioral or breaking changes.
- Keep `RELEASENOTES.md` as the active cumulative release-cycle notes.
- Treat `CHANGELOG.md` as the stable release index and DocFX changelog pages as the detailed historical record.
- Public releases are created only through the manually dispatched `Release DisCatSharp` workflow.

## Hand off cleanly

- State what changed, what was verified, and any remaining risk.
- Do not claim a timed-out command failed without checking the process or its output.
- Follow the repository's pull-request template and requested draft/merge boundary.
- Never merge, publish a package, enable repository policy, or mutate production infrastructure unless the user explicitly authorizes it.

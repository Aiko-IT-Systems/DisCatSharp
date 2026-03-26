---
uid: vs_analyzer_authoring
title: Visual Studio Tools
author: DisCatSharp Team
---

# DisCatSharp Analyzer and Code Fix Authoring

This page documents the current authoring model for `DisCatSharp.Analyzer` and `DisCatSharp.Analyzer.CodeFixes`.

## Project layout

- `DisCatSharp.Tools\DisCatSharp.Analyzer\DisCatSharp.Analyzer`
  - owns diagnostics, descriptors, and analyzer logic
- `DisCatSharp.Tools\DisCatSharp.Analyzer\DisCatSharp.Analyzer.CodeFixes`
  - owns Roslyn code fixes and reusable rewrite helpers
- `DisCatSharp.Tools\DisCatSharp.Analyzer\DisCatSharp.Analyzer.Package`
  - packs both assemblies into the NuGet analyzer package
- `DisCatSharp.Tests\DisCatSharp.Analyzer.Tests`
  - regression tests for diagnostics and code fixes

## Current shared infrastructure

The analyzer and code-fix projects now share a small foundation:

- `DisCatSharpDiagnosticIds`
  - centralized diagnostic IDs
- `DisCatSharpDiagnosticProperties`
  - shared property keys for analyzer-to-code-fix metadata
- `SingleDiagnosticCodeFixProvider`
  - base class for one-diagnostic fixers
- `CodeFixSemanticHelpers`
  - semantic helpers for creation/type matching
- `CodeFixSyntaxHelpers`
  - reusable initializer/property rewrite helpers

This is the preferred starting point for future rule-specific fixes.

## Adding a new diagnostic

1. Add a diagnostic ID to `DisCatSharpDiagnosticIds`.
2. Add or reuse resource strings in `Resources.resx`.
3. Add the descriptor in `DisCatSharpAnalyzer`.
4. Register the analyzer logic for the relevant symbol or syntax shape.
5. If the code fix needs extra analyzer metadata, add a shared property name to `DisCatSharpDiagnosticProperties`.
6. Add or update docs under `DisCatSharp.Docs\vs\analyzer\`.
7. Add regression coverage in `DisCatSharp.Tests\DisCatSharp.Analyzer.Tests`.

## Adding a new code fix

For one diagnostic, inherit from `SingleDiagnosticCodeFixProvider`.

Typical pattern:

1. Read any analyzer-supplied metadata from `Diagnostic.Properties`.
2. Use semantic helpers to identify the right nodes or symbols.
3. Use syntax helpers to rewrite initializers, assignments, or other syntax shapes.
4. Keep transformations deterministic and idempotent.
5. Cover both the analyzer diagnostic and the code-fix result with tests.

## Migration fixer guidance

Future breaking-change fixers should start with rule-specific logic, but use the shared helpers wherever possible.

Recommended first-class migration categories:

- symbol rename
- namespace move and using rewrite
- object initializer/property injection
- invocation replacement
- signature reshaping when arguments or call shape change

If multiple fixers need the same metadata model, promote that pattern into shared infrastructure after the second real use case rather than inventing a broad manifest up front.

## Validation commands

Use these commands before merging analyzer/tooling changes:

```powershell
dotnet test .\DisCatSharp.Tests\DisCatSharp.Analyzer.Tests\DisCatSharp.Analyzer.Tests.csproj -c Debug --nologo
dotnet build .\DisCatSharp.Tools\DisCatSharp.Tools.slnx -c Debug --nologo
```

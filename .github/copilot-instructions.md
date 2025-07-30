---
applyTo: '**'
---
# DisCatSharp Copilot Instructions

## Project Architecture
- DisCatSharp is a modular .NET library for Discord bots, split into core (`/DisCatSharp`) and feature packages (e.g., `/DisCatSharp.ApplicationCommands`, `/DisCatSharp.CommandsNext`, `/DisCatSharp.Lavalink`, `/DisCatSharp.VoiceNext`).
- Each major feature (commands, interactivity, voice, configuration, etc.) is in its own folder/package. Shared types/utilities are in `/DisCatSharp.Common`.
- REST API payloads and client logic are in `/DisCatSharp/Net/`.
- Hosting and DI integration are in `/DisCatSharp.Hosting` and `/DisCatSharp.Hosting.DependencyInjection`.
- Analyzer and tooling live in `/DisCatSharp.Tools` and `/DisCatSharp.Analyzer`.
- Documentation is generated with DocFX from `/DisCatSharp.Docs`.

## Developer Workflows
- Build: Use `dotnet build` on the solution or individual projects. Solution files: `DisCatSharp.slnx`, `DisCatSharp.Tools.slnx`.
- Test: Use `dotnet test` on test projects in `/DisCatSharp.Tests` (xUnit).
- Analyzer: Custom Roslyn analyzers are in `/DisCatSharp.Analyzer` and can be packaged as NuGet or VSIX.
- Docs: Run DocFX using `docfx docfx.json` in `/DisCatSharp.Docs`.
- PowerShell scripts for rebuilds and packaging are in `/DisCatSharp.Tools`.

## Coding Conventions
- Follows Microsoft C# conventions with project-specific rules:
  - Use `Optional<T>` for optional fields/properties in REST payloads.
  - Use `NullValueHandling = NullValueHandling.Ignore` for nullable REST properties.
  - Add `ShouldSerialize[Property]()` for conditional serialization if needed. Especially useful for properties that should not be serialized when null.
  - Use `JsonIgnore` for properties that should not be serialized at all.
  - Use PascalCase for public APIs, camelCase for locals/parameters.
  - Prefer initializer syntax for object construction.
  - Use XML doc comments for all public APIs.
  - Avoid breaking changes; use `[Deprecated]` attribute for deprecations.
  - We use file scoped namespaces for all files.
  - Namespaces does not neccessarily match folder structure, but should be consistent with the project structure.
- Async methods should use `async`/`await` unless returning a single task directly.
- All code changes must build and pass CI before merge.

## Integration & Patterns
- Logging uses `Microsoft.Extensions.Logging.ILogger` (can swap providers).
- DI/Hosting: Use extension methods in `/DisCatSharp.Hosting` for ASP.NET Core/Generic Host integration.
- REST API: All REST calls go through `DiscordApiClient` in `/DisCatSharp/Net/Rest/`.
- Sentry is used for error reporting; see `/DisCatSharp/Net/Serialization/DiscordJson.cs` for integration details.
- Configuration classes: `DiscordConfiguration`, `CommandsNextConfiguration`, etc. (see `/DisCatSharp/Configuration/Models/`).

## Examples & Templates
- Example projects and templates are referenced in `/DisCatSharp.Docs` and the [DisCatSharp.Examples](https://github.com/Aiko-IT-Systems/DisCatSharp.Examples) repo.
- Project templates are available for bots, web, and combined solutions.

## Key Files & Directories
- `/DisCatSharp/Net/Rest/DiscordApiClient.cs`: Core REST client logic.
- `/DisCatSharp/Entities/`: Discord entity models.
- `/DisCatSharp.Tests/`: Test projects for all major features.
- `/DisCatSharp.Docs/`: DocFX documentation source.
- `/DisCatSharp.Tools/`: Build, packaging, and helper scripts.
- `/DisCatSharp.Analyzer/`: Custom Roslyn analyzers and code fixes.

## Versioning
- Uses custom SemVer: `{DISCORD_API_VERSION}.{MAJOR}.{MINOR}-{PATCH}`. Major changes require discussion and deprecation attributes.

## Test Placement Guidelines

- If you are generating or updating tests specifically to help diagnose, reproduce, or fix bugs (e.g., Copilot-generated regression or scenario tests), place them in `/DisCatSharp.Tests/DisCatSharp.CopilotTests/` and update its `.csproj` file.
- For all other standard or feature tests, use the appropriate subfolder in `/DisCatSharp.Tests/` according to the feature or package being tested.

---

For more, see `/CONTRIBUTING.md`, `/README.md`, and `/DisCatSharp.Docs`.

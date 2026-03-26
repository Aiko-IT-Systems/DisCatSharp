---
uid: vs
title: Visual Studio Tools
author: DisCatSharp Team
---

# Visual Studio Tools

This section contains information on how to use the Visual Studio tools for developing bots with DisCatSharp.

## Analyzers & Code Fixes

DisCatSharp can be extended with a set of analyzers that can help you write better code. These analyzers will warn you about common mistakes and bad practices.

### DisCatSharp Analyzer

#### Installation

To use the DisCatSharp Analyzer, you need to install the [DisCatSharp.Analyzer](https://www.nuget.org/packages/DisCatSharp.Analyzer) NuGet package.

The NuGet package is the supported delivery model for analyzers and code fixes.

If you maintain the analyzer/tooling stack, see the [authoring guide](xref:vs_analyzer_authoring).

##### Visual Studio Code

Enable `omnisharp.enableRoslynAnalyzers` in your settings.

##### JetBrains Rider

See https://www.jetbrains.com/help/rider/Settings_Roslyn_Analyzers.html

#### Included Rules

#### Diagnostic Families

The analyzer currently contains a mix of legacy shipped IDs and newer family-based ranges.

Use this as the quick orientation guide.

##### Legacy shipped diagnostics

These rules keep their historical IDs for compatibility.

| Rule ID                              | Category | Severity | Notes                                  |
| ------------------------------------ | -------- | -------- | -------------------------------------- |
| [DCS0001](xref:vs_analyzer_dcs_0001) | Usage    | Info     | Experimental Attribute Analyzer        |
| [DCS0002](xref:vs_analyzer_dcs_0002) | Usage    | Error    | Deprecated Attribute Analyzer          |
| [DCS0101](xref:vs_analyzer_dcs_0101) | Usage    | Warning  | DiscordInExperiment Attribute Analyzer |
| [DCS0102](xref:vs_analyzer_dcs_0102) | Usage    | Error    | DiscordDeprecated Attribute Analyzer   |
| [DCS0103](xref:vs_analyzer_dcs_0103) | Usage    | Warning  | DiscordUnreleased Attribute Analyzer   |
| [DCS0200](xref:vs_analyzer_dcs_0200) | Usage    | Info     | RequiresFeature Attribute Analyzer     |
| [DCS0201](xref:vs_analyzer_dcs_0201) | Usage    | Warning  | RequiresOverride Attribute Analyzer    |

##### Reserved families

| Family | Purpose |
| ------ | ------- |
| `DCS1XXX` | Core `DisCatSharp` diagnostics, migrations, and cross-cutting code fixes |
| `DCS2XXX` | `DisCatSharp.ApplicationCommands` diagnostics and code-fix families |
| `DCS3XXX` | `DisCatSharp.CommandsNext` diagnostics and code-fix families |
| `DCS4XXX` | `DisCatSharp.Interactivity` diagnostics and code-fix families |
| `DCS5XXX` | `DisCatSharp.Voice` diagnostics and code-fix families |
| `DCS6XXX` | `DisCatSharp.Lavalink` diagnostics and code-fix families |
| `DCS7XXX` | `DisCatSharp.Common` diagnostics and code-fix families |
| `DCS8XXX` | Hosting, dependency injection, and configuration diagnostics and code-fix families |
| `DCS9XXX` | Reserved |

##### Application command family

| Rule ID                              | Category | Severity | Notes                                            |
| ------------------------------------ | -------- | -------- | ------------------------------------------------ |
| [DCS2101](xref:vs_analyzer_dcs_2101) | Usage    | Info     | Application command checks-failed migration prototype |

##### Core family

| Rule ID                              | Category | Severity | Notes                                      |
| ------------------------------------ | -------- | -------- | ------------------------------------------ |
| [DCS1101](xref:vs_analyzer_dcs_1101) | Usage    | Info     | Prefer `DiscordClient.GetPresences(userId)` over manual `Presences` filtering |

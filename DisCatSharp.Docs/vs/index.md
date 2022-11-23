---
uid: vs
title: Visual Studio Tools
---

## Visual Studio Tools

This section contains information on how to use the Visual Studio tools for developing bots with DisCatSharp.

### Analyzers & Code Fixes

DisCatSharp can be extended with a set of analyzers that can help you write better code. These analyzers will warn you about common mistakes and bad practices.

#### DisCatSharp Analyzer

#### Installation

To use the DisCatSharp Analyzer, you need to install the [DisCatSharp.Analyzer.Roselyn](https://www.nuget.org/packages/DisCatSharp.Analyzer.Roselyn) NuGet package.

##### Included Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
[DCS0001](xref:vs_analyzer_dcs_0001) | Information | Warning | Experimental Attribute Analyzer
[DCS0101](xref:vs_analyzer_dcs_0101) | Information | Warning | DiscordInExperiment Attribute Analyzer
[DCS0102](xref:vs_analyzer_dcs_0102) | Information | Warning | DiscordDeprecated Attribute Analyzer
[DCS0103](xref:vs_analyzer_dcs_0103) | Information | Warning | DiscordUnreleased Attribute Analyzer

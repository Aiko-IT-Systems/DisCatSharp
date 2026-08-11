[![Build](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/build.yml/badge.svg)](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/build.yml) [![Documentation](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/documentation.yml/badge.svg)](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/documentation.yml) [![CodeQL](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/codeql-analysis.yml)

[![GitHub last commit](https://img.shields.io/github/last-commit/Aiko-IT-Systems/DisCatSharp?label=Last%20Commit&style=flat-square&logo=github)](https://aitsys.dev/source/DisCatSharp/history/) [![GitHub commit activity](https://img.shields.io/github/commit-activity/w/Aiko-IT-Systems/DisCatSharp?label=Commit%20Activity&style=flat-square&logo=github)](https://github.com/Aiko-IT-Systems/DisCatSharp/commits/main)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/Aiko-IT-Systems/DisCatSharp?label=PRs&style=flat-square&logo=github)](https://github.com/Aiko-IT-Systems/DisCatSharp/pulls) ![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Aiko-IT-Systems/DisCatSharp?label=Size&style=flat-square&logo=github)

![LTS](https://img.shields.io/nuget/v/DisCatSharp?color=1F8B4C&label=LTS&style=flat-square&logo=Nuget) ![Latest](https://img.shields.io/nuget/vpre/DisCatSharp?color=AD1457&label=Latest&style=flat-square&logo=Nuget)

----

# DisCatSharp

## A Discord app library written in C# for .NET

----

## Why DisCatSharp?

If you:

- want a library where you get kind and efficient help
- would like to have and use the most recent features of the Discord API
- are ready to build great things

Then this is the right place for you!

## History

We squashed DisCatSharp's Git history because of its clone size and accumulated clutter.
The original history remains available through the releases, tags, and the [archived pre-squash repository](https://github.com/Aiko-IT-Systems/DisCatSharp.Backup).

## Install

Install the latest stable release from [NuGet](https://www.nuget.org/packages/DisCatSharp):

```shell
dotnet add package DisCatSharp
```

To use the latest nightly release instead:

```shell
dotnet add package DisCatSharp --prerelease
```

Packages are also available from [GitHub Packages](https://github.com/orgs/Aiko-IT-Systems/packages?tab=packages&q=DisCatSharp).

## Quick start

Create a [Discord bot account](https://docs.dcs.aitsys.dev/articles/getting_started/bot_account), store its token in the `DISCORD_TOKEN` environment variable, and connect the client:

```csharp
using DisCatSharp;
using DisCatSharp.Enums;

string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")
    ?? throw new InvalidOperationException("Set the DISCORD_TOKEN environment variable.");

var discord = new DiscordClient(new DiscordConfiguration
{
    Token = token,
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged
});

await discord.ConnectAsync();
await Task.Delay(Timeout.InfiniteTimeSpan);
```

Continue with the complete [first bot guide](https://docs.dcs.aitsys.dev/articles/getting_started/first_bot).

## Documentation

The documentation is available at [docs.dcs.aitsys.dev](https://docs.dcs.aitsys.dev).
A backup is available at [backup-docs.dcs.aitsys.dev](https://backup-docs.dcs.aitsys.dev).

## AI coding assistants

Install the consumer skill globally for Codex:

```shell
gh skill install Aiko-IT-Systems/DisCatSharp use-discatsharp --agent codex --scope user --pin main
```

Then add the public, stateless documentation MCP as a Streamable HTTP server named `discatsharp`:

```toml
[mcp_servers.discatsharp]
url = "https://docs.dcs.aitsys.dev/mcp"
```

The MCP provides live documentation search, symbol lookup, full-page retrieval, and indexed source access. See the [Agent Skills and MCP setup guide](skills/README.md) for GitHub Copilot, Claude Code, Gemini CLI, other Agent Skills clients, version pinning, and maintainer guidance.

## Bugs and feature requests

Join our [official support guild](https://discord.gg/RXA6u3jxdU), open an [issue](https://github.com/Aiko-IT-Systems/DisCatSharp/issues/new/choose), or email [bugs@aitsys.dev](mailto:bugs@aitsys.dev).

## Tutorials and examples

- [Getting started](https://docs.dcs.aitsys.dev/articles/getting_started/bot_account)
- [Examples](https://github.com/Aiko-IT-Systems/DisCatSharp.Examples)
- [Template app (outdated)](https://github.com/Aiko-IT-Systems/DisCatSharp.TemplateApp)
- [Public support app for the DisCatSharp server](https://github.com/Aiko-IT-Systems/DisCatSharp.Support)

## Visual Studio tools

- [DisCatSharp Analyzer documentation](https://docs.dcs.aitsys.dev/vs/index)
- [Snippets for Visual Studio](https://github.com/Aiko-IT-Systems/DisCatSharp.Snippets)

----

## NuGet Packages

### Main

| Package | Purpose | LTS | Latest |
| --- | --- | --- | --- |
| [DisCatSharp](https://www.nuget.org/packages/DisCatSharp) | Core Discord API client with Gateway, REST, entities, and events. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.ApplicationCommands](https://www.nuget.org/packages/DisCatSharp.ApplicationCommands) | Slash commands, context menus, autocomplete, and command groups. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.ApplicationCommands.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.ApplicationCommands.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.CommandsNext](https://www.nuget.org/packages/DisCatSharp.CommandsNext) | Prefix and text command framework. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.CommandsNext.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.CommandsNext.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Interactivity](https://www.nuget.org/packages/DisCatSharp.Interactivity) | Interactive waits, pagination, components, modals, and response helpers. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Interactivity.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Interactivity.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |

### Voice

| Package | Purpose | LTS | Latest |
| --- | --- | --- | --- |
| [DisCatSharp.Lavalink](https://www.nuget.org/packages/DisCatSharp.Lavalink) | Lavalink v4 client for remote audio playback. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Lavalink.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Lavalink.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Voice](https://www.nuget.org/packages/DisCatSharp.Voice) | Direct Discord voice connections with audio send, receive, and DAVE support. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Voice.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Voice.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Voice.Natives](https://www.nuget.org/packages/DisCatSharp.Voice.Natives) | Native runtime libraries used by DisCatSharp.Voice. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Voice.Natives.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Voice.Natives.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |

### Hosting

| Package | Purpose | LTS | Latest |
| --- | --- | --- | --- |
| [DisCatSharp.Configuration](https://www.nuget.org/packages/DisCatSharp.Configuration) | Configuration models and binding for hosted bots. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Configuration.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Configuration.svg?label=&logo=nuget&color=%23ff1493&style=flat-square) |
| [DisCatSharp.Hosting](https://www.nuget.org/packages/DisCatSharp.Hosting) | .NET Generic Host integration and bot lifetime management. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Hosting.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Hosting.svg?label=&logo=nuget&color=%23ff1493&style=flat-square) |
| [DisCatSharp.Hosting.AspNetCore](https://www.nuget.org/packages/DisCatSharp.Hosting.AspNetCore) | ASP.NET Core ingress for interactions, webhooks, OAuth callbacks, and linked roles. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Hosting.AspNetCore.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Hosting.AspNetCore.svg?label=&logo=nuget&color=%23ff1493&style=flat-square) |
| [DisCatSharp.Hosting.DependencyInjection](https://www.nuget.org/packages/DisCatSharp.Hosting.DependencyInjection) | Dependency-injection helpers for clients and hosted bots. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Hosting.DependencyInjection.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Hosting.DependencyInjection.svg?label=&logo=nuget&color=%23ff1493&style=flat-square) |

### Templates

| Package | Purpose | LTS | Latest |
| --- | --- | --- | --- |
| [DisCatSharp.ProjectTemplates](https://www.nuget.org/packages/DisCatSharp.ProjectTemplates) | Project templates for creating hosted DisCatSharp bots. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.ProjectTemplates.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.ProjectTemplates.svg?label=&logo=nuget&color=%23ff1493&style=flat-square) |

### Development / Commons

| Package | Purpose | LTS | Latest |
| --- | --- | --- | --- |
| [DisCatSharp.Attributes](https://www.nuget.org/packages/DisCatSharp.Attributes) | Shared annotations used by DisCatSharp packages and tooling. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Attributes.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Attributes.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Common](https://www.nuget.org/packages/DisCatSharp.Common) | Shared utilities, regular expressions, and converters. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Common.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Common.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Experimental](https://www.nuget.org/packages/DisCatSharp.Experimental) | Experimental and unsupported Discord features that may change without notice. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Experimental.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Experimental.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Analyzer](https://www.nuget.org/packages/DisCatSharp.Analyzer) | Recommended analyzers and code fixes for DisCatSharp applications. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Analyzer.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Analyzer.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |

### [Extensions](https://github.com/Aiko-IT-Systems/DisCatSharp.Extensions)

| Package | Purpose | LTS | Latest |
| --- | --- | --- | --- |
| [DisCatSharp.Extensions.TwoFactorCommands](https://www.nuget.org/packages/DisCatSharp.Extensions.TwoFactorCommands) | Require two-factor authentication for selected commands. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Extensions.TwoFactorCommands.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Extensions.TwoFactorCommands.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Extensions.OAuth2Web](https://www.nuget.org/packages/DisCatSharp.Extensions.OAuth2Web) | **Deprecated.** Legacy OAuth web server; [migrate to DisCatSharp.Hosting.AspNetCore](https://docs.dcs.aitsys.dev/articles/modules/web_ingress/migration_from_oauth2web). | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Extensions.OAuth2Web.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Extensions.OAuth2Web.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| [DisCatSharp.Extensions.SimpleMusicCommands](https://www.nuget.org/packages/DisCatSharp.Extensions.SimpleMusicCommands) | Ready-to-use commands for building a simple music bot. | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Extensions.SimpleMusicCommands.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Extensions.SimpleMusicCommands.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |


----

## Sponsors (Current & Past)

- [Dei](https://github.com/DeividasKaza)
- [Will](https://github.com/villChurch)
- [SavageVictor](https://github.com/SavageVictor)
- [Schattenclown](https://github.com/Schattenclown)
- [FabiChan99](https://github.com/FabiChan99)

## Thanks

Big thanks goes to the following people who helped us without being part of the core team ♥️
- [Auros Nexus](https://github.com/Auros)
- [Lunar Starstrum](https://github.com/OoLunar)
- [Geferon](https://github.com/geferon)
- [Alice](https://github.com/QuantuChi)
- [Will](https://github.com/villChurch)
- [InFTord](https://github.com/InFTord)

## Special Thanks

The special thanks goes to Nagisa. Make sure to check out her [Instagram](https://www.instagram.com/nagisaarts_/) ♥️♥️

The second special thanks goes to [Sentry](https://sentry.io) ([GitHub](https://github.com/getsentry/)) for sponsoring us with a business account on Sentry for error tracking.
You guys are the best 💕⭐

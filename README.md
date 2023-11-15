[![Build](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/build.yml/badge.svg)](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/build.yml) [![Documentation](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/documentation.yml/badge.svg)](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/documentation.yml) [![CodeQL](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Aiko-IT-Systems/DisCatSharp/actions/workflows/codeql-analysis.yml) [![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FAiko-IT-Systems%2FDisCatSharp.svg?type=shield)](https://app.fossa.com/reports/d18d903c-f217-4d82-a7ec-e113fb147275?ref=badge_shield)

[![GitHub last commit](https://img.shields.io/github/last-commit/Aiko-IT-Systems/DisCatSharp?label=Last%20Commit&style=flat-square&logo=github)](https://aitsys.dev/source/DisCatSharp/history/) [![GitHub commit activity](https://img.shields.io/github/commit-activity/w/Aiko-IT-Systems/DisCatSharp?label=Commit%20Activity&style=flat-square&logo=github)](https://github.com/Aiko-IT-Systems/DisCatSharp/commits/main)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/Aiko-IT-Systems/DisCatSharp?label=PRs&style=flat-square&logo=github&logo=gitub)](https://github.com/Aiko-IT-Systems/DisCatSharp/pulls) ![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Aiko-IT-Systems/DisCatSharp?label=Size&style=flat-square&logo=github)

![Stable](https://img.shields.io/nuget/v/DisCatSharp?color=1F8B4C&label=Stable&style=flat-square&logo=Nuget) ![Nightly](https://img.shields.io/nuget/vpre/DisCatSharp?color=AD1457&label=Nightly&style=flat-square&logo=Nuget)

----

# DisCatSharp
## A Discord Bot Library written in C# for .NET

----

# News

## New

- NET 8 Support
- Support for the new [Username System](https://dis.gd/usernames) (pomelo)
- Support for [Linked Roles](https://discord.com/build/linked-roles)
- Full support for [Application Subscriptions](https://discord.com/build/apply-now)
- Support for [Voice Messages](https://support.discord.com/hc/en-us/articles/13091096725527)
- Partial support for role subscriptions
- Partial support for burst reactions
- Full support for onboarding
- Support for default select menu values (THANKS MAISY FOR ADDING IT TO DISCORD)

## Breaking

- Lavalink V4 implementation. Read more [here](https://docs.dcs.aitsys.dev/articles/modules/audio/lavalink_v4/intro)

----

# About

## Why DisCatSharp?

If you:
- want a library where you get kind and efficient help
- would like to have and use the most recent features of the Discord API
- are ready to build great things

Then this is the right place for you!

## Installing

You can install the library from the following sources:
- [NuGet](https://www.nuget.org/profiles/DisCatSharp)
- [GitHub](https://github.com/orgs/Aiko-IT-Systems/packages?tab=packages&q=DisCatSharp)

## Documentation

The documentation is available at [docs.dcs.aitsys.dev](https://docs.dcs.aitsys.dev).

Alternative hosts for our docs are:
- Backup Host [backup-docs.dcs.aitsys.dev](https://backup-docs.dcs.aitsys.dev)


## Bugs or Feature requests?

Either join our official support guild at https://discord.gg/Uk7sggRBTm

Or write us an email at [bugs@aitsys.dev](mailto:bugs@aitsys.dev).

<!-- All requests are tracked at [aitsys.dev](https://aitsys.dev/proje<ct/view/1/). We currently don't do that for reasons -->

## Tutorials / Examples

* [Howto](https://docs.dcs.aitsys.dev/articles/getting_started/bot_account.html)
* [Examples](https://github.com/Aiko-IT-Systems/DisCatSharp.Examples)
* [Template Bot(:warning:Outdated)](https://github.com/Aiko-IT-Systems/DisCatSharp.TemplateBot)
* [Public Support Bot for DisCatSharp Server](https://github.com/Aiko-IT-Systems/DisCatSharp.Support)

## Visual Studio Tools
* [DisCatSharp Analyzer Docs](https://docs.dcs.aitsys.dev/vs/index)
* [Snippets for Visual Studio](https://github.com/Aiko-IT-Systems/DisCatSharp.Snippets)

----

## NuGet Packages

### Main

| Package                         | Stable                                                                                                           | Nightly                                                                                                                             |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| DisCatSharp                     | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.svg?label=&logo=nuget&style=flat-square)                     | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.svg?label=&logo=nuget&style=flat-square&color=%23ff1493)                     |
| DisCatSharp.ApplicationCommands | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.ApplicationCommands.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.ApplicationCommands.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |
| DisCatSharp.CommandsNext        | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.CommandsNext.svg?label=&logo=nuget&style=flat-square)        | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.CommandsNext.svg?label=&logo=nuget&style=flat-square&color=%23ff1493)        |
| DisCatSharp.Interactivity       | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Interactivity.svg?label=&logo=nuget&style=flat-square)       | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Interactivity.svg?label=&logo=nuget&style=flat-square&color=%23ff1493)       |

### Voice

| Package                       | Stable                                                                                                         | Nightly                                                                                                                           |
| ----------------------------- | -------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| DisCatSharp.Lavalink          | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Lavalink.svg?label=&logo=nuget&style=flat-square)          | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Lavalink.svg?label=&logo=nuget&style=flat-square&color=%23ff1493)          |
| DisCatSharp.VoiceNext         | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.VoiceNext.svg?label=&logo=nuget&style=flat-square)         | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.VoiceNext.svg?label=&logo=nuget&style=flat-square&color=%23ff1493)         |
| DisCatSharp.VoiceNext.Natives | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.VoiceNext.Natives.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.VoiceNext.Natives.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |

### Hosting

| Package                                 | Stable                                                                                                                   | Nightly                                                                                                                                     |
| --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------- |
| DisCatSharp.Configuration               | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Configuration.svg?label=&logo=nuget&style=flat-square)               | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Configuration.svg?label=&logo=nuget&color=%23ff1493&style=flat-square)               |
| DisCatSharp.Hosting                     | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Hosting.svg?label=&logo=nuget&style=flat-square)                     | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Hosting.svg?label=&logo=nuget&color=%23ff1493&style=flat-square)                     |
| DisCatSharp.Hosting.DependencyInjection | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Hosting.DependencyInjection.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Hosting.DependencyInjection.svg?label=&logo=nuget&color=%23ff1493&style=flat-square) |

### Templates

| Package                                                                                         | Stable                                                                                                        | Nightly                                                                                                                          |
| ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| [DisCatSharp.ProjectTemplates](https://github.com/Aiko-IT-Systems/DisCatSharp.ProjectTemplates) | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.ProjectTemplates.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.ProjectTemplates.svg?label=&logo=nuget&color=%23ff1493&style=flat-square) |

### Development / Commons

| Package                      | Stable                                                                                                        | Nightly                                                                                                                          |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| DisCatSharp.Common           | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Common.svg?label=&logo=nuget&style=flat-square)           | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Common.svg?label=&logo=nuget&style=flat-square&color=%23ff1493)           |
| DisCatSharp.Analyzer.Roselyn | ![NuGet](https://img.shields.io/nuget/v/DisCatSharp.Analyzer.Roselyn.svg?label=&logo=nuget&style=flat-square) | ![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Analyzer.Roselyn.svg?label=&logo=nuget&style=flat-square&color=%23ff1493) |


----

## Sponsors

- [Dei](https://github.com/DeividasKaza)
- [Will](https://github.com/villChurch)
- [SavageVictor](https://github.com/SavageVictor)
- [Schattenclown](https://github.com/Schattenclown)
- [FabiChan99](https://github.com/FabiChan99)

## Hacktober Participants

The following users participated in Hacktoberfest 2022 and contributed to DisCatSharp:
- [Mira](https://github.com/TheXorog)
- [Sh1be](https://github.com/xMaxximum)
- [Lulalaby](https://github.com/Lulalaby)
- [Badger](https://github.com/JBraunsmaJr)

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

The second special thanks goes to [Sentry](https://sentry.io) ([GitHub](https://github.com/getsentry/)) for sponsering us a business account on sentry for error tracking.
You guys are the best 💕⭐

## Open Source License Status

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FAiko-IT-Systems%2FDisCatSharp.svg?type=large)](https://app.fossa.com/reports/d18d903c-f217-4d82-a7ec-e113fb147275?ref=badge_large)

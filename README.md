# DisCatSharp [![GitHub](https://img.shields.io/github/license/Aiko-IT-Systems/DisCatSharp?label=License)](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/LICENSE.md) [![Sponsors](https://img.shields.io/github/sponsors/Lulalaby?label=Sponsors)](https://github.com/sponsors/Lulalaby) [![Discord Server](https://img.shields.io/discord/858089281214087179.svg?label=Discord)](https://discord.gg/discatsharp)

![Temporary Logo](https://docs.dcs.aitsys.dev/logobig.png)

Discord Bot Library written in C# for .NET. https://discord.gg/discatsharp

#### Status
[![NuGet](https://img.shields.io/nuget/v/DisCatSharp.svg?label=NuGet%20Overall%20Version)](https://nuget.dcs.aitsys.dev)
[![Build status](https://ci.appveyor.com/api/projects/status/fy4xn9s3cq7j30j7/branch/main?svg=true)](https://ci.appveyor.com/project/AITSYS/discatsharp/branch/main)

#### Commit Activities
[![GitHub last commit](https://img.shields.io/github/last-commit/Aiko-IT-Systems/DisCatSharp?label=Last%20Commit)](https://bugs.aitsys.dev/source/DisCatSharp/history/)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/w/Aiko-IT-Systems/DisCatSharp?label=Commit%20Activity)](https://github.com/Aiko-IT-Systems/DisCatSharp/commits/main)

#### Stats
[![GitHub pull requests](https://img.shields.io/github/issues-pr/Aiko-IT-Systems/DisCatSharp?label=PRs)](https://github.com/Aiko-IT-Systems/DisCatSharp/pulls)
[![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Aiko-IT-Systems/DisCatSharp?label=Size)](#)
[![GitHub contributors](https://img.shields.io/github/contributors/Aiko-IT-Systems/DisCatSharp)](https://github.com/Aiko-IT-Systems/DisCatSharp/graphs/contributors)
[![GitHub Repo stars](https://img.shields.io/github/stars/Aiko-IT-Systems/DisCatSharp?label=Stars)](https://github.com/Aiko-IT-Systems/DisCatSharp/stargazers)

## Why DisCatSharp?
We want the lib always up-to-date. The newest features are important for us.

So the API version is always the newest, in the actual case `v9`.

## Where is the Changelog?
On our guild! You find it [here](#bugs-or-feature-requests).

## Installing
You can install the library from following source:

The latest release is always available on [NuGet](https://nuget.dcs.aitsys.dev).

## Documentation
The documentation for the latest stable version is available at [docs.dcs.aitsys.dev/lts](https://docs.dcs.aitsys.dev/lts).

The documentation of the latest nightly versions is available at [docs.dcs.aitsys.dev](https://docs.dcs.aitsys.dev).

## Bugs or Feature requests?
Join our [support guild](https://discord.gg/discatsharp) and open a support ticket. All requests are tracked at [bugs.aitsys.dev](https://bugs.aitsys.dev).

## Tutorials
* [Howto](https://docs.dcs.aitsys.dev/articles/basics/bot_account.html)
* [Examples](https://examples.dcs.aitsys.dev)

## Snippts
[Snippets for Visual Studio](https://github.com/Aiko-IT-Systems/DisCatSharp.Snippets)

## Latest NuGet Packages
Package|NuGet
|--|--|
DisCatSharp|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp)
DisCatSharp.ApplicationCommands|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.ApplicationCommands.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.ApplicationCommands)
DisCatSharp.CommandsNext|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.CommandsNext.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.CommandsNext)
DisCatSharp.Common|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Common.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.Common)
DisCatSharp.Interactivity|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Interactivity.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.Interactivity)
DisCatSharp.Lavalink|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Lavalink.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.Lavalink)
DisCatSharp.VoiceNext|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.VoiceNext.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.VoiceNext)
DisCatSharp.VoiceNext.Natives|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.VoiceNext.Natives.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.VoiceNext.Natives)

## Outdated NuGet Packages
Package|NuGet
|--|--|
DisCatSharp.SlashCommands|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.SlashCommands.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.SlashCommands)

## Releasing
To release a new version do the following steps:
- Create locally a repo named `release/VERSION` (Don't forget to replace VERSION with the target version number)
- Replace version number with the correct version in [appveyor.yml#L78](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/appveyor.yml#L78) with the new release number and [appveyor.yml#L5](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/appveyor.yml#L5) with the next-ahead release number.
- Replace nuget version number in [Version.targets#L4](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/Version.targets#L4)
- Publish branch to GitHub
- Wait for the CI/CD to complete.
- Merge the branch into main and delete it afterwards

## Thanks
Big thanks goes to the following people who helps us ♥️
- [Auros Nexus](https://github.com/Auros)
- [Lunar Starstrum](https://github.com/OoLunar)
- [Johannes](https://github.com/JMLutra)
- [Geferon](https://github.com/geferon)
- [Alice](https://github.com/mortAlice)

## Notice
Feel free to re-use code in DSharpPlus.

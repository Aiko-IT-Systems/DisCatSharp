# DisCatSharp ![GitHub](https://img.shields.io/github/license/Aiko-IT-Systems/DisCatSharp?label=License) ![Sponsors](https://img.shields.io/github/sponsors/Lulalaby?label=Sponsors) ![Discord Server](https://img.shields.io/discord/858089281214087179.svg?label=Discord)
Discord Bot Library written in C# for .NET.

#### Status
[![NuGet](https://img.shields.io/nuget/v/DisCatSharp.svg?label=NuGet%20Overall%20Version)](https://nuget.dcs.aitsys.dev)
[![Build status](https://ci.appveyor.com/api/projects/status/fy4xn9s3cq7j30j7/branch/main?svg=true)](https://ci.appveyor.com/project/AITSYS/discatsharp/branch/main)

#### Commit Activities
![GitHub last commit](https://img.shields.io/github/last-commit/Aiko-IT-Systems/DisCatSharp?label=Last%20Commit)
![GitHub commit activity](https://img.shields.io/github/commit-activity/w/Aiko-IT-Systems/DisCatSharp?label=Commit%20Activity)

#### Stats
![GitHub issues](https://img.shields.io/github/issues/Aiko-IT-Systems/DisCatSharp?label=Issues)
![GitHub pull requests](https://img.shields.io/github/issues-pr/Aiko-IT-Systems/DisCatSharp?label=PRs)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Aiko-IT-Systems/DisCatSharp?label=Size)
![GitHub contributors](https://img.shields.io/github/contributors/Aiko-IT-Systems/DisCatSharp)
![GitHub Repo stars](https://img.shields.io/github/stars/Aiko-IT-Systems/DisCatSharp?label=Stars)

## Why DisCatSharp?
We want the lib always up-to-date. The newest features are important for us.

With DSharpPlus and their way how to implement changes, it is not possible.

We decided to maintain an own version of it.

## Goal
We want this lib always up-to-date. So the API version is always the newest, in the actual case `v9`.

## Installing
You can install the library from following source:

The latest release is always available on [NuGet](https://nuget.dcs.aitsys.dev).

## Documentation
The documentation for the latest stable version is available at [docs.dcs.aitsys.dev](https://docs.dcs.aitsys.dev).

Do note that the documentation might not reflect the latest changes in nightly version of the library.

### Tutorials
* [Howto](https://docs.dcs.aitsys.dev/articles/basics/bot_account.html)
* [Examples](https://examples.dcs.aitsys.dev)

## Notice
**Do not start a fight**

No one have to use this. This is just to provide transparency & to provide this version to everyone who wants to use it.

Feel free to re-use code in DSharpPlus.

## Latest NuGet
Package|NuGet
|--|--|
DisCatSharp|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp)
DisCatSharp.CommandsNext|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.CommandsNext.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.CommandsNext)
DisCatSharp.Common|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Common.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.Common)
DisCatSharp.Interactivity|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Interactivity.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.Interactivity)
DisCatSharp.Lavalink|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.Lavalink.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.Lavalink)
DisCatSharp.SlashCommands|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.SlashCommands.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.SlashCommands)
DisCatSharp.VoiceNext|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.VoiceNext.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.VoiceNext)
DisCatSharp.VoiceNext.Natives|[![NuGet](https://img.shields.io/nuget/vpre/DisCatSharp.VoiceNext.Natives.svg?label=)](https://nuget.dcs.aitsys.dev/DisCatSharp.VoiceNext.Natives)

## Releasing
To release a new version do the following steps:
- Create locally a repo named `release/VERSION` (Don't forget to replace VERSION with the target version number)
- Replace version number with the correct version in [appveyor.yml#L70](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/appveyor.yml#L70) with the new release number and [appveyor.yml#L5](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/appveyor.yml#L5) with the next-ahead release number.
- Replace nuget version number in [Version.targets](https://github.com/Aiko-IT-Systems/DisCatSharp/blob/main/Version.targets)
- Publish branch to GitHub
- Wait for the CI/CD to complete.
- Merge the branch into main and delete it afterwards

# DSharpPlusNextGen ![GitHub](https://img.shields.io/github/license/Aiko-IT-Systems/DSharpPlusNextGen?label=License) ![Sponsors](https://img.shields.io/github/sponsors/Lulalaby?label=Sponsors)  ![Discord Server](https://img.shields.io/discord/858089281214087179.svg?label=Discord)
Discord Bot Library written in C# for .NET - based off [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).

#### Status
[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.svg?label=NuGet%20Overall%20Version)](https://nuget.dspng.aitsys.dev)
[![Build status](https://ci.appveyor.com/api/projects/status/9hv6emqnew8dgjue/branch/main?svg=true)](https://ci.appveyor.com/project/AITSYS/dsharpplusnextgen/branch/main)

#### Commit Activities
![GitHub last commit](https://img.shields.io/github/last-commit/Aiko-IT-Systems/DSharpPlusNextGen?label=Last%20Commit)
![GitHub commit activity](https://img.shields.io/github/commit-activity/w/Aiko-IT-Systems/DSharpPlusNextGen?label=Commit%20Activity)

#### Stats
![GitHub issues](https://img.shields.io/github/issues/Aiko-IT-Systems/DSharpPlusNextGen?label=Issues)
![GitHub pull requests](https://img.shields.io/github/issues-pr/Aiko-IT-Systems/DSharpPlusNextGen?label=PRs)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Aiko-IT-Systems/DSharpPlusNextGen?label=Size)
![GitHub contributors](https://img.shields.io/github/contributors/Aiko-IT-Systems/DSharpPlusNextGen)
![GitHub Repo stars](https://img.shields.io/github/stars/Aiko-IT-Systems/DSharpPlusNextGen?label=Stars)

## Why NextGen?
We want the lib always up-to-date. The newest features are important for us.

With DSharpPlus and their way how to implement changes, it is not possible.

We decided to maintain an own version of it.

## Goal
We want this lib always up-to-date. So the API version is always the newest, in the actual case `v9`.

## Installing
You can install the library from following source:

The latest release is always available on [NuGet](https://nuget.dspng.aitsys.dev).

## Documentation
The documentation for the latest stable version is available at [docs.dspng.aitsys.dev](https://docs.dspng.aitsys.dev).

Do note that the documentation might not reflect the latest changes in nightly version of the library.

### Tutorials
* [Howto](https://docs.dspng.aitsys.dev/articles/basics/bot_account.html)
* [Examples](https://examples.dspng.aitsys.dev)

## Notice
**Do not start a fight**

No one have to use this. This is just to provide transparency & to provide this version to everyone who wants to use it.

Feel free to re-use code in DSharpPlus.

## NuGet
Package|NuGet
|--|--|
DSharpPlusNextGen|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen)
DSharpPlusNextGen.CommandsNext|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.CommandsNext.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen.CommandsNext)
DSharpPlusNextGen.Common|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.Common.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen.Common)
DSharpPlusNextGen.Interactivity|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.Interactivity.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen.Interactivity)
DSharpPlusNextGen.Lavalink|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.Lavalink.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen.Lavalink)
DSharpPlusNextGen.SlashCommands|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.SlashCommands.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen.SlashCommands)
DSharpPlusNextGen.VoiceNext|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.VoiceNext.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen.VoiceNext)
DSharpPlusNextGen.VoiceNext.Natives|[![NuGet](https://img.shields.io/nuget/vpre/DSharpPlusNextGen.VoiceNext.Natives.svg?label=)](https://nuget.dspng.aitsys.dev/DSharpPlusNextGen.VoiceNext.Natives)

## Releasing
To release a new version do the following steps:
- Create locally a repo named `release/VERSION` (Don't forget to replace VERSION with the target version number)
- Replace version number with the correct version in https://github.com/Aiko-IT-Systems/DSharpPlusNextGen/blob/main/appveyor.yml#L69 and https://github.com/Aiko-IT-Systems/DSharpPlusNextGen/blob/main/appveyor.yml#L5
- Replace nuget version number in https://github.com/Aiko-IT-Systems/DSharpPlusNextGen/blob/main/Version.targets
- Publish branch to GitHub
- Wait for the CI/CD to complete.
- Merge the branch into main and delete it afterwards
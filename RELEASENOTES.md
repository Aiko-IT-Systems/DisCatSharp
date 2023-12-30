DisCatSharp Release Notes

	Important fix:
 	- Apparently the built-in c# method for building uris broke. The gateway uri included never the gateway version, encoding and compression. This is fixed now!

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods

    Notable Changes
    - Full support for onboarding
    - Custom status support
    - Full support for Application Subscriptions aka. Premium Apps
    - DiscordOAuth2Client: Allows bots to request and use access tokens for the Discord API.
    - Support for default select menu values (THANKS MAISY FOR ADDING IT TO DISCORD)
    - DisCatSharp can now check for new releases on startup. Including support for extensions 


DisCatSharp.Attributes Release Notes

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods

    Added new required feature enums to notate feature usage


DisCatSharp.ApplicationCommands Release Notes

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods

    Contains a rework for command registration (Kinda wacky tho with translation-enabled commands)
	Fixed a major issue with application commands. Upgrade mandatory


DisCatSharp.CommandsNext Release Notes

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods


DisCatSharp.Interactivity Release Notes

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods

    Contains important bug fixes for interactions and pagination

DisCatSharp.Common Release Notes

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods

    We added all of our regexes to the Common package as GenereicRegexes


DisCatSharp.Lavalink Release Notes

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods
    - Lavalink got a complete rework for V4.
    - Visit the documentation for more information: https://docs.dcs.aitsys.dev/articles/modules/audio/lavalink_v4/intro


DisCatSharp.VoiceNext Release Notes

    Breaking changes:
    - Dropped support for .NET 6
    - Removed previously deprecated fields and methods

    Will be deprecated soon and replaced by DisCatSharp.Voice


DisCatSharp.Experimental Release Notes

    Breaking changes:
    - Dropped support for .NET 6


DisCatSharp.Configuration Release Notes

    Breaking changes:
    - Dropped support for .NET 6


DisCatSharp.Hosting Release Notes

    Breaking changes:
    - Dropped support for .NET 6


DisCatSharp.Hosting.DependencyInjection Release Notes

    Breaking changes:
    - Dropped support for .NET 6


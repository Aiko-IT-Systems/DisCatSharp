DisCatSharp Release Notes

    - Full support for Components V2
    - Stability improvements across gateway, interactivity and voice
    - Built-in statistics support
    - Support for .NET 10 & .NET 11
    - Improvements for OAuth2 operations
    - Support for Lavalink 4.2.0 (including LavaLyrics)
    - New voice implementation with DAVE (E2EE) support
    - New packages: DisCatSharp.Voice & DisCatSharp.Voice.Natives
    - AuditLog has been completely rewritten from scratch

    Breaking

    - DiscordAttachment: MediaType renamed to ContentType
    - DiscordTextComponent: switched parameter order (customId, label)
    - Application Commands: removed dmPermission, use allowedContexts instead
    - LavalinkGuildPlayer: RemoveQueue renamed to RemoveFromQueue
    - Url fields now use DiscordUri instead of string
    - Updated namespaces in DisCatSharp.Interactivity
    - Removed DisCatSharp.VoiceNext and DisCatSharp.VoiceNext.Natives

DisCatSharp.Attributes Release Notes

    - None

DisCatSharp.ApplicationCommands Release Notes

    - Internal improvements and fixes

DisCatSharp.CommandsNext Release Notes

    - None

DisCatSharp.Interactivity Release Notes

    - Added full Components V2 support
    - Improved pagination and component handling
    - Modal improvements (checkboxes, radio groups)

DisCatSharp.Common Release Notes

    - None

DisCatSharp.Lavalink Release Notes

    - New queue system
    - Support for LavaLyrics plugin

DisCatSharp.Voice Release Notes

    - Replaces DisCatSharp.VoiceNext
    - Full Discord voice send/receive support
    - DAVE protocol support
    - New native runtime package: DisCatSharp.Voice.Natives

DisCatSharp.Experimental Release Notes

    - Message search support
    - Guild member search (Elasticsearch-based)
    - GCP attachments

DisCatSharp.Configuration Release Notes

    - Added proxy support via appsettings.json

DisCatSharp.Hosting Release Notes

    - None

DisCatSharp.Hosting.DependencyInjection Release Notes

    - None

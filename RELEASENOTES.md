DisCatSharp Release Notes

	- Full support for Components V2
	- Stability Improvements
	- Build-in Statistics
	- Support for .NET 10
	- Improvements for OAuth2 operations

    Breaking

    - **DiscordAttachment**: Renamed `MediaType` to `ContentType` to align with Discord's API.
    - **DiscordTextComponent**: Switched the position of `customId` and `label` because of nullability.
    - **Application Commands**: Removed `dmPermission` fields, causing DisCatSharp to do weird bulk-updates. Use `allowedContexts` instead.
    - **LavalinkGuildPlayer**: `RemoveQueue` renamed to [`RemoveFromQueue`]((xref:DisCatSharp.Lavalink.Entities.LavalinkGuildPlayer.RemoveFromQueue*)).
    - **Url fields**: Any `Url` fields on objects like `DiscordAttachment`, `DiscordEmbed`, etc., are now of type [`DiscordUri`](xref:DisCatSharp.Entities.DiscordUri) instead of `string`. Use `.ToUri()` to get a `Uri` object or use `.ToString()`.
    - We updated some namespaces in DisCatSharp.Interactivity. You might need to update your imports for some entities and enums.


DisCatSharp.Attributes Release Notes

    - None


DisCatSharp.ApplicationCommands Release Notes

    - Some optimizations


DisCatSharp.CommandsNext Release Notes

    - None

DisCatSharp.Interactivity Release Notes

    - None

DisCatSharp.Common Release Notes

    - None


DisCatSharp.Lavalink Release Notes

    - New queue system. See https://docs.dcs.aitsys.dev/articles/modules/audio/lavalink_v4/queue
    - Support for LavaLyrics plugin


DisCatSharp.VoiceNext Release Notes

    - Will be deprecated 2025 and replaced by DisCatSharp.Voice


DisCatSharp.Experimental Release Notes

    - GCP Attachments
    - Guild Member Search powered by elasticsearch


DisCatSharp.Configuration Release Notes

    - None


DisCatSharp.Hosting Release Notes

    - None


DisCatSharp.Hosting.DependencyInjection Release Notes

    - None

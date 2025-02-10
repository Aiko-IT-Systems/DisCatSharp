DisCatSharp Release Notes

	- Full support for role subscriptions
	- Full support for burst reactions
	- Full support for subscriptions and entitlements
	- Support for join request (create, update & delete) events for clans
	- Support for message forwarding
	- Support for application emojis
	- Fix for gateway close code handling

    Breaking

    - Small breaking change in `AttachmentFlags`: The flag values are now prefixed with `Is` to be in line with other flag names.
    - `AddMention(IMention mention)` => `WithAllowedMention(IMention allowedMention)`
    - `AddMentions(IEnumerable<IMention> mentions)` => `WithAllowedMentions(IEnumerable<IMention> allowedMentions)`


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

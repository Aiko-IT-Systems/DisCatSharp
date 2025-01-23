DisCatSharp Release Notes

    - Full support for role subscriptions
    - Full support for burst reactions
    - Full support for subscriptions and entitlements
    - New queue system for Lavalink
    - Support for join request (create, update & delete) events for clans

    Breaking

    - Small breaking change in `AttachmentFlags`: The flag values are now prefixed with `Is` to be in line with other flag names.
    - `AddMention(IMention mention)` => `WithAllowedMention(IMention allowedMention)`
    - `AddMentions(IEnumerable<IMention> mentions)` => `WithAllowedMentions(IEnumerable<IMention> allowedMentions)`


DisCatSharp.Attributes Release Notes

    None


DisCatSharp.ApplicationCommands Release Notes

    None


DisCatSharp.CommandsNext Release Notes

    None

DisCatSharp.Interactivity Release Notes

    None

DisCatSharp.Common Release Notes

    None


DisCatSharp.Lavalink Release Notes

    None


DisCatSharp.VoiceNext Release Notes

    Will be deprecated 2025 and replaced by DisCatSharp.Voice


DisCatSharp.Experimental Release Notes

    - GCP Attachments
    - Guild Member Search powered by elasticsearch


DisCatSharp.Configuration Release Notes

    None


DisCatSharp.Hosting Release Notes

    None


DisCatSharp.Hosting.DependencyInjection Release Notes

    None

# v10.1.0

**Full Changelog**: [GitHub](https://github.com/Aiko-IT-Systems/DisCatSharp/compare/10.0.0...v10.1.0)

## Changes

> [!NOTE]
 > This release contains breaking changes. Please read the changelog carefully.
 > Some bug fixes aren't noted here

### All packages
NuGet packages now support Source Link & Deterministic Builds.
- Updated the NuGet specs to be compatible with NuGet Gallery.
- Changed PackageLicenseUrl to PackageLicenseFile and included the top-level LICENSE.md
- Changed PackageIconUrl to PackageIcon and included DisCatSharp.Logos/logobig.png


### DisCatSharp
- Reworked documentation
- Support for sending component-only messages
- Reworked component result for modal submits
- DiscordClient.ReconnectAsync param startNewSession now defaults to true
- Added Avatar Decorations
- Added Theme Colors
- Fixed webhooks for threads
- Added DiscordMember.DisconnectFromVoiceAsync
- Added support for the 'X-Discord-Locale' Header in the [DiscordConfiguration](xref:DisCatSharp.DiscordConfiguration#DisCatSharp_DiscordConfiguration_Locale).
- Implemented Forum Channels
	* Added fields (DiscordChannel PostCreateUserRateLimit, DefaultReactionEmoji (new entity ForumReactionEmoji), AvailableTags | DiscordThreadChannel TotalMessagesSent, AppliedTags)
	* Added operations to create a forum through the guild entity (DiscordGuild.CreateForumChannelAsync)
	* Added operations to modify a forum channel (DiscordChannel.ModifyForumAsync)
	* Added operations to create and delete tags (DiscordChannel CreateForumPostTagAsync, GetForumPostTag, DeleteForumPostTag)
	* Added operations to modify tags (new entity ForumPostTag) (ForumPostTag ModifyAsync, DeleteAsync)
	* Fixed bugs in forum channel post creation
	* Added forum post tag operations on threads
	* Added checks to channel update
	* AvailableTags Object in DiscordChannel is now read-only
	* Handle available_tags Key in Channel Update
	* Forum Channels are now included when using OrderedChannels, GetOrderedChannels or GetOrderedChannelsAsync
- Dropped support for channel banners, it sadly never made it's way into discord
- Implemented ResumeGatewayUrl
- Added GuildFeatures GuildHomeTest (Experimental) & InvitesDisabled
- Add disable invites for DiscordGuild (In experiment, won't work)
	* Added new function EnableInvitesAsync
	* Added new function DisableInvitesAsync
- Reworked DiscordIntegration
	* Added SubscriberCount
	* Added Revoked
	* Added Application
	* Added Scopes
	* Removed int ExpireBehavior
	* Added ExpireBehavior as new enum IntegrationExpireBehavior
- Reworked DiscordConnection
	* Removed int Visibility
	* Added Visibility as new enum ConnectionVisibilityType
	* Added TwoWayLink
- Implemented DiscordWebhookBuilder.WithThreadName to create forum posts via webhook
- Added DisCatSharp.ApplicationFlags.ApplicationCommandBadge
- Added a bypassCache Option to DiscordChannel.GetMessageAsync
- Added the new field app_permissions to the interaction entity and the context entities.
- Implemented DiscordGuild.EnableMfaAsync
- Implemented DiscordGuild.DisableMfaAsync
- Removed guild related enums from the DisCatSharp namespace
- Added guild related enums to the DisCatSharp.Enums namespace

### DisCatSharp.ApplicationCommands
- Added Translation Generator & Exporter
- Fixed double interaction bug
- Fixed int > long cast exception
- Fixed a bug where the default help command would not work if auto defer was enabled
- Added support for slash commands in shards
- Removed ApplicationCommandsExtension.CleanGuildCommandsAsync()
- Removed ApplicationCommandsExtension.CleanGlobalCommandsAsync()
- Added DiscordClient.RemoveGlobalApplicationCommandsAsync()
- Added DiscordClient.RemoveGuildApplicationCommandsAsync(ulong)
- Added DiscordClient.RemoveGuildApplicationCommandsAsync(DiscordGuild)
- Fixed DmPermissions check for Application Commands on registration
- Reworked application command registration
- Changed namespaces
	* DisCatSharp.ApplicationCommands;
	* DisCatSharp.ApplicationCommands.Attributes;
	* DisCatSharp.ApplicationCommands.Context;
	* DisCatSharp.ApplicationCommands.Exceptions
- Reworked translation for application commands
- ApplicationCommandsExtension.StartupFinished now defaults to false
- ApplicationCommandsExtension.UpdateAsync checks now if it's just a restart to avoid crash
- Various bug fixes
- Implemented support for minimum_length and maximum_length for application command options of type string.
- Renamed MinimumAttribute to MinimumValueAttribute. Valid for: int, long & double
- Renamed MaximumAttribute & MaximumValueAttribute. Valid for: int, long & double
- Added MinimumLengthAttribute . Minimum int if set: 0. Valid for: string
- Added MaximumLengthAttribute. Minimum int if set: 1. Valid for: string
- Proper exception log when registering app commands fails

### DisCatSharp.Lavalink
- Added support for spotify search
- Added support for apple music search

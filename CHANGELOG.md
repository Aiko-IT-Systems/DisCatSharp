# v10.1.0

**Full Changelog**: [GitHub](https://github.com/Aiko-IT-Systems/DisCatSharp/compare/10.0.0...v10.1.0)

## Changes

> [!NOTE]
 > This release contains breaking changes. Please read the changelog carefully.
 > Some bug fixes aren't noted here.

### All packages
NuGet packages now support Source Link & Deterministic Builds.
- Updated the NuGet specs to be compatible with NuGet Gallery.
- Changed PackageLicenseUrl to PackageLicenseFile and included the top-level LICENSE.md
- Changed PackageIconUrl to PackageIcon and included DisCatSharp.Logos/logobig.png


### DisCatSharp
- Documentation has a bunch of new and reworked articles!
<br></br>
- Implemented Forum Channels
	* Added fields (`DiscordChannel.PostCreateUserRateLimit`, `.DefaultReactionEmoji` (new entity `ForumReactionEmoji`), `.AvailableTags` and `DiscordThreadChannel.TotalMessagesSent`, `.AppliedTags`)
	* Added function to create a forum through the guild entity (`DiscordGuild.CreateForumChannelAsync`)
	* Added functions to modify a forum channel (`DiscordChannel.ModifyForumAsync`)
	* Added functions to create and delete tags (`DiscordChannel.CreateForumPostTagAsync`, `.GetForumPostTag`, `.DeleteForumPostTag`)
	* Added functions to modify tags (new entity ForumPostTag) (`ForumPostTag.ModifyAsync`, `.DeleteAsync`)
	* Fixed bugs in forum channel post creation
	* Added forum post tag operations on threads
	* Added checks to channel update
	* `AvailableTags` Object in DiscordChannel is now read-only
	* Handle available_tags Key in Channel Update
- Added disable invites for DiscordGuild (In experiment, won't work)
	* Added new function `DiscordGuild.EnableInvitesAsync`
	* Added new function `DiscordGuild.DisableInvitesAsync`
- `DiscordChannel.OrderedChannels`, `.GetOrderedChannels` and `.GetOrderedChannelsAsync` now include Forum Channels
- Added `DiscordMember.DisconnectFromVoiceAsync`
- Added Avatar Decorations
- Added Theme Colors
- Added support for the `X-Discord-Locale` Header in the [DiscordConfiguration]
- Added support for sending component-only messages
- Implemented `ResumeGatewayUrl`
- Added `GuildFeatures`: `GuildHomeTest` (Experimental) & `InvitesDisabled`
- Implemented `DiscordWebhookBuilder.WithThreadName` to create forum posts via a webhook
- Added `DisCatSharp.ApplicationFlags.ApplicationCommandBadge`
- Added a `bypassCache` option to `DiscordChannel.GetMessageAsync`
- Added the new field app_permissions to the interaction entity and the context entities.
- Added function `DiscordGuild.EnableMfaAsync`
- Added function `DiscordGuild.DisableMfaAsync`
<br></br>
- Reworked component result for modal submits
- Reworked `DiscordIntegration`
	* Added `SubscriberCount`
	* Added `Revoked`
	* Added `Application`
	* Added `Scopes`
	* Removed int `ExpireBehavior`
	* Added `ExpireBehavior` as new enum `IntegrationExpireBehavior`
- Reworked `DiscordConnection`
	* Removed int `Visibility`
	* Added `Visibility` as new enum `ConnectionVisibilityType`
	* Added `TwoWayLink`
- `DiscordClient.ReconnectAsync` param `startNewSession` now defaults to true
- Moved guild related enums from the `DisCatSharp` to the `DisCatSharp.Enums` namespace
<br></br>
- Fixed webhooks for threads
(xref:DisCatSharp.DiscordConfiguration#DisCatSharp_DiscordConfiguration_Locale).
- Dropped support for channel banners, it sadly never made its way into discord

### DisCatSharp.ApplicationCommands
- Added support for slash commands in shards
- Added Translation Generator & Exporter
- Added `DiscordClient.RemoveGlobalApplicationCommandsAsync()`
- Added `DiscordClient.RemoveGuildApplicationCommandsAsync(ulong)`
- Added `DiscordClient.RemoveGuildApplicationCommandsAsync(DiscordGuild)`
- Implemented support for minimum_length and maximum_length for application command options of type string.
	- Added `MinimumLengthAttribute`. Minimum int if set: 0. Valid for: string
	- Added `MaximumLengthAttribute`. Minimum int if set: 1. Valid for: string
<br></br>
- Changed namespaces
	* `DisCatSharp.ApplicationCommands`;
	* `DisCatSharp.ApplicationCommands.Attributes`;
	* `DisCatSharp.ApplicationCommands.Context`;
	* `DisCatSharp.ApplicationCommands.Exceptions`;
- Renamed `MinimumAttribute` to `MinimumValueAttribute`. Valid for: int, long & double
- Renamed `MaximumAttribute` & `MaximumValueAttribute`. Valid for: int, long & double
- Proper exception log when registering app commands fails
- Reworked translation for application commands
- `ApplicationCommandsExtension.StartupFinished` now defaults to false
- `ApplicationCommandsExtension.UpdateAsync` checks now if it's just a restart to avoid crash
- Reworked application command registration
- Fixed DmPermissions check for Application Commands on registration
- Fixed double interaction bug
- Fixed int > long cast exception
- Fixed a bug where the default help command would not work if auto defer was enabled
- Various bug fixes
<br></br>
- Removed `ApplicationCommandsExtension.CleanGuildCommandsAsync()`
- Removed `ApplicationCommandsExtension.CleanGlobalCommandsAsync()`

### DisCatSharp.Lavalink
- Added support for apple music & spotify search

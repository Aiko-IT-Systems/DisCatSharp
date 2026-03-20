using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents type of the action that was taken in given audit log event.
/// </summary>
public enum AuditLogActionType
{
	/// <summary>
	///     Indicates an invalid action type.
	/// </summary>
	Invalid = 0,

	/// <summary>
	///     Indicates that the guild was updated.
	/// </summary>
	GuildUpdate = 1,

	/// <summary>
	///     Indicates that the channel was created.
	/// </summary>
	ChannelCreate = 10,

	/// <summary>
	///     Indicates that the channel was updated.
	/// </summary>
	ChannelUpdate = 11,

	/// <summary>
	///     Indicates that the channel was deleted.
	/// </summary>
	ChannelDelete = 12,

	/// <summary>
	///     Indicates that the channel permission overwrite was created.
	/// </summary>
	OverwriteCreate = 13,

	/// <summary>
	///     Indicates that the channel permission overwrite was updated.
	/// </summary>
	OverwriteUpdate = 14,

	/// <summary>
	///     Indicates that the channel permission overwrite was deleted.
	/// </summary>
	OverwriteDelete = 15,

	/// <summary>
	///     Indicates that the user was kicked.
	/// </summary>
	Kick = 20,

	/// <summary>
	///     Indicates that users were pruned.
	/// </summary>
	Prune = 21,

	/// <summary>
	///     Indicates that the user was banned.
	/// </summary>
	Ban = 22,

	/// <summary>
	///     Indicates that the user was unbanned.
	/// </summary>
	Unban = 23,

	/// <summary>
	///     Indicates that the member was updated.
	/// </summary>
	MemberUpdate = 24,

	/// <summary>
	///     Indicates that the member's roles were updated.
	/// </summary>
	MemberRoleUpdate = 25,

	/// <summary>
	///     Indicates that the member has moved to another voice channel.
	/// </summary>
	MemberMove = 26,

	/// <summary>
	///     Indicates that the member has disconnected from a voice channel.
	/// </summary>
	MemberDisconnect = 27,

	/// <summary>
	///     Indicates that a bot was added to the guild.
	/// </summary>
	BotAdd = 28,

	/// <summary>
	///     Indicates that the role was created.
	/// </summary>
	RoleCreate = 30,

	/// <summary>
	///     Indicates that the role was updated.
	/// </summary>
	RoleUpdate = 31,

	/// <summary>
	///     Indicates that the role was deleted.
	/// </summary>
	RoleDelete = 32,

	/// <summary>
	///     Indicates that the invite was created.
	/// </summary>
	InviteCreate = 40,

	/// <summary>
	///     Indicates that the invite was updated.
	/// </summary>
	InviteUpdate = 41,

	/// <summary>
	///     Indicates that the invite was deleted.
	/// </summary>
	InviteDelete = 42,

	/// <summary>
	///     Indicates that the webhook was created.
	/// </summary>
	WebhookCreate = 50,

	/// <summary>
	///     Indicates that the webhook was updated.
	/// </summary>
	WebhookUpdate = 51,

	/// <summary>
	///     Indicates that the webhook was deleted.
	/// </summary>
	WebhookDelete = 52,

	/// <summary>
	///     Indicates that an emoji was created.
	/// </summary>
	EmojiCreate = 60,

	/// <summary>
	///     Indicates that an emoji was updated.
	/// </summary>
	EmojiUpdate = 61,

	/// <summary>
	///     Indicates that an emoji was deleted.
	/// </summary>
	EmojiDelete = 62,

	/// <summary>
	///     Indicates that the message was deleted.
	/// </summary>
	MessageDelete = 72,

	/// <summary>
	///     Indicates that messages were bulk-deleted.
	/// </summary>
	MessageBulkDelete = 73,

	/// <summary>
	///     Indicates that a message was pinned.
	/// </summary>
	MessagePin = 74,

	/// <summary>
	///     Indicates that a message was unpinned.
	/// </summary>
	MessageUnpin = 75,

	/// <summary>
	///     Indicates that an integration was created.
	/// </summary>
	IntegrationCreate = 80,

	/// <summary>
	///     Indicates that an integration was updated.
	/// </summary>
	IntegrationUpdate = 81,

	/// <summary>
	///     Indicates that an integration was deleted.
	/// </summary>
	IntegrationDelete = 82,

	/// <summary>
	///     Indicates that a stage instance was created.
	/// </summary>
	StageInstanceCreate = 83,

	/// <summary>
	///     Indicates that a stage instance was updated.
	/// </summary>
	StageInstanceUpdate = 84,

	/// <summary>
	///     Indicates that a stage instance was deleted.
	/// </summary>
	StageInstanceDelete = 85,

	/// <summary>
	///     Indicates that a sticker was created.
	/// </summary>
	StickerCreate = 90,

	/// <summary>
	///     Indicates that a sticker was updated.
	/// </summary>
	StickerUpdate = 91,

	/// <summary>
	///     Indicates that a sticker was deleted.
	/// </summary>
	StickerDelete = 92,

	/// <summary>
	///     Indicates that a guild scheduled event was created.
	/// </summary>
	GuildScheduledEventCreate = 100,

	/// <summary>
	///     Indicates that a guild scheduled event was updated.
	/// </summary>
	GuildScheduledEventUpdate = 101,

	/// <summary>
	///     Indicates that a guild scheduled event was deleted.
	/// </summary>
	GuildScheduledEventDelete = 102,

	/// <summary>
	///     Indicates that a thread was created.
	/// </summary>
	ThreadCreate = 110,

	/// <summary>
	///     Indicates that a thread was updated.
	/// </summary>
	ThreadUpdate = 111,

	/// <summary>
	///     Indicates that a thread was deleted.
	/// </summary>
	ThreadDelete = 112,

	/// <summary>
	///     Indicates that the permissions for an application command were updated.
	/// </summary>
	ApplicationCommandPermissionUpdate = 121,

	/// <summary>
	///     Indicates that a soundboard sound was created.
	/// </summary>
	SoundboardSoundCreate = 130,

	/// <summary>
	///     Indicates that a soundboard sound was updated.
	/// </summary>
	SoundboardSoundUpdate = 131,

	/// <summary>
	///     Indicates that a soundboard sound was deleted.
	/// </summary>
	SoundboardSoundDelete = 132,

	/// <summary>
	///     Indicates that a new automod rule has been added.
	/// </summary>
	AutoModerationRuleCreate = 140,

	/// <summary>
	///     Indicates that an automod rule was updated.
	/// </summary>
	AutoModerationRuleUpdate = 141,

	/// <summary>
	///     Indicates that an automod rule was deleted.
	/// </summary>
	AutoModerationRuleDelete = 142,

	/// <summary>
	///     Indicates that automod blocked a message.
	/// </summary>
	AutoModerationBlockMessage = 143,

	/// <summary>
	///     Indicates that automod flagged a message.
	/// </summary>
	AutoModerationFlagMessage = 144,

	/// <summary>
	///     Indicates that automod timed out a user.
	/// </summary>
	AutoModerationTimeOutUser = 145,

	/// <summary>
	///     Indicates that automod quarantined a user.
	/// </summary>
	AutoModerationQuarantineUser = 146,

	/// <summary>
	///     Indicates that a creator monetization request was created for the guild.
	/// </summary>
	CreatorMonetizationRequestCreated = 150,

	/// <summary>
	///     Indicates that the guild accepted creator monetization terms.
	/// </summary>
	CreatorMonetizationTermsAccepted = 151,

	/// <summary>
	///     Indicates that an onboarding prompt was created.
	/// </summary>
	OnboardingPromptCreate = 163,

	/// <summary>
	///     Indicates that an onboarding prompt was updated.
	/// </summary>
	OnboardingPromptUpdate = 164,

	/// <summary>
	///     Indicates that an onboarding prompt was deleted.
	/// </summary>
	OnboardingPromptDelete = 165,

	/// <summary>
	///     Indicates that guild onboarding was initialized.
	/// </summary>
	OnboardingCreate = 166,

	/// <summary>
	///     Indicates that guild onboarding was updated.
	/// </summary>
	OnboardingUpdate = 167,

	/// <summary>
	///     Alias for <see cref="ServerGuideCreate" /> retained for Discord naming compatibility.
	/// </summary>
	HomeSettingsCreate = ServerGuideCreate,

	/// <summary>
	///     Indicates that guild home or server guide settings were created.
	/// </summary>
	ServerGuideCreate = 190,

	/// <summary>
	///     Alias for <see cref="ServerGuideUpdate" /> retained for Discord naming compatibility.
	/// </summary>
	HomeSettingsUpdate = ServerGuideUpdate,

	/// <summary>
	///     Indicates that guild home or server guide settings were updated.
	/// </summary>
	ServerGuideUpdate = 191,

	/// <summary>
	///     Indicates that a voice channel status entry was created or updated.
	/// </summary>
	VoiceChannelStatusCreate = 192,

	/// <summary>
	///     Indicates that a voice channel status entry was deleted.
	/// </summary>
	VoiceChannelStatusDelete = 193,

	/// <summary>
	///     Indicates that Clyde AI profile settings were updated.
	/// </summary>
	/// <remarks>
	///     This action is deprecated and is not expected to appear in modern audit log payloads.
	/// </remarks>
	[DiscordDeprecated]
	ClydeAiProfileUpdate = 194,

	/// <summary>
	///     Indicates that a guild scheduled event exception was created.
	/// </summary>
	GuildScheduledEventExceptionCreate = 200,

	/// <summary>
	///     Indicates that a guild scheduled event exception was updated.
	/// </summary>
	GuildScheduledEventExceptionUpdate = 201,

	/// <summary>
	///     Indicates that a guild scheduled event exception was deleted.
	/// </summary>
	GuildScheduledEventExceptionDelete = 202,

	/// <summary>
	///     Indicates that the guild's member verification configuration was updated.
	/// </summary>
	GuildMemberVerificationUpdate = 210,

	/// <summary>
	///     Indicates that the guild's public profile configuration was updated.
	/// </summary>
	GuildProfileUpdate = 211,

	/// <summary>
	///     Indicates that the guild migrated to the dedicated pin messages permission.
	/// </summary>
	GuildMigratePinPermission = 212,

	/// <summary>
	///     Indicates that the guild migrated to the dedicated bypass slowmode permission.
	/// </summary>
	GuildMigrateBypassSlowmodePermission = 213
}

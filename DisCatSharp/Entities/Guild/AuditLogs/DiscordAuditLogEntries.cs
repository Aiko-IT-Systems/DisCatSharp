using System.Collections.Generic;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a guild audit log entry.
/// </summary>
public sealed class DiscordGuildAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted guild.
	/// </summary>
	public DiscordGuild? TargetGuild { get; internal set; }

	/// <summary>
	///     Gets the typed guild change helper view.
	/// </summary>
	public DiscordGuildAuditLogChanges ChangeSet
		=> new(this);
}

/// <summary>
///     Represents a channel audit log entry.
/// </summary>
public sealed class DiscordChannelAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted channel.
	/// </summary>
	public DiscordChannel? TargetChannel { get; internal set; }

	/// <summary>
	///     Gets the typed channel change helper view.
	/// </summary>
	public DiscordChannelAuditLogChanges ChangeSet
		=> new(this);
}

/// <summary>
///     Represents an overwrite audit log entry.
/// </summary>
public sealed class DiscordOverwriteAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the channel.
	/// </summary>
	public DiscordChannel? Channel { get; internal set; }

	/// <summary>
	///     Gets the overwritten entity id.
	/// </summary>
	public ulong? OverwrittenEntityId { get; internal set; }

	/// <summary>
	///     Gets the overwritten entity type.
	/// </summary>
	public string? OverwrittenEntityType { get; internal set; }

	/// <summary>
	///     Gets the overwrite role name.
	/// </summary>
	public string? RoleName { get; internal set; }

	/// <summary>
	///     Gets the overwritten role when the overwrite targets a role and the reference has been resolved or hydrated.
	/// </summary>
	public DiscordRole? OverwrittenRole { get; internal set; }

	/// <summary>
	///     Gets the overwritten member when the overwrite targets a member and the reference has been resolved or hydrated.
	/// </summary>
	public DiscordMember? OverwrittenMember { get; internal set; }

	/// <summary>
	///     Gets the typed overwrite target kind derived from Discord's raw overwrite type string.
	/// </summary>
	public AuditLogOverwriteTargetType OverwriteTargetType
		=> this.OverwrittenEntityType switch
		{
			"0" => AuditLogOverwriteTargetType.Role,
			"1" => AuditLogOverwriteTargetType.Member,
			_ => AuditLogOverwriteTargetType.Unknown
		};

	/// <summary>
	///     Gets whether the overwrite targets a role.
	/// </summary>
	public bool TargetsRole
		=> this.OverwriteTargetType is AuditLogOverwriteTargetType.Role;

	/// <summary>
	///     Gets whether the overwrite targets a member.
	/// </summary>
	public bool TargetsMember
		=> this.OverwriteTargetType is AuditLogOverwriteTargetType.Member;
}

/// <summary>
///     Represents a member audit log entry.
/// </summary>
public sealed class DiscordMemberAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted member.
	/// </summary>
	public DiscordMember? TargetMember { get; internal set; }

	/// <summary>
	///     Gets the channel associated with the member action, if Discord supplied one.
	/// </summary>
	public DiscordChannel? Channel { get; internal set; }

	/// <summary>
	///     Gets the number of targeted members for move or disconnect actions.
	/// </summary>
	public int? Count { get; internal set; }

	/// <summary>
	///     Gets the integration type associated with the member action, if Discord supplied one.
	/// </summary>
	public string? IntegrationType { get; internal set; }

	/// <summary>
	///     Gets the raw role additions attached to a member role update entry.
	/// </summary>
	/// <remarks>
	///     Discord exposes member role deltas using the special <c>$add</c> change key.
	/// </remarks>
	public DiscordAuditLogChange? AddedRolesChange
		=> this.GetChange("$add");

	/// <summary>
	///     Gets the raw role removals attached to a member role update entry.
	/// </summary>
	/// <remarks>
	///     Discord exposes member role deltas using the special <c>$remove</c> change key.
	/// </remarks>
	public DiscordAuditLogChange? RemovedRolesChange
		=> this.GetChange("$remove");

	/// <summary>
	///     Gets the typed member change helper view.
	/// </summary>
	public DiscordMemberAuditLogChanges ChangeSet
		=> new(this);

	/// <summary>
	///     Gets the typed partial role collection added to the member, if present.
	/// </summary>
	public IReadOnlyList<DiscordAuditLogPartialRole>? AddedRoles
		=> this.ChangeSet.AddedRoles?.After;

	/// <summary>
	///     Gets the typed partial role collection removed from the member, if present.
	/// </summary>
	public IReadOnlyList<DiscordAuditLogPartialRole>? RemovedRoles
		=> this.ChangeSet.RemovedRoles?.After;
}

/// <summary>
///     Represents a role audit log entry.
/// </summary>
public sealed class DiscordRoleAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted role.
	/// </summary>
	public DiscordRole? TargetRole { get; internal set; }

	/// <summary>
	///     Gets the typed role change helper view.
	/// </summary>
	public DiscordRoleAuditLogChanges ChangeSet
		=> new(this);
}

/// <summary>
///     Represents an invite audit log entry.
/// </summary>
public sealed class DiscordInviteAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted invite code.
	/// </summary>
	public string? Code { get; internal set; }

	/// <summary>
	///     Gets the channel referenced by the entry.
	/// </summary>
	public DiscordChannel? Channel { get; internal set; }
}

/// <summary>
///     Represents a webhook audit log entry.
/// </summary>
public sealed class DiscordWebhookAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted webhook.
	/// </summary>
	public DiscordWebhook? TargetWebhook { get; internal set; }
}

/// <summary>
///     Represents an emoji audit log entry.
/// </summary>
public sealed class DiscordEmojiAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted emoji id.
	/// </summary>
	public ulong? EmojiId { get; internal set; }

	/// <summary>
	///     Gets the targeted emoji when the reference has been hydrated.
	/// </summary>
	public DiscordGuildEmoji? TargetEmoji { get; internal set; }
}

/// <summary>
///     Represents a sticker audit log entry.
/// </summary>
public sealed class DiscordStickerAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted sticker id.
	/// </summary>
	public ulong? StickerId { get; internal set; }

	/// <summary>
	///     Gets the targeted sticker when the reference has been hydrated.
	/// </summary>
	public DiscordSticker? TargetSticker { get; internal set; }
}

/// <summary>
///     Represents a message audit log entry.
/// </summary>
public sealed class DiscordMessageAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the channel.
	/// </summary>
	public DiscordChannel? Channel { get; internal set; }

	/// <summary>
	///     Gets the member targeted by the message action, if Discord supplied a user target id.
	/// </summary>
	/// <remarks>
	///     For message deletion actions this usually represents the author whose messages were affected. For pin and
	///     unpin actions this typically represents the author of the pinned message.
	/// </remarks>
	public DiscordMember? TargetMember { get; internal set; }

	/// <summary>
	///     Gets the message id.
	/// </summary>
	public ulong? MessageId { get; internal set; }

	/// <summary>
	///     Gets the count.
	/// </summary>
	public int? Count { get; internal set; }

	/// <summary>
	///     Gets the typed message change helper view.
	/// </summary>
	public DiscordMessageAuditLogChanges ChangeSet
		=> new(this);

	/// <summary>
	///     Gets the number of affected messages when Discord reports one.
	/// </summary>
	public int? AffectedMessageCount
		=> this.Count;

	/// <summary>
	///     Gets the targeted message id when Discord reports one.
	/// </summary>
	public ulong? TargetMessageId
		=> this.MessageId;

	/// <summary>
	///     Gets whether the entry represents a bulk delete action.
	/// </summary>
	public bool IsBulkDeleteAction
		=> this.ActionType is AuditLogActionType.MessageBulkDelete;

	/// <summary>
	///     Gets whether the entry represents a pin or unpin action.
	/// </summary>
	public bool IsPinAction
		=> this.ActionType is AuditLogActionType.MessagePin or AuditLogActionType.MessageUnpin;
}

/// <summary>
///     Represents an integration audit log entry.
/// </summary>
public sealed class DiscordIntegrationAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the integration id.
	/// </summary>
	public ulong? IntegrationId { get; internal set; }

	/// <summary>
	///     Gets the integration type.
	/// </summary>
	public string? IntegrationType { get; internal set; }

	/// <summary>
	///     Gets the targeted integration when the reference has been hydrated.
	/// </summary>
	public DiscordIntegration? TargetIntegration { get; internal set; }
}

/// <summary>
///     Represents a stage instance audit log entry.
/// </summary>
public sealed class DiscordStageInstanceAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the stage instance id.
	/// </summary>
	public ulong? StageInstanceId { get; internal set; }

	/// <summary>
	///     Gets the related channel.
	/// </summary>
	public DiscordChannel? Channel { get; internal set; }

	/// <summary>
	///     Gets the targeted stage instance when the reference has been hydrated.
	/// </summary>
	public DiscordStageInstance? TargetStageInstance { get; internal set; }
}

/// <summary>
///     Represents a guild scheduled event audit log entry.
/// </summary>
public sealed class DiscordGuildScheduledEventAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted guild scheduled event.
	/// </summary>
	public DiscordScheduledEvent? TargetGuildScheduledEvent { get; internal set; }
}

/// <summary>
///     Represents a guild scheduled event exception audit log entry.
/// </summary>
public sealed class DiscordGuildScheduledEventExceptionAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the guild scheduled event the exception belongs to.
	/// </summary>
	public DiscordScheduledEvent? TargetGuildScheduledEvent { get; internal set; }

	/// <summary>
	///     Gets the guild scheduled event exception id reported by Discord.
	/// </summary>
	public ulong? GuildScheduledEventExceptionId { get; internal set; }
}

/// <summary>
///     Represents a thread audit log entry.
/// </summary>
public sealed class DiscordThreadAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the targeted thread.
	/// </summary>
	public DiscordChannel? TargetThread { get; internal set; }

	/// <summary>
	///     Gets the thread name change, if present.
	/// </summary>
	public DiscordAuditLogChange? NameChange
		=> this.GetChange("name");

	/// <summary>
	///     Gets the thread archived-state change, if present.
	/// </summary>
	public DiscordAuditLogChange? ArchivedChange
		=> this.GetChange("archived");

	/// <summary>
	///     Gets the thread locked-state change, if present.
	/// </summary>
	public DiscordAuditLogChange? LockedChange
		=> this.GetChange("locked");

	/// <summary>
	///     Gets the thread auto archive duration change, if present.
	/// </summary>
	public DiscordAuditLogChange? AutoArchiveDurationChange
		=> this.GetChange("auto_archive_duration");

	/// <summary>
	///     Gets the typed thread change helper view.
	/// </summary>
	public DiscordThreadAuditLogChanges ChangeSet
		=> new(this);
}

/// <summary>
///     Represents an application command audit log entry.
/// </summary>
public sealed class DiscordApplicationCommandAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the application id.
	/// </summary>
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	///     Gets the command id.
	/// </summary>
	public ulong? CommandId { get; internal set; }

	/// <summary>
	///     Gets the targeted application command when the reference has been hydrated.
	/// </summary>
	public DiscordApplicationCommand? TargetCommand { get; internal set; }
}

/// <summary>
///     Represents a soundboard sound audit log entry.
/// </summary>
public sealed class DiscordSoundboardSoundAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the soundboard sound id.
	/// </summary>
	public ulong? SoundboardSoundId { get; internal set; }

	/// <summary>
	///     Gets the targeted soundboard sound when the reference has been hydrated.
	/// </summary>
	public DiscordSoundboardSound? TargetSoundboardSound { get; internal set; }
}

/// <summary>
///     Represents an auto moderation audit log entry.
/// </summary>
public sealed class DiscordAutoModerationRuleAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the auto moderation rule id.
	/// </summary>
	/// <remarks>
	///     This property is typically populated for rule create, update, and delete actions. Execution actions such as
	///     message blocking or user timeouts instead use <see cref="TargetMember" /> as their primary target.
	/// </remarks>
	public ulong? AutoModerationRuleId { get; internal set; }

	/// <summary>
	///     Gets the rule name.
	/// </summary>
	public string? RuleName { get; internal set; }

	/// <summary>
	///     Gets the rule trigger type reported by Discord.
	/// </summary>
	public string? TriggerType { get; internal set; }

	/// <summary>
	///     Gets the channel associated with the auto moderation action, if Discord supplied one.
	/// </summary>
	public DiscordChannel? Channel { get; internal set; }

	/// <summary>
	///     Gets the member targeted by an auto moderation execution action.
	/// </summary>
	/// <remarks>
	///     This property is usually populated for actions such as blocking, flagging, timing out, or quarantining a user.
	/// </remarks>
	public DiscordMember? TargetMember { get; internal set; }

	/// <summary>
	///     Gets the typed auto moderation change helper view.
	/// </summary>
	public DiscordAutoModerationRuleAuditLogChanges ChangeSet
		=> new(this);

	/// <summary>
	///     Gets whether the entry represents an auto moderation execution action.
	/// </summary>
	public bool IsExecutionAction
		=> this.ActionType is AuditLogActionType.AutoModerationBlockMessage
			or AuditLogActionType.AutoModerationFlagMessage
			or AuditLogActionType.AutoModerationTimeOutUser
			or AuditLogActionType.AutoModerationQuarantineUser;

	/// <summary>
	///     Gets whether the entry represents a rule create, update, or delete mutation action.
	/// </summary>
	public bool IsRuleMutationAction
		=> !this.IsExecutionAction;
}

/// <summary>
///     Represents a creator monetization audit log entry.
/// </summary>
/// <remarks>
///     Creator monetization audit log actions are currently guild-scoped system events without a dedicated target
///     object, so the guild itself is the only stable typed reference exposed by the library.
/// </remarks>
public sealed class DiscordCreatorMonetizationAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the guild whose creator monetization state changed.
	/// </summary>
	public DiscordGuild TargetGuild
		=> this.Guild;
}

/// <summary>
///     Represents an onboarding audit log entry.
/// </summary>
public sealed class DiscordOnboardingAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the onboarding id.
	/// </summary>
	public ulong? OnboardingId { get; internal set; }
}

/// <summary>
///     Represents a server guide audit log entry.
/// </summary>
public sealed class DiscordServerGuideAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the home settings id.
	/// </summary>
	public ulong? HomeSettingsId { get; internal set; }
}

/// <summary>
///     Represents a voice channel status audit log entry.
/// </summary>
public sealed class DiscordVoiceChannelStatusAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the channel.
	/// </summary>
	public DiscordChannel? Channel { get; internal set; }

	/// <summary>
	///     Gets the voice channel status.
	/// </summary>
	public string? Status { get; internal set; }
}

/// <summary>
///     Represents a guild profile audit log entry.
/// </summary>
public sealed class DiscordGuildProfileAuditLogEntry : DiscordAuditLogEntry;

/// <summary>
///     Represents a member verification audit log entry.
/// </summary>
/// <remarks>
///     Member verification settings are guild-scoped, so these entries generally describe verification form or
///     descriptive-text changes rather than a separate target snowflake.
/// </remarks>
public sealed class DiscordMemberVerificationAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the guild whose member verification settings were updated.
	/// </summary>
	public DiscordGuild TargetGuild
		=> this.Guild;
}

/// <summary>
///     Represents a permission migration completion audit log entry.
/// </summary>
/// <remarks>
///     Discord emits these system entries when an older permission semantic is migrated to a newer dedicated one.
/// </remarks>
public sealed class DiscordPermissionMigrationAuditLogEntry : DiscordAuditLogEntry
{
	/// <summary>
	///     Gets the legacy permissions that were migrated away from.
	/// </summary>
	/// <remarks>
	///     Some migration entries collapse more than one legacy permission into a single new permission.
	/// </remarks>
	public IReadOnlyList<Permissions> LegacyPermissions { get; internal set; } = [];

	/// <summary>
	///     Gets the replacement permission introduced by Discord.
	/// </summary>
	public Permissions ReplacementPermission { get; internal set; }

	/// <summary>
	///     Gets the single legacy permission when the migration originated from exactly one permission.
	/// </summary>
	/// <remarks>
	///     This convenience accessor returns <see langword="null" /> for multi-source migrations such as bypass
	///     slowmode.
	/// </remarks>
	public Permissions? LegacyPermission
		=> this.LegacyPermissions.Count is 1 ? this.LegacyPermissions[0] : null;
}

/// <summary>
///     Represents an unknown or raw audit log entry.
/// </summary>
public sealed class DiscordRawAuditLogEntry : DiscordAuditLogEntry;

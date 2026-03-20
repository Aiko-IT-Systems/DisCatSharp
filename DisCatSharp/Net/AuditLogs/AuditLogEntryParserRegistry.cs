using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.AuditLogs;

/// <summary>
///     Maps raw Discord audit log actions to their corresponding DisCatSharp entry families.
/// </summary>
/// <remarks>
///     The registry is shared by both REST and gateway parsing so audit log behavior stays consistent regardless of
///     where the payload originated. Unknown or currently unsupported actions intentionally fall back to
///     <see cref="DiscordRawAuditLogEntry"/> instead of throwing.
/// </remarks>
internal sealed class AuditLogEntryParserRegistry
{
	/// <summary>
	///     Stores the parser delegate for each supported audit log action type.
	/// </summary>
	private readonly Dictionary<AuditLogActionType, Func<DiscordGuild, AuditLogReferenceStore, RawAuditLogEntry, DiscordAuditLogEntry>> _parsers;

	/// <summary>
	///     Gets the shared audit log parser registry instance.
	/// </summary>
	public static AuditLogEntryParserRegistry Instance { get; } = new();

	/// <summary>
	///     Initializes a new instance of the <see cref="AuditLogEntryParserRegistry"/> class.
	/// </summary>
	/// <remarks>
	///     The mapping is intentionally family-based rather than action-per-class so the public model can stay small while
	///     still exposing typed targets and options.
	/// </remarks>
	private AuditLogEntryParserRegistry()
	{
		this._parsers = new()
		{
			[AuditLogActionType.GuildUpdate] = static (guild, _, _) => new DiscordGuildAuditLogEntry
			{
				TargetGuild = guild
			},
			[AuditLogActionType.ChannelCreate] = ParseChannelEntry,
			[AuditLogActionType.ChannelUpdate] = ParseChannelEntry,
			[AuditLogActionType.ChannelDelete] = ParseChannelEntry,
			[AuditLogActionType.OverwriteCreate] = ParseOverwriteEntry,
			[AuditLogActionType.OverwriteUpdate] = ParseOverwriteEntry,
			[AuditLogActionType.OverwriteDelete] = ParseOverwriteEntry,
			[AuditLogActionType.Kick] = ParseMemberEntry,
			[AuditLogActionType.Prune] = ParseMemberEntry,
			[AuditLogActionType.Ban] = ParseMemberEntry,
			[AuditLogActionType.Unban] = ParseMemberEntry,
			[AuditLogActionType.MemberUpdate] = ParseMemberEntry,
			[AuditLogActionType.MemberRoleUpdate] = ParseMemberEntry,
			[AuditLogActionType.MemberMove] = ParseMemberEntry,
			[AuditLogActionType.MemberDisconnect] = ParseMemberEntry,
			[AuditLogActionType.BotAdd] = ParseMemberEntry,
			[AuditLogActionType.RoleCreate] = ParseRoleEntry,
			[AuditLogActionType.RoleUpdate] = ParseRoleEntry,
			[AuditLogActionType.RoleDelete] = ParseRoleEntry,
			[AuditLogActionType.InviteCreate] = ParseInviteEntry,
			[AuditLogActionType.InviteUpdate] = ParseInviteEntry,
			[AuditLogActionType.InviteDelete] = ParseInviteEntry,
			[AuditLogActionType.WebhookCreate] = ParseWebhookEntry,
			[AuditLogActionType.WebhookUpdate] = ParseWebhookEntry,
			[AuditLogActionType.WebhookDelete] = ParseWebhookEntry,
			[AuditLogActionType.EmojiCreate] = ParseEmojiEntry,
			[AuditLogActionType.EmojiUpdate] = ParseEmojiEntry,
			[AuditLogActionType.EmojiDelete] = ParseEmojiEntry,
			[AuditLogActionType.StickerCreate] = ParseStickerEntry,
			[AuditLogActionType.StickerUpdate] = ParseStickerEntry,
			[AuditLogActionType.StickerDelete] = ParseStickerEntry,
			[AuditLogActionType.MessageDelete] = ParseMessageEntry,
			[AuditLogActionType.MessageBulkDelete] = ParseMessageEntry,
			[AuditLogActionType.MessagePin] = ParseMessageEntry,
			[AuditLogActionType.MessageUnpin] = ParseMessageEntry,
			[AuditLogActionType.IntegrationCreate] = ParseIntegrationEntry,
			[AuditLogActionType.IntegrationUpdate] = ParseIntegrationEntry,
			[AuditLogActionType.IntegrationDelete] = ParseIntegrationEntry,
			[AuditLogActionType.StageInstanceCreate] = ParseStageEntry,
			[AuditLogActionType.StageInstanceUpdate] = ParseStageEntry,
			[AuditLogActionType.StageInstanceDelete] = ParseStageEntry,
			[AuditLogActionType.GuildScheduledEventCreate] = ParseScheduledEventEntry,
			[AuditLogActionType.GuildScheduledEventUpdate] = ParseScheduledEventEntry,
			[AuditLogActionType.GuildScheduledEventDelete] = ParseScheduledEventEntry,
			[AuditLogActionType.GuildScheduledEventExceptionCreate] = ParseScheduledEventExceptionEntry,
			[AuditLogActionType.GuildScheduledEventExceptionUpdate] = ParseScheduledEventExceptionEntry,
			[AuditLogActionType.GuildScheduledEventExceptionDelete] = ParseScheduledEventExceptionEntry,
			[AuditLogActionType.ThreadCreate] = ParseThreadEntry,
			[AuditLogActionType.ThreadUpdate] = ParseThreadEntry,
			[AuditLogActionType.ThreadDelete] = ParseThreadEntry,
			[AuditLogActionType.ApplicationCommandPermissionUpdate] = ParseApplicationCommandEntry,
			[AuditLogActionType.SoundboardSoundCreate] = ParseSoundboardEntry,
			[AuditLogActionType.SoundboardSoundUpdate] = ParseSoundboardEntry,
			[AuditLogActionType.SoundboardSoundDelete] = ParseSoundboardEntry,
			[AuditLogActionType.AutoModerationRuleCreate] = ParseAutoModerationEntry,
			[AuditLogActionType.AutoModerationRuleUpdate] = ParseAutoModerationEntry,
			[AuditLogActionType.AutoModerationRuleDelete] = ParseAutoModerationEntry,
			[AuditLogActionType.AutoModerationBlockMessage] = ParseAutoModerationEntry,
			[AuditLogActionType.AutoModerationFlagMessage] = ParseAutoModerationEntry,
			[AuditLogActionType.AutoModerationTimeOutUser] = ParseAutoModerationEntry,
			[AuditLogActionType.AutoModerationQuarantineUser] = ParseAutoModerationEntry,
			[AuditLogActionType.CreatorMonetizationRequestCreated] = static (_, _, _) => new DiscordCreatorMonetizationAuditLogEntry(),
			[AuditLogActionType.CreatorMonetizationTermsAccepted] = static (_, _, _) => new DiscordCreatorMonetizationAuditLogEntry(),
			[AuditLogActionType.OnboardingPromptCreate] = ParseOnboardingEntry,
			[AuditLogActionType.OnboardingPromptUpdate] = ParseOnboardingEntry,
			[AuditLogActionType.OnboardingPromptDelete] = ParseOnboardingEntry,
			[AuditLogActionType.OnboardingCreate] = ParseOnboardingEntry,
			[AuditLogActionType.OnboardingUpdate] = ParseOnboardingEntry,
			[AuditLogActionType.ServerGuideCreate] = ParseServerGuideEntry,
			[AuditLogActionType.ServerGuideUpdate] = ParseServerGuideEntry,
			[AuditLogActionType.VoiceChannelStatusCreate] = ParseVoiceChannelStatusEntry,
			[AuditLogActionType.VoiceChannelStatusDelete] = ParseVoiceChannelStatusEntry,
			[AuditLogActionType.GuildMemberVerificationUpdate] = static (_, _, _) => new DiscordMemberVerificationAuditLogEntry(),
			[AuditLogActionType.GuildProfileUpdate] = static (_, _, _) => new DiscordGuildProfileAuditLogEntry(),
			[AuditLogActionType.GuildMigratePinPermission] = ParsePermissionMigrationEntry,
			[AuditLogActionType.GuildMigrateBypassSlowmodePermission] = ParsePermissionMigrationEntry
		};
	}

	/// <summary>
	///     Parses a raw audit log page into DisCatSharp audit log entry objects.
	/// </summary>
	/// <param name="guild">The guild that owns the audit log page.</param>
	/// <param name="rawAuditLog">The raw audit log payload returned by Discord.</param>
	/// <param name="isAscending">
	///     Whether the entries should be treated as ascending according to the query that produced the page.
	/// </param>
	/// <returns>A parsed audit log page.</returns>
	public DiscordAuditLogPage ParsePage(DiscordGuild guild, RawAuditLog rawAuditLog, bool isAscending)
	{
		var references = new AuditLogReferenceStore(guild, rawAuditLog);
		var entries = rawAuditLog.Entries.Select(entry => this.ParseEntry(guild, references, entry)).ToArray();
		return new()
		{
			Entries = entries,
			IsAscending = isAscending,
			FirstEntryId = entries.FirstOrDefault()?.Id,
			LastEntryId = entries.LastOrDefault()?.Id
		};
	}

	/// <summary>
	///     Parses a single raw audit log entry into its public entry representation.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The prebuilt reference store for resolving side-loaded objects.</param>
	/// <param name="entry">The raw entry to parse.</param>
	/// <returns>
	///     A typed audit log entry when a dedicated family is known; otherwise a <see cref="DiscordRawAuditLogEntry"/>.
	/// </returns>
	/// <remarks>
	///     This method never performs REST calls. Missing references are represented as partial objects or left unset.
	/// </remarks>
	public DiscordAuditLogEntry ParseEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
	{
		var parsedEntry = this._parsers.TryGetValue(entry.ActionType, out var parser)
			? parser(guild, references, entry)
			: new DiscordRawAuditLogEntry();

		var changes = entry.Changes?.Select(change => new DiscordAuditLogChange(change.Key, change.OldValue?.DeepClone(), change.NewValue?.DeepClone())).ToArray()
			?? [];

		parsedEntry.Discord = guild.Discord;
		parsedEntry.Id = entry.Id;
		parsedEntry.Guild = guild;
		parsedEntry.ActionType = entry.ActionType;
		parsedEntry.ActionCategory = GetActionCategory(entry.ActionType);
		parsedEntry.TargetId = entry.TargetId;
		parsedEntry.Actor = references.ResolveActor(entry.UserId);
		parsedEntry.Reason = entry.Reason;
		parsedEntry.Changes = changes;
		parsedEntry.RawChanges = [.. changes.Select(static change => change.ToRawObject())];
		parsedEntry.Options = ParseOptions(entry.Options);
		parsedEntry.RawOptions = parsedEntry.Options?.RawObject;

		return parsedEntry;
	}

	/// <summary>
	///     Converts raw audit log entry options into the public typed options object.
	/// </summary>
	/// <param name="options">The raw options object.</param>
	/// <returns>
	///     A typed options object when options are present; otherwise <see langword="null"/>.
	/// </returns>
	/// <remarks>
	///     Discord serializes some numeric values in this object as strings, so those fields are normalized here using
	///     invariant parsing. Unknown fields are preserved inside <see cref="DiscordAuditLogEntryOptions.RawObject"/>.
	/// </remarks>
	private static DiscordAuditLogEntryOptions? ParseOptions(RawAuditLogEntryOptions? options)
	{
		if (options is null)
			return null;

		var rawObject = new JObject();
		void Set(string key, JToken? value)
		{
			if (value is not null)
				rawObject[key] = value;
		}

		Set("application_id", options.ApplicationId);
		Set("auto_moderation_rule_name", options.AutoModerationRuleName);
		Set("auto_moderation_rule_trigger_type", options.AutoModerationRuleTriggerType);
		Set("channel_id", options.ChannelId);
		Set("count", options.Count);
		Set("delete_member_days", options.DeleteMemberDays);
		Set("event_exception_id", options.EventExceptionId);
		Set("id", options.Id);
		Set("members_removed", options.MembersRemoved);
		Set("message_id", options.MessageId);
		Set("role_name", options.RoleName);
		Set("status", options.Status);
		Set("type", options.Type);
		Set("integration_type", options.IntegrationType);
		if (options.AdditionalData is not null)
			foreach (var pair in options.AdditionalData)
				rawObject[pair.Key] = pair.Value.DeepClone();

		return new()
		{
			ApplicationId = options.ApplicationId,
			AutoModerationRuleName = options.AutoModerationRuleName,
			AutoModerationRuleTriggerType = options.AutoModerationRuleTriggerType,
			ChannelId = options.ChannelId,
			Count = ParseInt(options.Count),
			DeleteMemberDays = ParseInt(options.DeleteMemberDays),
			EventExceptionId = options.EventExceptionId,
			Id = options.Id,
			MembersRemoved = ParseInt(options.MembersRemoved),
			MessageId = options.MessageId,
			RoleName = options.RoleName,
			Status = options.Status,
			Type = options.Type,
			IntegrationType = options.IntegrationType,
			RawObject = rawObject
		};
	}

	/// <summary>
	///     Parses a channel-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed channel audit log entry.</returns>
	private static DiscordChannelAuditLogEntry ParseChannelEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			TargetChannel = references.ResolveChannel(ParseSnowflake(entry.TargetId))
		};

	/// <summary>
	///     Parses an overwrite-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed overwrite audit log entry.</returns>
	/// <remarks>
	///     Discord stores overwrite target details inside the options object rather than inside the entry target id.
	/// </remarks>
	private static DiscordOverwriteAuditLogEntry ParseOverwriteEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			Channel = references.ResolveChannel(entry.Options?.ChannelId),
			OverwrittenEntityId = entry.Options?.Id,
			OverwrittenEntityType = entry.Options?.Type,
			RoleName = entry.Options?.RoleName
		};

	/// <summary>
	///     Parses a member-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed member audit log entry.</returns>
	private static DiscordMemberAuditLogEntry ParseMemberEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			TargetMember = references.ResolveMember(ParseSnowflake(entry.TargetId)),
			Channel = references.ResolveChannel(entry.Options?.ChannelId),
			Count = ParseInt(entry.Options?.Count),
			IntegrationType = entry.Options?.IntegrationType
		};

	/// <summary>
	///     Parses a role-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed role audit log entry.</returns>
	private static DiscordRoleAuditLogEntry ParseRoleEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			TargetRole = references.ResolveRole(ParseSnowflake(entry.TargetId))
		};

	/// <summary>
	///     Parses an invite-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed invite audit log entry.</returns>
	/// <remarks>
	///     Invite actions use the invite code as the raw target id rather than a snowflake.
	/// </remarks>
	private static DiscordInviteAuditLogEntry ParseInviteEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			Code = entry.TargetId,
			Channel = references.ResolveChannel(entry.Options?.ChannelId)
		};

	/// <summary>
	///     Parses a webhook-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed webhook audit log entry.</returns>
	private static DiscordWebhookAuditLogEntry ParseWebhookEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			TargetWebhook = references.ResolveWebhook(ParseSnowflake(entry.TargetId))
		};

	/// <summary>
	///     Parses an emoji-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed emoji audit log entry.</returns>
	private static DiscordEmojiAuditLogEntry ParseEmojiEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			EmojiId = ParseSnowflake(entry.TargetId)
		};

	/// <summary>
	///     Parses a sticker-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed sticker audit log entry.</returns>
	private static DiscordStickerAuditLogEntry ParseStickerEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			StickerId = ParseSnowflake(entry.TargetId)
		};

	/// <summary>
	///     Parses a message-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed message audit log entry.</returns>
	/// <remarks>
	///     Message-related audit log actions often carry their useful payload inside the options object and may have no
	///     change array at all.
	/// </remarks>
	private static DiscordMessageAuditLogEntry ParseMessageEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			Channel = references.ResolveChannel(entry.Options?.ChannelId),
			TargetMember = references.ResolveMember(ParseSnowflake(entry.TargetId)),
			MessageId = entry.Options?.MessageId,
			Count = ParseInt(entry.Options?.Count)
		};

	/// <summary>
	///     Parses an integration-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed integration audit log entry.</returns>
	private static DiscordIntegrationAuditLogEntry ParseIntegrationEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			IntegrationId = ParseSnowflake(entry.TargetId),
			IntegrationType = entry.Options?.IntegrationType
		};

	/// <summary>
	///     Parses a stage instance-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed stage instance audit log entry.</returns>
	private static DiscordStageInstanceAuditLogEntry ParseStageEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			StageInstanceId = ParseSnowflake(entry.TargetId),
			Channel = references.ResolveChannel(entry.Options?.ChannelId)
		};

	/// <summary>
	///     Parses a guild scheduled event-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed guild scheduled event audit log entry.</returns>
	private static DiscordGuildScheduledEventAuditLogEntry ParseScheduledEventEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			TargetGuildScheduledEvent = references.ResolveScheduledEvent(ParseSnowflake(entry.TargetId))
		};

	/// <summary>
	///     Parses a guild scheduled event exception-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed guild scheduled event exception audit log entry.</returns>
	/// <remarks>
	///     Discord currently reports the parent scheduled event as the raw target id and exposes the exception id in the
	///     options object via <c>event_exception_id</c>. The exception id itself is documented as not being globally
	///     unique, so the parent scheduled event remains the primary reference for this family.
	/// </remarks>
	private static DiscordGuildScheduledEventExceptionAuditLogEntry ParseScheduledEventExceptionEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			TargetGuildScheduledEvent = references.ResolveScheduledEvent(ParseSnowflake(entry.TargetId)),
			GuildScheduledEventExceptionId = entry.Options?.EventExceptionId
		};

	/// <summary>
	///     Parses a thread-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed thread audit log entry.</returns>
	private static DiscordThreadAuditLogEntry ParseThreadEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			TargetThread = references.ResolveChannel(ParseSnowflake(entry.TargetId))
		};

	/// <summary>
	///     Parses an application command-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed application command audit log entry.</returns>
	private static DiscordApplicationCommandAuditLogEntry ParseApplicationCommandEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			ApplicationId = entry.Options?.ApplicationId,
			CommandId = ParseSnowflake(entry.TargetId)
		};

	/// <summary>
	///     Parses a soundboard sound-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed soundboard sound audit log entry.</returns>
	private static DiscordSoundboardSoundAuditLogEntry ParseSoundboardEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			SoundboardSoundId = ParseSnowflake(entry.TargetId)
		};

	/// <summary>
	///     Parses an auto moderation-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed auto moderation audit log entry.</returns>
	/// <remarks>
	///     Several auto moderation actions are options-driven in real payloads, so the entry intentionally keeps the
	///     generic change list and exposes the stable rule metadata separately. Execution actions target members,
	///     whereas rule create, update, and delete actions target the rule id itself.
	/// </remarks>
	private static DiscordAutoModerationRuleAuditLogEntry ParseAutoModerationEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			AutoModerationRuleId = IsAutoModerationExecutionAction(entry.ActionType) ? null : ParseSnowflake(entry.TargetId),
			RuleName = entry.Options?.AutoModerationRuleName,
			TriggerType = entry.Options?.AutoModerationRuleTriggerType,
			Channel = references.ResolveChannel(entry.Options?.ChannelId),
			TargetMember = IsAutoModerationExecutionAction(entry.ActionType)
				? references.ResolveMember(ParseSnowflake(entry.TargetId))
				: null
		};

	/// <summary>
	///     Gets whether the action type represents an auto moderation execution event rather than a rule mutation event.
	/// </summary>
	/// <param name="actionType">The action type to inspect.</param>
	/// <returns><see langword="true"/> when the action targets a member execution; otherwise <see langword="false"/>.</returns>
	private static bool IsAutoModerationExecutionAction(AuditLogActionType actionType)
		=> actionType is AuditLogActionType.AutoModerationBlockMessage
			or AuditLogActionType.AutoModerationFlagMessage
			or AuditLogActionType.AutoModerationTimeOutUser
			or AuditLogActionType.AutoModerationQuarantineUser;

	/// <summary>
	///     Parses an onboarding-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed onboarding audit log entry.</returns>
	private static DiscordOnboardingAuditLogEntry ParseOnboardingEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			OnboardingId = ParseSnowflake(entry.TargetId)
		};

	/// <summary>
	///     Parses a server guide or home settings-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed server guide audit log entry.</returns>
	private static DiscordServerGuideAuditLogEntry ParseServerGuideEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			HomeSettingsId = ParseSnowflake(entry.TargetId)
		};

	/// <summary>
	///     Parses a voice channel status-focused audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed voice channel status audit log entry.</returns>
	/// <remarks>
	///     Datamined and live payloads show the channel id can appear either as the target id or inside the options
	///     object, so both locations are checked.
	/// </remarks>
	private static DiscordVoiceChannelStatusAuditLogEntry ParseVoiceChannelStatusEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> new()
		{
			Channel = references.ResolveChannel(ParseSnowflake(entry.TargetId) ?? entry.Options?.ChannelId),
			Status = entry.Options?.Status
		};

	/// <summary>
	///     Parses a permission migration audit log entry.
	/// </summary>
	/// <param name="guild">The guild that owns the entry.</param>
	/// <param name="references">The reference resolver for side-loaded objects.</param>
	/// <param name="entry">The raw entry being parsed.</param>
	/// <returns>A typed permission migration audit log entry.</returns>
	/// <remarks>
	///     Discord currently emits permission migration entries as sparse system events without richer targets. The
	///     semantic value is the permission transition itself, which is stable even when the raw entry has no changes or
	///     options attached.
	/// </remarks>
	private static DiscordPermissionMigrationAuditLogEntry ParsePermissionMigrationEntry(DiscordGuild guild, AuditLogReferenceStore references, RawAuditLogEntry entry)
		=> entry.ActionType switch
		{
			AuditLogActionType.GuildMigratePinPermission => new DiscordPermissionMigrationAuditLogEntry
			{
				LegacyPermissions = [Permissions.ManageMessages],
				ReplacementPermission = Permissions.PinMessages
			},
			AuditLogActionType.GuildMigrateBypassSlowmodePermission => new DiscordPermissionMigrationAuditLogEntry
			{
				LegacyPermissions = [Permissions.ManageMessages, Permissions.ManageChannels, Permissions.ManageThreads],
				ReplacementPermission = Permissions.BypassSlowmode
			},
			_ => new DiscordPermissionMigrationAuditLogEntry()
		};

	/// <summary>
	///     Parses a raw snowflake string into an unsigned integer identifier.
	/// </summary>
	/// <param name="value">The raw string value returned by Discord.</param>
	/// <returns>The parsed identifier, or <see langword="null"/> when the value is absent or malformed.</returns>
	private static ulong? ParseSnowflake(string? value)
		=> ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : null;

	/// <summary>
	///     Parses a numeric audit log option value into an integer.
	/// </summary>
	/// <param name="value">The raw string value returned by Discord.</param>
	/// <returns>The parsed number, or <see langword="null"/> when the value is absent or malformed.</returns>
	private static int? ParseInt(string? value)
		=> int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : null;

	/// <summary>
	///     Maps an audit log action type to its broad action category.
	/// </summary>
	/// <param name="actionType">The action type to classify.</param>
	/// <returns>The corresponding action category.</returns>
	/// <remarks>
	///     Discord does not provide the category directly, so the mapping is maintained by the library for convenience.
	/// </remarks>
	private static AuditLogActionCategory GetActionCategory(AuditLogActionType actionType)
		=> actionType switch
		{
			AuditLogActionType.ChannelCreate
				or AuditLogActionType.EmojiCreate
				or AuditLogActionType.InviteCreate
				or AuditLogActionType.OverwriteCreate
				or AuditLogActionType.RoleCreate
				or AuditLogActionType.WebhookCreate
				or AuditLogActionType.IntegrationCreate
				or AuditLogActionType.StickerCreate
				or AuditLogActionType.StageInstanceCreate
				or AuditLogActionType.ThreadCreate
				or AuditLogActionType.GuildScheduledEventCreate
				or AuditLogActionType.AutoModerationRuleCreate
				or AuditLogActionType.OnboardingPromptCreate
				or AuditLogActionType.SoundboardSoundCreate
				or AuditLogActionType.GuildScheduledEventExceptionCreate
				or AuditLogActionType.OnboardingCreate
				or AuditLogActionType.VoiceChannelStatusCreate
				or AuditLogActionType.ServerGuideCreate => AuditLogActionCategory.Create,
			AuditLogActionType.ChannelDelete
				or AuditLogActionType.EmojiDelete
				or AuditLogActionType.InviteDelete
				or AuditLogActionType.MessageDelete
				or AuditLogActionType.MessageBulkDelete
				or AuditLogActionType.OverwriteDelete
				or AuditLogActionType.RoleDelete
				or AuditLogActionType.WebhookDelete
				or AuditLogActionType.IntegrationDelete
				or AuditLogActionType.StickerDelete
				or AuditLogActionType.StageInstanceDelete
				or AuditLogActionType.ThreadDelete
				or AuditLogActionType.GuildScheduledEventDelete
				or AuditLogActionType.OnboardingPromptDelete
				or AuditLogActionType.SoundboardSoundDelete
				or AuditLogActionType.GuildScheduledEventExceptionDelete
				or AuditLogActionType.VoiceChannelStatusDelete
				or AuditLogActionType.AutoModerationRuleDelete => AuditLogActionCategory.Delete,
			AuditLogActionType.ChannelUpdate
				or AuditLogActionType.GuildUpdate
				or AuditLogActionType.EmojiUpdate
				or AuditLogActionType.InviteUpdate
				or AuditLogActionType.MemberRoleUpdate
				or AuditLogActionType.MemberUpdate
				or AuditLogActionType.OverwriteUpdate
				or AuditLogActionType.RoleUpdate
				or AuditLogActionType.WebhookUpdate
				or AuditLogActionType.IntegrationUpdate
				or AuditLogActionType.StickerUpdate
				or AuditLogActionType.StageInstanceUpdate
				or AuditLogActionType.ThreadUpdate
				or AuditLogActionType.GuildScheduledEventUpdate
				or AuditLogActionType.AutoModerationRuleUpdate
				or AuditLogActionType.OnboardingPromptUpdate
				or AuditLogActionType.SoundboardSoundUpdate
				or AuditLogActionType.GuildScheduledEventExceptionUpdate
				or AuditLogActionType.OnboardingUpdate
				or AuditLogActionType.ServerGuideUpdate
				or AuditLogActionType.GuildMemberVerificationUpdate
				or AuditLogActionType.GuildProfileUpdate => AuditLogActionCategory.Update,
			_ => AuditLogActionCategory.Other
		};
}

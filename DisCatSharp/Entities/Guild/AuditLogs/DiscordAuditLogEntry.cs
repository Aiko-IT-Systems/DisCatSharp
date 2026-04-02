using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord audit log entry.
/// </summary>
/// <remarks>
///     The base type preserves Discord's common audit log fields for both documented and undocumented action types.
///     Dedicated subclasses add typed targets for stable entry families, while raw changes and raw options remain
///     available for forward compatibility.
/// </remarks>
public abstract class DiscordAuditLogEntry : SnowflakeObject
{
	/// <summary>
	///     Gets the guild the entry belongs to.
	/// </summary>
	public DiscordGuild Guild { get; internal set; } = null!;

	/// <summary>
	///     Gets the action type.
	/// </summary>
	public AuditLogActionType ActionType { get; internal set; }

	/// <summary>
	///     Gets the action category.
	/// </summary>
	public AuditLogActionCategory ActionCategory { get; internal set; }

	/// <summary>
	///     Gets the raw Discord target id.
	/// </summary>
	/// <remarks>
	///     Discord serializes this field as a string and may omit it for some action types.
	/// </remarks>
	public string? TargetId { get; internal set; }

	/// <summary>
	///     Gets the acting user.
	/// </summary>
	/// <remarks>
	///     If Discord does not include enough data to resolve the actor fully, this may be a partial user object.
	/// </remarks>
	public DiscordUser? Actor { get; internal set; }

	/// <summary>
	///     Gets the reason for the audit log entry.
	/// </summary>
	public string? Reason { get; internal set; }

	/// <summary>
	///     Gets the entry changes.
	/// </summary>
	/// <remarks>
	///     Some action types do not include any change objects and instead encode their useful information only in
	///     <see cref="Options" />.
	/// </remarks>
	public IReadOnlyList<DiscordAuditLogChange> Changes { get; internal set; } = [];

	/// <summary>
	///     Gets the raw change objects.
	/// </summary>
	/// <remarks>
	///     This collection preserves Discord's original change payloads for undocumented keys or advanced consumers that
	///     need the exact transport shape.
	/// </remarks>
	public IReadOnlyList<JObject> RawChanges { get; internal set; } = [];

	/// <summary>
	///     Gets the typed options object, if present.
	/// </summary>
	/// <remarks>
	///     Not every action type includes options, and Discord may add option fields before the library exposes them as
	///     dedicated properties. Unknown fields remain available through <see cref="RawOptions" />.
	/// </remarks>
	public DiscordAuditLogEntryOptions? Options { get; internal set; }

	/// <summary>
	///     Gets the raw options object.
	/// </summary>
	/// <remarks>
	///     Prefer <see cref="Options" /> for documented fields. This raw object is mainly useful when Discord adds new
	///     option members before the library exposes them as dedicated properties.
	/// </remarks>
	public JObject? RawOptions { get; internal set; }

	/// <summary>
	///     Gets whether this entry has changes.
	/// </summary>
	/// <remarks>
	///     A value of <see langword="false" /> does not imply that the entry contains no useful data. Several Discord
	///     actions place their interesting payload only in <see cref="Options" />.
	/// </remarks>
	public bool HasChanges
		=> this.Changes.Count != 0;

	/// <summary>
	///     Tries to get a change by key.
	/// </summary>
	/// <param name="key">The Discord change key to look up.</param>
	/// <param name="change">When this method returns, contains the matching change if one was found; otherwise <see langword="null" />.</param>
	/// <returns><see langword="true" /> when a matching change exists; otherwise <see langword="false" />.</returns>
	/// <remarks>
	///     The lookup is ordinal and case-sensitive to match Discord's change key behavior exactly.
	/// </remarks>
	public bool TryGetChange(string key, out DiscordAuditLogChange change)
	{
		change = this.Changes.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.Ordinal));
		return change is not null;
	}

	/// <summary>
	///     Gets a change by key, or <see langword="null" /> if not present.
	/// </summary>
	/// <param name="key">The Discord change key to look up.</param>
	/// <returns>The matching change, or <see langword="null" /> when the key does not exist.</returns>
	/// <remarks>
	///     The lookup is ordinal and case-sensitive to match Discord's change key behavior exactly.
	/// </remarks>
	public DiscordAuditLogChange? GetChange(string key)
		=> this.Changes.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.Ordinal));

	/// <summary>
	///     Replaces audit log placeholder references with cached or freshly fetched live entities.
	/// </summary>
	/// <param name="targets">Controls which reference groups should be hydrated.</param>
	/// <param name="force">
	///     Controls whether the helper may perform REST requests when the requested entities are not already cached.
	/// </param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>A task representing the asynchronous hydration operation.</returns>
	/// <remarks>
	///     <para>
	///         Audit log parsing intentionally avoids implicit REST calls and may therefore expose partial synthetic
	///         entities. Calling this method allows consumers to opt into upgrading those references after parsing.
	///     </para>
	///     <para>
	///         When <paramref name="force" /> is <see langword="false" />, only cached entities are used and no network
	///         request is issued. When it is <see langword="true" />, the helper fetches live entities for resolvable
	///         references where DisCatSharp exposes an appropriate retrieval API.
	///     </para>
	///     <para>
	///         Missing or deleted targets are treated as a best-effort miss and leave the existing partial reference in
	///         place. Other API failures still bubble to the caller.
	///     </para>
	/// </remarks>
	/// <exception cref="UnauthorizedException">Thrown when Discord denies one of the requested live fetches.</exception>
	/// <exception cref="BadRequestException">Thrown when Discord rejects one of the requested live fetches.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process one of the requested live fetches.</exception>
	public async Task HydrateAsync(AuditLogHydrationTargets targets = AuditLogHydrationTargets.All, bool force = true, CancellationToken cancellationToken = default)
	{
		if (targets is AuditLogHydrationTargets.None)
			return;

		if (ShouldHydrate(targets, AuditLogHydrationTargets.Actor) && this.Actor is { Id: not 0 })
			this.Actor = await this.TryHydrateUserAsync(this.Actor.Id, force, cancellationToken).ConfigureAwait(false) ?? this.Actor;

		switch (this)
		{
			case DiscordChannelAuditLogEntry channelEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && channelEntry.TargetChannel is { Id: not 0 })
					channelEntry.TargetChannel = await this.TryHydrateChannelAsync(channelEntry.TargetChannel.Id, force, cancellationToken).ConfigureAwait(false) ?? channelEntry.TargetChannel;
				break;

			case DiscordOverwriteAuditLogEntry overwriteEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Related) && overwriteEntry.Channel is { Id: not 0 })
					overwriteEntry.Channel = await this.TryHydrateChannelAsync(overwriteEntry.Channel.Id, force, cancellationToken).ConfigureAwait(false) ?? overwriteEntry.Channel;

				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && overwriteEntry.OverwrittenEntityId.HasValue)
				{
					if (overwriteEntry.TargetsRole)
						overwriteEntry.OverwrittenRole = await this.TryHydrateRoleAsync(overwriteEntry.OverwrittenEntityId.Value, force, cancellationToken).ConfigureAwait(false) ?? overwriteEntry.OverwrittenRole;
					else if (overwriteEntry.TargetsMember)
						overwriteEntry.OverwrittenMember = await this.TryHydrateMemberAsync(overwriteEntry.OverwrittenEntityId.Value, force, cancellationToken).ConfigureAwait(false) ?? overwriteEntry.OverwrittenMember;
				}
				break;

			case DiscordMemberAuditLogEntry memberEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && memberEntry.TargetMember is { Id: not 0 })
					memberEntry.TargetMember = await this.TryHydrateMemberAsync(memberEntry.TargetMember.Id, force, cancellationToken).ConfigureAwait(false) ?? memberEntry.TargetMember;

				if (ShouldHydrate(targets, AuditLogHydrationTargets.Related) && memberEntry.Channel is { Id: not 0 })
					memberEntry.Channel = await this.TryHydrateChannelAsync(memberEntry.Channel.Id, force, cancellationToken).ConfigureAwait(false) ?? memberEntry.Channel;
				break;

			case DiscordRoleAuditLogEntry roleEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && roleEntry.TargetRole is { Id: not 0 })
					roleEntry.TargetRole = await this.TryHydrateRoleAsync(roleEntry.TargetRole.Id, force, cancellationToken).ConfigureAwait(false) ?? roleEntry.TargetRole;
				break;

			case DiscordInviteAuditLogEntry inviteEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Related) && inviteEntry.Channel is { Id: not 0 })
					inviteEntry.Channel = await this.TryHydrateChannelAsync(inviteEntry.Channel.Id, force, cancellationToken).ConfigureAwait(false) ?? inviteEntry.Channel;
				break;

			case DiscordWebhookAuditLogEntry webhookEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && webhookEntry.TargetWebhook is { Id: not 0 })
					webhookEntry.TargetWebhook = await this.TryHydrateWebhookAsync(webhookEntry.TargetWebhook.Id, force, cancellationToken).ConfigureAwait(false) ?? webhookEntry.TargetWebhook;
				break;

			case DiscordEmojiAuditLogEntry emojiEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && emojiEntry.EmojiId.HasValue)
					emojiEntry.TargetEmoji = await this.TryHydrateEmojiAsync(emojiEntry.EmojiId.Value, force, cancellationToken).ConfigureAwait(false) ?? emojiEntry.TargetEmoji;
				break;

			case DiscordStickerAuditLogEntry stickerEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && stickerEntry.StickerId.HasValue)
					stickerEntry.TargetSticker = await this.TryHydrateStickerAsync(stickerEntry.StickerId.Value, force, cancellationToken).ConfigureAwait(false) ?? stickerEntry.TargetSticker;
				break;

			case DiscordMessageAuditLogEntry messageEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && messageEntry.TargetMember is { Id: not 0 })
					messageEntry.TargetMember = await this.TryHydrateMemberAsync(messageEntry.TargetMember.Id, force, cancellationToken).ConfigureAwait(false) ?? messageEntry.TargetMember;

				if (ShouldHydrate(targets, AuditLogHydrationTargets.Related) && messageEntry.Channel is { Id: not 0 })
					messageEntry.Channel = await this.TryHydrateChannelAsync(messageEntry.Channel.Id, force, cancellationToken).ConfigureAwait(false) ?? messageEntry.Channel;
				break;

			case DiscordIntegrationAuditLogEntry integrationEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && integrationEntry.IntegrationId.HasValue)
					integrationEntry.TargetIntegration = await this.TryHydrateIntegrationAsync(integrationEntry.IntegrationId.Value, force, cancellationToken).ConfigureAwait(false) ?? integrationEntry.TargetIntegration;
				break;

			case DiscordStageInstanceAuditLogEntry stageEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Related) && stageEntry.Channel is { Id: not 0 })
					stageEntry.Channel = await this.TryHydrateChannelAsync(stageEntry.Channel.Id, force, cancellationToken).ConfigureAwait(false) ?? stageEntry.Channel;

				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && stageEntry.Channel is not null)
					stageEntry.TargetStageInstance = await this.TryHydrateStageInstanceAsync(stageEntry.Channel, force, cancellationToken).ConfigureAwait(false) ?? stageEntry.TargetStageInstance;
				break;

			case DiscordGuildScheduledEventAuditLogEntry scheduledEventEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && scheduledEventEntry.TargetGuildScheduledEvent is { Id: not 0 })
					scheduledEventEntry.TargetGuildScheduledEvent = await this.TryHydrateScheduledEventAsync(scheduledEventEntry.TargetGuildScheduledEvent.Id, force, cancellationToken).ConfigureAwait(false) ?? scheduledEventEntry.TargetGuildScheduledEvent;
				break;

			case DiscordGuildScheduledEventExceptionAuditLogEntry scheduledEventExceptionEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && scheduledEventExceptionEntry.TargetGuildScheduledEvent is { Id: not 0 })
					scheduledEventExceptionEntry.TargetGuildScheduledEvent = await this.TryHydrateScheduledEventAsync(scheduledEventExceptionEntry.TargetGuildScheduledEvent.Id, force, cancellationToken).ConfigureAwait(false) ?? scheduledEventExceptionEntry.TargetGuildScheduledEvent;
				break;

			case DiscordThreadAuditLogEntry threadEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && threadEntry.TargetThread is { Id: not 0 })
					threadEntry.TargetThread = await this.TryHydrateThreadAsync(threadEntry.TargetThread.Id, force, cancellationToken).ConfigureAwait(false) ?? threadEntry.TargetThread;
				break;

			case DiscordApplicationCommandAuditLogEntry applicationCommandEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && applicationCommandEntry.CommandId.HasValue)
					applicationCommandEntry.TargetCommand = await this.TryHydrateApplicationCommandAsync(applicationCommandEntry.CommandId.Value, force, cancellationToken).ConfigureAwait(false) ?? applicationCommandEntry.TargetCommand;
				break;

			case DiscordSoundboardSoundAuditLogEntry soundboardEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && soundboardEntry.SoundboardSoundId.HasValue)
					soundboardEntry.TargetSoundboardSound = await this.TryHydrateSoundboardSoundAsync(soundboardEntry.SoundboardSoundId.Value, force, cancellationToken).ConfigureAwait(false) ?? soundboardEntry.TargetSoundboardSound;
				break;

			case DiscordAutoModerationRuleAuditLogEntry autoModerationEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Target) && autoModerationEntry.TargetMember is { Id: not 0 })
					autoModerationEntry.TargetMember = await this.TryHydrateMemberAsync(autoModerationEntry.TargetMember.Id, force, cancellationToken).ConfigureAwait(false) ?? autoModerationEntry.TargetMember;

				if (ShouldHydrate(targets, AuditLogHydrationTargets.Related) && autoModerationEntry.Channel is { Id: not 0 })
					autoModerationEntry.Channel = await this.TryHydrateChannelAsync(autoModerationEntry.Channel.Id, force, cancellationToken).ConfigureAwait(false) ?? autoModerationEntry.Channel;
				break;

			case DiscordVoiceChannelStatusAuditLogEntry voiceChannelStatusEntry:
				if (ShouldHydrate(targets, AuditLogHydrationTargets.Related) && voiceChannelStatusEntry.Channel is { Id: not 0 })
					voiceChannelStatusEntry.Channel = await this.TryHydrateChannelAsync(voiceChannelStatusEntry.Channel.Id, force, cancellationToken).ConfigureAwait(false) ?? voiceChannelStatusEntry.Channel;
				break;
		}
	}

	/// <summary>
	///     Hydrates the actor, primary target, and related references for this entry.
	/// </summary>
	/// <param name="force">
	///     Controls whether the helper may perform REST requests when the requested entities are not already cached.
	/// </param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>A task representing the asynchronous hydration operation.</returns>
	/// <remarks>
	///     This is a convenience wrapper around <see cref="HydrateAsync(AuditLogHydrationTargets, bool)" /> with
	///     <see cref="AuditLogHydrationTargets.All" />.
	/// </remarks>
	public Task HydrateAllAsync(bool force = true, CancellationToken cancellationToken = default)
		=> this.HydrateAsync(AuditLogHydrationTargets.All, force, cancellationToken);

	/// <summary>
	///     Gets whether a hydration flag is enabled.
	/// </summary>
	/// <param name="value">The full requested hydration value.</param>
	/// <param name="flag">The specific flag to inspect.</param>
	/// <returns><see langword="true" /> when the flag is enabled; otherwise <see langword="false" />.</returns>
	private static bool ShouldHydrate(AuditLogHydrationTargets value, AuditLogHydrationTargets flag)
		=> (value & flag) == flag;

	/// <summary>
	///     Tries to replace a cached or partial actor reference with a live user.
	/// </summary>
	/// <param name="userId">The user id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the user is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved user, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordUser?> TryHydrateUserAsync(ulong userId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return this.Discord.UserCache.TryGetValue(userId, out var cachedUser) ? cachedUser : null;

		try
		{
			return await this.Discord.ApiClient.GetUserAsync(userId, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a cached or partial member reference with a live member.
	/// </summary>
	/// <param name="memberId">The member id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the member is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved member, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordMember?> TryHydrateMemberAsync(ulong memberId, bool force, CancellationToken cancellationToken = default)
	{
		return !force
			? this.Guild.MembersInternal.TryGetValue(memberId, out var cachedMember) ? cachedMember : null
			: await this.Guild.TryGetMemberAsync(memberId, true, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Tries to replace a cached or partial channel reference with a live channel.
	/// </summary>
	/// <param name="channelId">The channel id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the channel is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved channel, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordChannel?> TryHydrateChannelAsync(ulong channelId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return this.Guild.GetThread(channelId) ?? this.Guild.GetChannel(channelId);

		try
		{
			return await this.Discord.ApiClient.GetChannelAsync(channelId, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a cached or partial thread reference with a live thread.
	/// </summary>
	/// <param name="threadId">The thread id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the thread is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved thread, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordThreadChannel?> TryHydrateThreadAsync(ulong threadId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return this.Guild.GetThread(threadId);

		try
		{
			return await this.Discord.ApiClient.GetThreadAsync(threadId, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a cached or partial role reference with a live role.
	/// </summary>
	/// <param name="roleId">The role id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the role is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved role, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordRole?> TryHydrateRoleAsync(ulong roleId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return this.Guild.RolesInternal.TryGetValue(roleId, out var cachedRole) ? cachedRole : null;

		try
		{
			return await this.Guild.GetRoleAsync(roleId, cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a cached or partial webhook reference with a live webhook.
	/// </summary>
	/// <param name="webhookId">The webhook id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the webhook is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved webhook, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordWebhook?> TryHydrateWebhookAsync(ulong webhookId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return null;

		try
		{
			return await this.Discord.ApiClient.GetWebhookAsync(webhookId, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a partial emoji reference with a live emoji.
	/// </summary>
	/// <param name="emojiId">The emoji id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the emoji is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved emoji, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordGuildEmoji?> TryHydrateEmojiAsync(ulong emojiId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return null;

		try
		{
			return await this.Guild.GetEmojiAsync(emojiId, cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a partial sticker reference with a live sticker.
	/// </summary>
	/// <param name="stickerId">The sticker id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the sticker is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved sticker, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordSticker?> TryHydrateStickerAsync(ulong stickerId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return null;

		try
		{
			return await this.Guild.GetStickerAsync(stickerId, cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a partial integration reference with a live integration.
	/// </summary>
	/// <param name="integrationId">The integration id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the integration is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved integration, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordIntegration?> TryHydrateIntegrationAsync(ulong integrationId, bool force, CancellationToken cancellationToken = default)
		=> !force ? null : (await this.Guild.GetIntegrationsAsync(cancellationToken).ConfigureAwait(false)).FirstOrDefault(x => x.Id == integrationId);

	/// <summary>
	///     Tries to replace a partial stage instance reference with a live stage instance.
	/// </summary>
	/// <param name="channel">The stage channel the instance belongs to.</param>
	/// <param name="force">Whether REST calls are allowed when the stage channel is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved stage instance, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordStageInstance?> TryHydrateStageInstanceAsync(DiscordChannel channel, bool force, CancellationToken cancellationToken = default)
	{
		var hydratedChannel = force
			? await this.TryHydrateChannelAsync(channel.Id, true, cancellationToken).ConfigureAwait(false) ?? channel
			: channel;

		try
		{
			return await hydratedChannel.GetStageAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a cached or partial scheduled event reference with a live scheduled event.
	/// </summary>
	/// <param name="scheduledEventId">The scheduled event id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the scheduled event is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved scheduled event, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordScheduledEvent?> TryHydrateScheduledEventAsync(ulong scheduledEventId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return this.Guild.ScheduledEventsInternal.TryGetValue(scheduledEventId, out var cachedScheduledEvent) ? cachedScheduledEvent : null;

		try
		{
			return await this.Guild.GetScheduledEventAsync(scheduledEventId, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a partial application command reference with a live guild application command.
	/// </summary>
	/// <param name="commandId">The command id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the command is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved application command, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordApplicationCommand?> TryHydrateApplicationCommandAsync(ulong commandId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return null;

		try
		{
			var applicationId = this.Discord.CurrentApplication?.Id
				?? (await this.Discord.GetCurrentApplicationAsync(cancellationToken).ConfigureAwait(false)).Id;
			return await this.Discord.ApiClient.GetGuildApplicationCommandAsync(applicationId, this.Guild.Id, commandId, cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Tries to replace a partial soundboard sound reference with a live soundboard sound.
	/// </summary>
	/// <param name="soundId">The soundboard sound id to resolve.</param>
	/// <param name="force">Whether REST calls are allowed when the soundboard sound is not already cached.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The resolved soundboard sound, or <see langword="null" /> when it could not be resolved.</returns>
	private async Task<DiscordSoundboardSound?> TryHydrateSoundboardSoundAsync(ulong soundId, bool force, CancellationToken cancellationToken = default)
	{
		if (!force)
			return null;

		try
		{
			return await this.Guild.GetSoundboardSoundAsync(soundId).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}
}

using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;

namespace DisCatSharp.Net.AuditLogs;

/// <summary>
///     Resolves audit log side-loaded references into DisCatSharp entity instances.
/// </summary>
/// <remarks>
///     This resolver intentionally never performs REST calls. If Discord omits a referenced object and the guild cache
///     cannot resolve it, the store creates a minimal partial entity instead so audit log parsing stays side-effect free.
/// </remarks>
internal sealed class AuditLogReferenceStore
{
	/// <summary>
	///     The guild the audit log entries belong to.
	/// </summary>
	private readonly DiscordGuild _guild;

	/// <summary>
	///     Users included in the audit log response, indexed by id.
	/// </summary>
	private readonly Dictionary<ulong, DiscordUser> _users = [];

	/// <summary>
	///     Webhooks included in the audit log response, indexed by id.
	/// </summary>
	private readonly Dictionary<ulong, DiscordWebhook> _webhooks = [];

	/// <summary>
	///     Thread channels included in the audit log response, indexed by id.
	/// </summary>
	private readonly Dictionary<ulong, DiscordThreadChannel> _threads = [];

	/// <summary>
	///     Guild scheduled events included in the audit log response, indexed by id.
	/// </summary>
	private readonly Dictionary<ulong, DiscordScheduledEvent> _guildScheduledEvents = [];

	/// <summary>
	///     Initializes a new instance of the <see cref="AuditLogReferenceStore"/> class.
	/// </summary>
	/// <param name="guild">The guild that owns the audit log payload.</param>
	/// <param name="auditLog">The raw audit log payload containing side-loaded reference collections.</param>
	public AuditLogReferenceStore(DiscordGuild guild, RawAuditLog? auditLog)
	{
		this._guild = guild;

		foreach (var user in auditLog?.Users ?? [])
		{
			this._users[user.Id] = new DiscordUser(new TransportUser
			{
				Id = user.Id,
				Username = user.Username,
				Discriminator = user.Discriminator,
				AvatarHash = user.AvatarHash,
				GlobalName = user.GlobalName
			})
			{
				Discord = guild.Discord
			};
		}

		foreach (var webhook in auditLog?.Webhooks ?? [])
		{
			this._webhooks[webhook.Id] = new DiscordWebhook
			{
				Discord = guild.Discord,
				Id = webhook.Id,
				Name = webhook.Name,
				AvatarHash = webhook.AvatarHash,
				ChannelId = webhook.ChannelId ?? 0,
				GuildId = webhook.GuildId ?? guild.Id,
				Token = webhook.Token
			};
		}

		foreach (var thread in auditLog?.Threads ?? [])
		{
			this._threads[thread.Id] = guild.GetThread(thread.Id) ?? new DiscordThreadChannel
			{
				Discord = guild.Discord,
				Id = thread.Id,
				GuildId = thread.GuildId ?? guild.Id,
				ParentId = thread.ParentId,
				Name = thread.Name,
				Type = thread.Type
			};
		}

		foreach (var scheduledEvent in auditLog?.GuildScheduledEvents ?? [])
		{
			this._guildScheduledEvents[scheduledEvent.Id] = new DiscordScheduledEvent
			{
				Discord = guild.Discord,
				Id = scheduledEvent.Id,
				GuildId = scheduledEvent.GuildId ?? guild.Id,
				ChannelId = scheduledEvent.ChannelId,
				CreatorId = scheduledEvent.CreatorId ?? 0,
				Name = scheduledEvent.Name,
				Description = scheduledEvent.Description
			};
		}
	}

	/// <summary>
	///     Resolves the acting user for an audit log entry.
	/// </summary>
	/// <param name="userId">The actor user id from the audit log entry.</param>
	/// <returns>
	///     The cached or side-loaded user when available; otherwise a partial placeholder user containing only the id.
	/// </returns>
	/// <remarks>
	///     Returning a placeholder user keeps the public <c>Actor</c> surface populated for sparse payloads without
	///     issuing an additional API request.
	/// </remarks>
	public DiscordUser? ResolveActor(ulong? userId)
	{
		return !userId.HasValue
			? null
			: this._guild.Discord.UserCache.TryGetValue(userId.Value, out var user)
			? user
			: this._users.TryGetValue(userId.Value, out var payloadUser)
			? payloadUser
			: new DiscordUser(new TransportUser
			{
				Id = userId.Value,
				Username = "Unknown User",
				Discriminator = "0"
			})
			{
				Discord = this._guild.Discord
			};
	}

	/// <summary>
	///     Resolves a guild member targeted by an audit log entry.
	/// </summary>
	/// <param name="userId">The targeted user id from the audit log entry.</param>
	/// <returns>
	///     The cached guild member when available; otherwise a partial member built from the resolved actor data.
	/// </returns>
	/// <remarks>
	///     Discord does not include a member side collection in audit log responses, so unresolved members are derived
	///     from the actor/user data when possible.
	/// </remarks>
	public DiscordMember? ResolveMember(ulong? userId)
	{
		if (!userId.HasValue)
			return null;

		if (this._guild.MembersInternal is not null && this._guild.MembersInternal.TryGetValue(userId.Value, out var member))
			return member;

		var actor = this.ResolveActor(userId);
		return actor is null
			? null
			: new DiscordMember(actor)
			{
				Discord = this._guild.Discord,
				GuildId = this._guild.Id
			};
	}

	/// <summary>
	///     Resolves a channel or thread referenced by an audit log entry.
	/// </summary>
	/// <param name="channelId">The channel id from the entry or its options object.</param>
	/// <returns>
	///     The cached or side-loaded channel when available; otherwise a partial channel containing only identifiers.
	/// </returns>
	/// <remarks>
	///     Guild cache is preferred over the side-loaded thread collection so already-cached
	///     <see cref="DiscordThreadChannel" /> instances and other richer channel subtypes stay intact.
	/// </remarks>
	public DiscordChannel? ResolveChannel(ulong? channelId)
	{
		return !channelId.HasValue
			? null
			: this._threads.TryGetValue(channelId.Value, out var thread)
			? thread
			: this._guild.GetChannel(channelId.Value) is { } cachedChannel
			? cachedChannel
			: new DiscordChannel
			{
				Discord = this._guild.Discord,
				Id = channelId.Value,
				GuildId = this._guild.Id
			};
	}

	/// <summary>
	///     Resolves a role referenced by an audit log entry.
	/// </summary>
	/// <param name="roleId">The role identifier.</param>
	/// <returns>
	///     The cached role when available; otherwise a partial role containing only identifiers.
	/// </returns>
	public DiscordRole? ResolveRole(ulong? roleId)
	{
		return !roleId.HasValue
			? null
			: this._guild.RolesInternal.TryGetValue(roleId.Value, out var role)
			? role
			: new DiscordRole
			{
				Discord = this._guild.Discord,
				Id = roleId.Value,
				GuildId = this._guild.Id
			};
	}

	/// <summary>
	///     Resolves a webhook referenced by an audit log entry.
	/// </summary>
	/// <param name="webhookId">The webhook identifier.</param>
	/// <returns>
	///     The side-loaded webhook when available; otherwise a partial webhook containing only identifiers.
	/// </returns>
	public DiscordWebhook? ResolveWebhook(ulong? webhookId)
	{
		return !webhookId.HasValue
			? null
			: this._webhooks.TryGetValue(webhookId.Value, out var webhook)
			? webhook
			: new DiscordWebhook
			{
				Discord = this._guild.Discord,
				Id = webhookId.Value,
				GuildId = this._guild.Id
			};
	}

	/// <summary>
	///     Resolves a guild scheduled event referenced by an audit log entry.
	/// </summary>
	/// <param name="guildScheduledEventId">The scheduled event identifier.</param>
	/// <returns>
	///     The cached or side-loaded scheduled event when available; otherwise a partial scheduled event containing only
	///     identifiers.
	/// </returns>
	public DiscordScheduledEvent? ResolveScheduledEvent(ulong? guildScheduledEventId)
	{
		return !guildScheduledEventId.HasValue
			? null
			: this._guild.ScheduledEventsInternal.TryGetValue(guildScheduledEventId.Value, out var cachedScheduledEvent)
			? cachedScheduledEvent
			: this._guildScheduledEvents.TryGetValue(guildScheduledEventId.Value, out var scheduledEvent)
			? scheduledEvent
			: new DiscordScheduledEvent
			{
				Discord = this._guild.Discord,
				Id = guildScheduledEventId.Value,
				GuildId = this._guild.Id
			};
	}
}

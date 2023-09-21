#nullable enable
using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Methods marked with this attribute will be registered as event handling methods
/// if the associated type / an associated instance is being registered.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EventAttribute : Attribute
{
	internal readonly string? EventName;

	public EventAttribute()
	{ }

	/// <param name="evtn"><para>The name of the event.</para>
	/// <para>The attributed method's name will be used if null.</para></param>
	public EventAttribute(DiscordEvent evtn)
	{
		this.EventName = evtn.ToString();
	}
}

/// <summary>
/// Classes marked with this attribute will be considered for event handler registration from an assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EventHandlerAttribute : Attribute
{ }

/// <summary>
/// All events available in <see cref="DiscordClient"/> for use with <see cref="EventAttribute"/>.
/// </summary>
public enum DiscordEvent
{
	ApplicationCommandCreated,
	ApplicationCommandDeleted,
	ApplicationCommandPermissionsUpdated,
	ApplicationCommandUpdated,
	ChannelCreated,
	ChannelDeleted,
	ChannelPinsUpdated,
	ChannelUpdated,
	ClientErrored,
	ComponentInteractionCreated,
	ContextMenuInteractionCreated,
	DmChannelDeleted,
	EmbeddedActivityUpdated,
	GuildApplicationCommandCountUpdated,
	GuildAvailable,
	GuildBanAdded,
	GuildBanRemoved,
	GuildCreated,
	GuildDeleted,
	GuildDownloadCompleted,
	GuildEmojisUpdated,
	GuildIntegrationCreated,
	GuildIntegrationDeleted,
	GuildIntegrationsUpdated,
	GuildIntegrationUpdated,
	GuildMemberAdded,
	GuildMemberRemoved,
	GuildMembersChunked,
	GuildMemberTimeoutAdded,
	GuildMemberTimeoutChanged,
	GuildMemberTimeoutRemoved,
	GuildMemberUpdated,
	GuildRoleCreated,
	GuildRoleDeleted,
	GuildRoleUpdated,
	GuildScheduledEventCreated,
	GuildScheduledEventDeleted,
	GuildScheduledEventUpdated,
	GuildScheduledEventUserAdded,
	GuildScheduledEventUserRemoved,
	GuildStickersUpdated,
	GuildUnavailable,
	GuildUpdated,
	Heartbeated,
	InteractionCreated,
	InviteCreated,
	InviteDeleted,
	MessageAcknowledged,
	MessageCreated,
	MessageDeleted,
	MessageReactionAdded,
	MessageReactionRemoved,
	MessageReactionRemovedEmoji,
	MessageReactionsCleared,
	MessagesBulkDeleted,
	MessageUpdated,
	PayloadReceived,
	PresenceUpdated,
	RateLimitHit,
	Ready,
	Resumed,
	SocketClosed,
	SocketErrored,
	SocketOpened,
	StageInstanceCreated,
	StageInstanceDeleted,
	StageInstanceUpdated,
	ThreadCreated,
	ThreadDeleted,
	ThreadListSynced,
	ThreadMembersUpdated,
	ThreadMemberUpdated,
	ThreadUpdated,
	TypingStarted,
	UnknownEvent,
	UserSettingsUpdated,
	UserUpdated,
	VoiceServerUpdated,
	VoiceStateUpdated,
	WebhooksUpdated,
	Zombied,
	AutomodRuleCreated,
	AutomodRuleUpdated,
	AutomodRuleDeleted,
	AutomodActionExecuted,
	GuildAuditLogEntryCreated,
	VoiceChannelStatusUpdated,
	EntitlementCreated,
	EntitlementUpdated,
	EntitlementDeleted
}

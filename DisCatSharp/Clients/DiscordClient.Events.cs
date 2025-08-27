using System;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
///     Represents a discord client.
/// </summary>
public sealed partial class DiscordClient
{
	/// <summary>
	///     Gets the event execution limit.
	/// </summary>
	internal static TimeSpan EventExecutionLimit { get; } = TimeSpan.FromSeconds(1);

#region WebSocket

	/// <summary>
	///     Fired whenever a WebSocket error occurs within the client.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, SocketErrorEventArgs> SocketErrored
	{
		add => this._socketErrored.Register(value);
		remove => this._socketErrored.Unregister(value);
	}

	private AsyncEvent<DiscordClient, SocketErrorEventArgs> _socketErrored;

	/// <summary>
	///     Fired whenever WebSocket connection is established.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, SocketEventArgs> SocketOpened
	{
		add => this._socketOpened.Register(value);
		remove => this._socketOpened.Unregister(value);
	}

	private AsyncEvent<DiscordClient, SocketEventArgs> _socketOpened;

	/// <summary>
	///     Fired whenever WebSocket connection is terminated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, SocketCloseEventArgs> SocketClosed
	{
		add => this._socketClosed.Register(value);
		remove => this._socketClosed.Unregister(value);
	}

	private AsyncEvent<DiscordClient, SocketCloseEventArgs> _socketClosed;

	/// <summary>
	///     Fired when the client enters ready state.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ReadyEventArgs> Ready
	{
		add => this.ReadyEv.Register(value);
		remove => this.ReadyEv.Unregister(value);
	}

	internal AsyncEvent<DiscordClient, ReadyEventArgs> ReadyEv;

	/// <summary>
	///     Fired whenever a session is resumed.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ReadyEventArgs> Resumed
	{
		add => this._resumed.Register(value);
		remove => this._resumed.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ReadyEventArgs> _resumed;

	/// <summary>
	///     Fired on received heartbeat ACK.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, HeartbeatEventArgs> Heartbeated
	{
		add => this._heartbeated.Register(value);
		remove => this._heartbeated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, HeartbeatEventArgs> _heartbeated;

#endregion

#region Channel

	/// <summary>
	///     Fired when a new channel is created.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ChannelCreateEventArgs> ChannelCreated
	{
		add => this._channelCreated.Register(value);
		remove => this._channelCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ChannelCreateEventArgs> _channelCreated;

	/// <summary>
	///     Fired when a channel is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ChannelUpdateEventArgs> ChannelUpdated
	{
		add => this._channelUpdated.Register(value);
		remove => this._channelUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ChannelUpdateEventArgs> _channelUpdated;

	/// <summary>
	///     Fired when a channel is deleted
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ChannelDeleteEventArgs> ChannelDeleted
	{
		add => this._channelDeleted.Register(value);
		remove => this._channelDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ChannelDeleteEventArgs> _channelDeleted;

	/// <summary>
	///     Fired when a dm channel is deleted
	///     For this Event you need the <see cref="DiscordIntents.DirectMessages" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, DmChannelDeleteEventArgs> DmChannelDeleted
	{
		add => this._dmChannelDeleted.Register(value);
		remove => this._dmChannelDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, DmChannelDeleteEventArgs> _dmChannelDeleted;

	/// <summary>
	///     Fired whenever a channel's pinned message list is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ChannelPinsUpdateEventArgs> ChannelPinsUpdated
	{
		add => this._channelPinsUpdated.Register(value);
		remove => this._channelPinsUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs> _channelPinsUpdated;

	/// <summary>
	///     Fired whenever a voice channel's status is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, VoiceChannelStatusUpdateEventArgs> VoiceChannelStatusUpdated
	{
		add => this._voiceChannelStatusUpdated.Register(value);
		remove => this._voiceChannelStatusUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, VoiceChannelStatusUpdateEventArgs> _voiceChannelStatusUpdated;

	/// <summary>
	///     Fired whenever a voice channel's start time is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, VoiceChannelStartTimeUpdateEventArgs> VoiceChannelStartTimeUpdated
	{
		add => this._voiceChannelStartTimeUpdated.Register(value);
		remove => this._voiceChannelStartTimeUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, VoiceChannelStartTimeUpdateEventArgs> _voiceChannelStartTimeUpdated;

#endregion

#region Guild

	/// <summary>
	///     Fired when the user joins a new guild.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	/// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
	public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildCreated
	{
		add => this._guildCreated.Register(value);
		remove => this._guildCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildCreated;

	/// <summary>
	///     Fired when a guild is becoming available.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildAvailable
	{
		add => this._guildAvailable.Register(value);
		remove => this._guildAvailable.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildAvailable;

	/// <summary>
	///     Fired when a guild is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildUpdateEventArgs> GuildUpdated
	{
		add => this._guildUpdated.Register(value);
		remove => this._guildUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildUpdateEventArgs> _guildUpdated;

	/// <summary>
	///     Fired when the user leaves or is removed from a guild.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildDeleted
	{
		add => this._guildDeleted.Register(value);
		remove => this._guildDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildDeleted;

	/// <summary>
	///     Fired when a guild becomes unavailable.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildUnavailable
	{
		add => this._guildUnavailable.Register(value);
		remove => this._guildUnavailable.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildUnavailable;

	/// <summary>
	///     Fired when all guilds finish streaming from Discord.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompleted
	{
		add => this.GuildDownloadCompletedEv.Register(value);
		remove => this.GuildDownloadCompletedEv.Unregister(value);
	}

	internal AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompletedEv;

	/// <summary>
	///     Fired when a guilds emojis get updated
	///     For this Event you need the <see cref="DiscordIntents.GuildEmojisAndStickers" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildEmojisUpdateEventArgs> GuildEmojisUpdated
	{
		add => this._guildEmojisUpdated.Register(value);
		remove => this._guildEmojisUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs> _guildEmojisUpdated;

	/// <summary>
	///     Fired when a guilds stickers get updated
	///     For this Event you need the <see cref="DiscordIntents.GuildEmojisAndStickers" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildStickersUpdateEventArgs> GuildStickersUpdated
	{
		add => this._guildStickersUpdated.Register(value);
		remove => this._guildStickersUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs> _guildStickersUpdated;

	/// <summary>
	///     Fired when a guild integration is updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
	{
		add => this._guildIntegrationsUpdated.Register(value);
		remove => this._guildIntegrationsUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

	/// <summary>
	///     Fired when a guild audit log entry was created.
	///     Requires bot to have the <see cref="Permissions.ViewAuditLog" /> permission.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildAuditLogEntryCreateEventArgs> GuildAuditLogEntryCreated
	{
		add => this._guildAuditLogEntryCreated.Register(value);
		remove => this._guildAuditLogEntryCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildAuditLogEntryCreateEventArgs> _guildAuditLogEntryCreated;

	/// <summary>
	///     Fired when a guild applied boosts get updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildAppliedBoostsUpdateEventArgs> GuildAppliedBoostsUpdated
	{
		add => this._guildAppliedBoostsUpdated.Register(value);
		remove => this._guildAppliedBoostsUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildAppliedBoostsUpdateEventArgs> _guildAppliedBoostsUpdated;

#endregion

#region Automod

	/// <summary>
	///     Fired when an auto mod rule gets created.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, AutomodRuleCreateEventArgs> AutomodRuleCreated
	{
		add => this._automodRuleCreated.Register(value);
		remove => this._automodRuleCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, AutomodRuleCreateEventArgs> _automodRuleCreated;

	/// <summary>
	///     Fired when an auto mod rule gets updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, AutomodRuleUpdateEventArgs> AutomodRuleUpdated
	{
		add => this._automodRuleUpdated.Register(value);
		remove => this._automodRuleUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, AutomodRuleUpdateEventArgs> _automodRuleUpdated;

	/// <summary>
	///     Fired when an auto mod rule gets deleted.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, AutomodRuleDeleteEventArgs> AutomodRuleDeleted
	{
		add => this._automodRuleDeleted.Register(value);
		remove => this._automodRuleDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, AutomodRuleDeleteEventArgs> _automodRuleDeleted;

	/// <summary>
	///     Fired when a rule is triggered and an action is executed.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, AutomodActionExecutedEventArgs> AutomodActionExecuted
	{
		add => this._automodActionExecuted.Register(value);
		remove => this._automodActionExecuted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, AutomodActionExecutedEventArgs> _automodActionExecuted;

#endregion

#region Guild Ban

	/// <summary>
	///     Fired when a guild ban gets added
	///     For this Event you need the <see cref="DiscordIntents.GuildModeration" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildBanAddEventArgs> GuildBanAdded
	{
		add => this._guildBanAdded.Register(value);
		remove => this._guildBanAdded.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildBanAddEventArgs> _guildBanAdded;

	/// <summary>
	///     Fired when a guild ban gets removed
	///     For this Event you need the <see cref="DiscordIntents.GuildModeration" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildBanRemoveEventArgs> GuildBanRemoved
	{
		add => this._guildBanRemoved.Register(value);
		remove => this._guildBanRemoved.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildBanRemoveEventArgs> _guildBanRemoved;

#endregion

#region Guild Timeout

	/// <summary>
	///     Fired when a guild member timeout gets added.
	///     For this Event you need the <see cref="DiscordIntents.GuildModeration" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildMemberTimeoutAddEventArgs> GuildMemberTimeoutAdded
	{
		add => this._guildMemberTimeoutAdded.Register(value);
		remove => this._guildMemberTimeoutAdded.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildMemberTimeoutAddEventArgs> _guildMemberTimeoutAdded;

	/// <summary>
	///     Fired when a guild member timeout gets changed.
	///     For this Event you need the <see cref="DiscordIntents.GuildMembers" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildMemberTimeoutUpdateEventArgs> GuildMemberTimeoutChanged
	{
		add => this._guildMemberTimeoutChanged.Register(value);
		remove => this._guildMemberTimeoutChanged.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildMemberTimeoutUpdateEventArgs> _guildMemberTimeoutChanged;

	/// <summary>
	///     Fired when a guild member timeout gets removed.
	///     For this Event you need the <see cref="DiscordIntents.GuildMembers" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildMemberTimeoutRemoveEventArgs> GuildMemberTimeoutRemoved
	{
		add => this._guildMemberTimeoutRemoved.Register(value);
		remove => this._guildMemberTimeoutRemoved.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildMemberTimeoutRemoveEventArgs> _guildMemberTimeoutRemoved;

#endregion

#region Guild Scheduled Event

	/// <summary>
	///     Fired when a scheduled Event is created.
	///     For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildScheduledEventCreateEventArgs> GuildScheduledEventCreated
	{
		add => this._guildScheduledEventCreated.Register(value);
		remove => this._guildScheduledEventCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildScheduledEventCreateEventArgs> _guildScheduledEventCreated;

	/// <summary>
	///     Fired when a scheduled Event is updated.
	///     For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildScheduledEventUpdateEventArgs> GuildScheduledEventUpdated
	{
		add => this._guildScheduledEventUpdated.Register(value);
		remove => this._guildScheduledEventUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildScheduledEventUpdateEventArgs> _guildScheduledEventUpdated;

	/// <summary>
	///     Fired when a scheduled Event is deleted.
	///     For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildScheduledEventDeleteEventArgs> GuildScheduledEventDeleted
	{
		add => this._guildScheduledEventDeleted.Register(value);
		remove => this._guildScheduledEventDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildScheduledEventDeleteEventArgs> _guildScheduledEventDeleted;

	/// <summary>
	///     Fired when a user subscribes to a scheduled event.
	///     For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildScheduledEventUserAddEventArgs> GuildScheduledEventUserAdded
	{
		add => this._guildScheduledEventUserAdded.Register(value);
		remove => this._guildScheduledEventUserAdded.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildScheduledEventUserAddEventArgs> _guildScheduledEventUserAdded;

	/// <summary>
	///     Fired when a user unsubscribes from a scheduled event.
	///     For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildScheduledEventUserRemoveEventArgs> GuildScheduledEventUserRemoved
	{
		add => this._guildScheduledEventUserRemoved.Register(value);
		remove => this._guildScheduledEventUserRemoved.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildScheduledEventUserRemoveEventArgs> _guildScheduledEventUserRemoved;

#endregion

#region Guild Integration

	/// <summary>
	///     Fired when a guild integration is created.
	///     For this Event you need the <see cref="DiscordIntents.GuildIntegrations" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildIntegrationCreateEventArgs> GuildIntegrationCreated
	{
		add => this._guildIntegrationCreated.Register(value);
		remove => this._guildIntegrationCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildIntegrationCreateEventArgs> _guildIntegrationCreated;

	/// <summary>
	///     Fired when a guild integration is updated.
	///     For this Event you need the <see cref="DiscordIntents.GuildIntegrations" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildIntegrationUpdateEventArgs> GuildIntegrationUpdated
	{
		add => this._guildIntegrationUpdated.Register(value);
		remove => this._guildIntegrationUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildIntegrationUpdateEventArgs> _guildIntegrationUpdated;

	/// <summary>
	///     Fired when a guild integration is deleted.
	///     For this Event you need the <see cref="DiscordIntents.GuildIntegrations" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildIntegrationDeleteEventArgs> GuildIntegrationDeleted
	{
		add => this._guildIntegrationDeleted.Register(value);
		remove => this._guildIntegrationDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildIntegrationDeleteEventArgs> _guildIntegrationDeleted;

#endregion

#region Guild Member

	/// <summary>
	///     Fired when a new user joins a guild.
	///     For this Event you need the <see cref="DiscordIntents.GuildMembers" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildMemberAddEventArgs> GuildMemberAdded
	{
		add => this._guildMemberAdded.Register(value);
		remove => this._guildMemberAdded.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildMemberAddEventArgs> _guildMemberAdded;

	/// <summary>
	///     Fired when a user is removed from a guild (leave/kick/ban).
	///     For this Event you need the <see cref="DiscordIntents.GuildMembers" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildMemberRemoveEventArgs> GuildMemberRemoved
	{
		add => this._guildMemberRemoved.Register(value);
		remove => this._guildMemberRemoved.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs> _guildMemberRemoved;

	/// <summary>
	///     Fired when a guild member is updated.
	///     For this Event you need the <see cref="DiscordIntents.GuildMembers" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildMemberUpdateEventArgs> GuildMemberUpdated
	{
		add => this._guildMemberUpdated.Register(value);
		remove => this._guildMemberUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs> _guildMemberUpdated;

	/// <summary>
	///     Fired in response to Gateway Request Guild Members.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildMembersChunkEventArgs> GuildMembersChunked
	{
		add => this._guildMembersChunked.Register(value);
		remove => this._guildMembersChunked.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildMembersChunkEventArgs> _guildMembersChunked;

#endregion

#region Guild Role

	/// <summary>
	///     Fired when a guild role is created.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildRoleCreateEventArgs> GuildRoleCreated
	{
		add => this._guildRoleCreated.Register(value);
		remove => this._guildRoleCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildRoleCreateEventArgs> _guildRoleCreated;

	/// <summary>
	///     Fired when a guild role is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildRoleUpdateEventArgs> GuildRoleUpdated
	{
		add => this._guildRoleUpdated.Register(value);
		remove => this._guildRoleUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs> _guildRoleUpdated;

	/// <summary>
	///     Fired when a guild role is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildRoleDeleteEventArgs> GuildRoleDeleted
	{
		add => this._guildRoleDeleted.Register(value);
		remove => this._guildRoleDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs> _guildRoleDeleted;

#endregion

#region Guild Soundboard Sound

	/// <summary>
	///     Fired when a guild soundboard sound is created.
	///     For this Event you need the <see cref="DiscordIntents.GuildExpressions" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildSoundboardSoundCreateEventArgs> GuildSoundboardSoundCreated
	{
		add => this._guildSoundboardSoundCreated.Register(value);
		remove => this._guildSoundboardSoundCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildSoundboardSoundCreateEventArgs> _guildSoundboardSoundCreated;

	/// <summary>
	///     Fired when a guild soundboard sound is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildSoundboardSoundUpdateEventArgs> GuildSoundboardSoundUpdated
	{
		add => this._guildSoundboardSoundUpdated.Register(value);
		remove => this._guildSoundboardSoundUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildSoundboardSoundUpdateEventArgs> _guildSoundboardSoundUpdated;

	/// <summary>
	///     Fired when a guild soundboard sound is deleted.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildSoundboardSoundDeleteEventArgs> GuildSoundboardSoundDeleted
	{
		add => this._guildSoundboardSoundDeleted.Register(value);
		remove => this._guildSoundboardSoundDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildSoundboardSoundDeleteEventArgs> _guildSoundboardSoundDeleted;

	/// <summary>
	///     Fired when guild soundboard sounds is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildSoundboardSoundsUpdateEventArgs> GuildSoundboardSoundsUpdated
	{
		add => this._guildSoundboardSoundsUpdated.Register(value);
		remove => this._guildSoundboardSoundsUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildSoundboardSoundsUpdateEventArgs> _guildSoundboardSoundsUpdated;

	/// <summary>
	///     Fired in response to <see cref="DiscordClient.RequestSoundboardSoundsAsync" />.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, SoundboardSoundsEventArgs> SoundboardSounds
	{
		add => this._soundboardSounds.Register(value);
		remove => this._soundboardSounds.Unregister(value);
	}

	private AsyncEvent<DiscordClient, SoundboardSoundsEventArgs> _soundboardSounds;

#endregion

#region Guild Member Application

	/// <summary>
	///     Fired when a guild join request is created.
	///     For this Event you need the <see cref="DiscordIntents.GuildExpressions" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildJoinRequestCreateEventArgs> GuildJoinRequestCreated
	{
		add => this._guildJoinRequestCreated.Register(value);
		remove => this._guildJoinRequestCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildJoinRequestCreateEventArgs> _guildJoinRequestCreated;

	/// <summary>
	///     Fired when a guild join request is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildJoinRequestUpdateEventArgs> GuildJoinRequestUpdated
	{
		add => this._guildJoinRequestUpdated.Register(value);
		remove => this._guildJoinRequestUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildJoinRequestUpdateEventArgs> _guildJoinRequestUpdated;

	/// <summary>
	///     Fired when a guild join request is deleted.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildJoinRequestDeleteEventArgs> GuildJoinRequestDeleted
	{
		add => this._guildJoinRequestDeleted.Register(value);
		remove => this._guildJoinRequestDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildJoinRequestDeleteEventArgs> _guildJoinRequestDeleted;

#endregion

#region Invite

	/// <summary>
	///     Fired when an invite is created.
	///     For this Event you need the <see cref="DiscordIntents.GuildInvites" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, InviteCreateEventArgs> InviteCreated
	{
		add => this._inviteCreated.Register(value);
		remove => this._inviteCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, InviteCreateEventArgs> _inviteCreated;

	/// <summary>
	///     Fired when an invite is deleted.
	///     For this Event you need the <see cref="DiscordIntents.GuildInvites" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, InviteDeleteEventArgs> InviteDeleted
	{
		add => this._inviteDeleted.Register(value);
		remove => this._inviteDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, InviteDeleteEventArgs> _inviteDeleted;

#endregion

#region Message

	/// <summary>
	///     Fired when a message is created.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessages" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageCreateEventArgs> MessageCreated
	{
		add => this._messageCreated.Register(value);
		remove => this._messageCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageCreateEventArgs> _messageCreated;

	/// <summary>
	///     Fired when message is acknowledged by the user.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessages" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageAcknowledgeEventArgs> MessageAcknowledged
	{
		add => this._messageAcknowledged.Register(value);
		remove => this._messageAcknowledged.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageAcknowledgeEventArgs> _messageAcknowledged;

	/// <summary>
	///     Fired when a message is updated.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessages" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageUpdateEventArgs> MessageUpdated
	{
		add => this._messageUpdated.Register(value);
		remove => this._messageUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageUpdateEventArgs> _messageUpdated;

	/// <summary>
	///     Fired when a message is deleted.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessages" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageDeleteEventArgs> MessageDeleted
	{
		add => this._messageDeleted.Register(value);
		remove => this._messageDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageDeleteEventArgs> _messageDeleted;

	/// <summary>
	///     Fired when multiple messages are deleted at once.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessages" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageBulkDeleteEventArgs> MessagesBulkDeleted
	{
		add => this._messagesBulkDeleted.Register(value);
		remove => this._messagesBulkDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs> _messagesBulkDeleted;

	/// <summary>
	///     Fired when a message poll vote is added.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessagePolls" /> and
	///     <see cref="DiscordIntents.DirectMessagePolls" /> intent (depending on where u want to receive events from)
	///     specified in <seealso cref="DiscordConfiguration.Intents" />.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessagePollVoteAddEventArgs> MessagePollVoteAdded
	{
		add => this._messagePollVoteAdded.Register(value);
		remove => this._messagePollVoteAdded.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessagePollVoteAddEventArgs> _messagePollVoteAdded;

	/// <summary>
	///     Fired when a message poll vote is removed.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessagePolls" /> and
	///     <see cref="DiscordIntents.DirectMessagePolls" /> intent (depending on where u want to receive events from)
	///     specified in <seealso cref="DiscordConfiguration.Intents" />.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessagePollVoteRemoveEventArgs> MessagePollVoteRemoved
	{
		add => this._messagePollVoteRemoved.Register(value);
		remove => this._messagePollVoteRemoved.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessagePollVoteRemoveEventArgs> _messagePollVoteRemoved;

#endregion

#region Message Reaction

	/// <summary>
	///     Fired when a reaction gets added to a message.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessageReactions" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs> MessageReactionAdded
	{
		add => this._messageReactionAdded.Register(value);
		remove => this._messageReactionAdded.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageReactionAddEventArgs> _messageReactionAdded;

	/// <summary>
	///     Fired when a reaction gets removed from a message.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessageReactions" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs> MessageReactionRemoved
	{
		add => this._messageReactionRemoved.Register(value);
		remove => this._messageReactionRemoved.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs> _messageReactionRemoved;

	/// <summary>
	///     Fired when all reactions get removed from a message.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessageReactions" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs> MessageReactionsCleared
	{
		add => this._messageReactionsCleared.Register(value);
		remove => this._messageReactionsCleared.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageReactionsClearEventArgs> _messageReactionsCleared;

	/// <summary>
	///     Fired when all reactions of a specific reaction are removed from a message.
	///     For this Event you need the <see cref="DiscordIntents.GuildMessageReactions" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEmojiEventArgs> MessageReactionRemovedEmoji
	{
		add => this._messageReactionRemovedEmoji.Register(value);
		remove => this._messageReactionRemovedEmoji.Unregister(value);
	}

	private AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs> _messageReactionRemovedEmoji;

#endregion

#region Activities

	/// <summary>
	///     Fired when a embedded activity has been updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, EmbeddedActivityUpdateEventArgs> EmbeddedActivityUpdated
	{
		add => this._embeddedActivityUpdated.Register(value);
		remove => this._embeddedActivityUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, EmbeddedActivityUpdateEventArgs> _embeddedActivityUpdated;

#endregion

#region Presence/User Update

	/// <summary>
	///     Fired when a presence has been updated.
	///     For this Event you need the <see cref="DiscordIntents.GuildPresences" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, PresenceUpdateEventArgs> PresenceUpdated
	{
		add => this._presenceUpdated.Register(value);
		remove => this._presenceUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, PresenceUpdateEventArgs> _presenceUpdated;

	/// <summary>
	///     Fired when the current user updates their settings.
	///     For this Event you need the <see cref="DiscordIntents.GuildPresences" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, UserSettingsUpdateEventArgs> UserSettingsUpdated
	{
		add => this._userSettingsUpdated.Register(value);
		remove => this._userSettingsUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs> _userSettingsUpdated;

	/// <summary>
	///     Fired when properties about the current user change.
	/// </summary>
	/// <remarks>
	///     NB: This event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
	///     For this Event you need the <see cref="DiscordIntents.GuildPresences" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </remarks>
	public event AsyncEventHandler<DiscordClient, UserUpdateEventArgs> UserUpdated
	{
		add => this._userUpdated.Register(value);
		remove => this._userUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, UserUpdateEventArgs> _userUpdated;

#endregion

#region Stage Instance

	/// <summary>
	///     Fired when a Stage Instance is created.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, StageInstanceCreateEventArgs> StageInstanceCreated
	{
		add => this._stageInstanceCreated.Register(value);
		remove => this._stageInstanceCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, StageInstanceCreateEventArgs> _stageInstanceCreated;

	/// <summary>
	///     Fired when a Stage Instance is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, StageInstanceUpdateEventArgs> StageInstanceUpdated
	{
		add => this._stageInstanceUpdated.Register(value);
		remove => this._stageInstanceUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs> _stageInstanceUpdated;

	/// <summary>
	///     Fired when a Stage Instance is deleted.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, StageInstanceDeleteEventArgs> StageInstanceDeleted
	{
		add => this._stageInstanceDeleted.Register(value);
		remove => this._stageInstanceDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs> _stageInstanceDeleted;

#endregion

#region Thread

	/// <summary>
	///     Fired when a thread is created.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ThreadCreateEventArgs> ThreadCreated
	{
		add => this._threadCreated.Register(value);
		remove => this._threadCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ThreadCreateEventArgs> _threadCreated;

	/// <summary>
	///     Fired when a thread is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ThreadUpdateEventArgs> ThreadUpdated
	{
		add => this._threadUpdated.Register(value);
		remove => this._threadUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ThreadUpdateEventArgs> _threadUpdated;

	/// <summary>
	///     Fired when a thread is deleted.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ThreadDeleteEventArgs> ThreadDeleted
	{
		add => this._threadDeleted.Register(value);
		remove => this._threadDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ThreadDeleteEventArgs> _threadDeleted;

	/// <summary>
	///     Fired when a thread member is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ThreadListSyncEventArgs> ThreadListSynced
	{
		add => this._threadListSynced.Register(value);
		remove => this._threadListSynced.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ThreadListSyncEventArgs> _threadListSynced;

	/// <summary>
	///     Fired when a thread member is updated.
	///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ThreadMemberUpdateEventArgs> ThreadMemberUpdated
	{
		add => this._threadMemberUpdated.Register(value);
		remove => this._threadMemberUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs> _threadMemberUpdated;

	/// <summary>
	///     Fired when the thread members are updated.
	///     For this Event you need the <see cref="DiscordIntents.GuildMembers" /> or <see cref="DiscordIntents.Guilds" />
	///     intent specified in <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ThreadMembersUpdateEventArgs> ThreadMembersUpdated
	{
		add => this._threadMembersUpdated.Register(value);
		remove => this._threadMembersUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs> _threadMembersUpdated;

#endregion

#region Voice

	/// <summary>
	///     Fired when someone joins/leaves/moves voice channels.
	///     For this Event you need the <see cref="DiscordIntents.GuildVoiceStates" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, VoiceStateUpdateEventArgs> VoiceStateUpdated
	{
		add => this._voiceStateUpdated.Register(value);
		remove => this._voiceStateUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs> _voiceStateUpdated;

	/// <summary>
	///     Fired when a guild's voice server is updated.
	///     For this Event you need the <see cref="DiscordIntents.GuildVoiceStates" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, VoiceServerUpdateEventArgs> VoiceServerUpdated
	{
		add => this._voiceServerUpdated.Register(value);
		remove => this._voiceServerUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs> _voiceServerUpdated;

	/// <summary>
	///     Fired when a voice channel effect was send.
	///     For this Event you need the <see cref="DiscordIntents.GuildVoiceStates" /> intent specified in
	///     <seealso cref="DiscordConfiguration.Intents" />
	/// </summary>
	public event AsyncEventHandler<DiscordClient, VoiceChannelEffectSendEventArgs> VoiceChannelEffectSend
	{
		add => this._voiceChannelEffectSend.Register(value);
		remove => this._voiceChannelEffectSend.Unregister(value);
	}

	private AsyncEvent<DiscordClient, VoiceChannelEffectSendEventArgs> _voiceChannelEffectSend;

#endregion

#region Application

	/// <summary>
	///     Fired when a new application command is registered.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ApplicationCommandEventArgs> ApplicationCommandCreated
	{
		add => this._applicationCommandCreated.Register(value);
		remove => this._applicationCommandCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ApplicationCommandEventArgs> _applicationCommandCreated;

	/// <summary>
	///     Fired when an application command is updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ApplicationCommandEventArgs> ApplicationCommandUpdated
	{
		add => this._applicationCommandUpdated.Register(value);
		remove => this._applicationCommandUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ApplicationCommandEventArgs> _applicationCommandUpdated;

	/// <summary>
	///     Fired when an application command is deleted.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ApplicationCommandEventArgs> ApplicationCommandDeleted
	{
		add => this._applicationCommandDeleted.Register(value);
		remove => this._applicationCommandDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ApplicationCommandEventArgs> _applicationCommandDeleted;

	/// <summary>
	///     Fired when a new application command is registered.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, GuildApplicationCommandCountEventArgs> GuildApplicationCommandCountUpdated
	{
		add => this._guildApplicationCommandCountUpdated.Register(value);
		remove => this._guildApplicationCommandCountUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, GuildApplicationCommandCountEventArgs> _guildApplicationCommandCountUpdated;

	/// <summary>
	///     Fired when a user uses a context menu.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ContextMenuInteractionCreateEventArgs> ContextMenuInteractionCreated
	{
		add => this._contextMenuInteractionCreated.Register(value);
		remove => this._contextMenuInteractionCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs> _contextMenuInteractionCreated;

	/// <summary>
	///     Fired when application command permissions gets updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ApplicationCommandPermissionsUpdateEventArgs> ApplicationCommandPermissionsUpdated
	{
		add => this._applicationCommandPermissionsUpdated.Register(value);
		remove => this._applicationCommandPermissionsUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdateEventArgs> _applicationCommandPermissionsUpdated;

#endregion

#region Misc

	/// <summary>
	///     Fired when an interaction is invoked.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, InteractionCreateEventArgs> InteractionCreated
	{
		add => this._interactionCreated.Register(value);
		remove => this._interactionCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, InteractionCreateEventArgs> _interactionCreated;

	/// <summary>
	///     Fired when a component is invoked.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> ComponentInteractionCreated
	{
		add => this._componentInteractionCreated.Register(value);
		remove => this._componentInteractionCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs> _componentInteractionCreated;

	/// <summary>
	///     Fired when an entitlement was created.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, EntitlementCreateEventArgs> EntitlementCreated
	{
		add => this._entitlementCreated.Register(value);
		remove => this._entitlementCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, EntitlementCreateEventArgs> _entitlementCreated;

	/// <summary>
	///     Fired when an entitlement was updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, EntitlementUpdateEventArgs> EntitlementUpdated
	{
		add => this._entitlementUpdated.Register(value);
		remove => this._entitlementUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, EntitlementUpdateEventArgs> _entitlementUpdated;

	/// <summary>
	///     Fired when an entitlement was deleted.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, EntitlementDeleteEventArgs> EntitlementDeleted
	{
		add => this._entitlementDeleted.Register(value);
		remove => this._entitlementDeleted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, EntitlementDeleteEventArgs> _entitlementDeleted;

	/// <summary>
	///     Fired when an subscription was created.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, SubscriptionCreateEventArgs> SubscriptionCreated
	{
		add => this._subscriptionCreated.Register(value);
		remove => this._subscriptionCreated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, SubscriptionCreateEventArgs> _subscriptionCreated;

	/// <summary>
	///     Fired when an subscription was updated.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, SubscriptionUpdateEventArgs> SubscriptionUpdated
	{
		add => this._subscriptionUpdated.Register(value);
		remove => this._subscriptionUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, SubscriptionUpdateEventArgs> _subscriptionUpdated;

	/// <summary>
	///     Fired when a user starts typing in a channel.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, TypingStartEventArgs> TypingStarted
	{
		add => this._typingStarted.Register(value);
		remove => this._typingStarted.Unregister(value);
	}

	private AsyncEvent<DiscordClient, TypingStartEventArgs> _typingStarted;

	/// <summary>
	///     Fired when an unknown event gets received.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, UnknownEventArgs> UnknownEvent
	{
		add => this._unknownEvent.Register(value);
		remove => this._unknownEvent.Unregister(value);
	}

	private AsyncEvent<DiscordClient, UnknownEventArgs> _unknownEvent;

	/// <summary>
	///     Fired whenever webhooks update.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, WebhooksUpdateEventArgs> WebhooksUpdated
	{
		add => this._webhooksUpdated.Register(value);
		remove => this._webhooksUpdated.Unregister(value);
	}

	private AsyncEvent<DiscordClient, WebhooksUpdateEventArgs> _webhooksUpdated;

	/// <summary>
	///     Fired whenever an error occurs within an event handler.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ClientErrorEventArgs> ClientErrored
	{
		add => this._clientErrored.Register(value);
		remove => this._clientErrored.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ClientErrorEventArgs> _clientErrored;

#endregion

#region Error Handling

	/// <summary>
	///     Handles event errors.
	/// </summary>
	/// <param name="asyncEvent">The event.</param>
	/// <param name="ex">The exception.</param>
	/// <param name="handler">The event handler.</param>
	/// <param name="sender">The sender.</param>
	/// <param name="eventArgs">The event args.</param>
	internal void EventErrorHandler<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
		where TArgs : AsyncEventArgs
	{
		if (ex is AsyncEventTimeoutException)
		{
			this.Logger.LogWarning(LoggerEvents.EventHandlerException, "An event handler for {AsyncEventName} took too long to execute. Defined as \"{TrimStart}\" located in \"{MethodDeclaringType}\"", asyncEvent.Name, handler.Method.ToString()?.Replace(handler.Method.ReturnType.ToString(), "").TrimStart(), handler.Method.DeclaringType);
			return;
		}

		this.Logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {Name} thrown from {Method} (defined in {DeclaringType})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
		this._clientErrored.InvokeAsync(this, new(this.ServiceProvider)
		{
			EventName = asyncEvent.Name,
			Exception = ex
		}).ConfigureAwait(false).GetAwaiter().GetResult();
	}

	/// <summary>
	///     Fired when a ratelimit was hit.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, RateLimitExceptionEventArgs> RateLimitHit
	{
		add => this.RateLimitHitInternal.Register(value);
		remove => this.RateLimitHitInternal.Unregister(value);
	}

	internal AsyncEvent<DiscordClient, RateLimitExceptionEventArgs> RateLimitHitInternal;

	/// <summary>
	///     Fired on heartbeat attempt cancellation due to too many failed heartbeats.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, ZombiedEventArgs> Zombied
	{
		add => this._zombied.Register(value);
		remove => this._zombied.Unregister(value);
	}

	private AsyncEvent<DiscordClient, ZombiedEventArgs> _zombied;

	/// <summary>
	///     Fired when a gateway payload is received.
	///     Only fired when <see cref="DiscordConfiguration.EnableLibraryDeveloperMode" /> or
	///     <see cref="DiscordConfiguration.EnablePayloadReceivedEvent" /> is enabled.
	/// </summary>
	public event AsyncEventHandler<DiscordClient, PayloadReceivedEventArgs> PayloadReceived
	{
		add => this._payloadReceived.Register(value);
		remove => this._payloadReceived.Unregister(value);
	}

	private AsyncEvent<DiscordClient, PayloadReceivedEventArgs> _payloadReceived;

	/// <summary>
	///     Handles event handler exceptions.
	/// </summary>
	/// <param name="asyncEvent">The event.</param>
	/// <param name="ex">The exception.</param>
	/// <param name="handler">The event handler.</param>
	/// <param name="sender">The sender.</param>
	/// <param name="eventArgs">The event args.</param>
	private void Goof<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
		where TArgs : AsyncEventArgs => this.Logger.LogCritical(LoggerEvents.EventHandlerException, ex, "Exception event handler {0} (defined in {1}) threw an exception", handler.Method, handler.Method.DeclaringType);

#endregion
}

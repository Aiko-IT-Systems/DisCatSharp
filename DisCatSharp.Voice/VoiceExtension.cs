using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Voice.Entities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace DisCatSharp.Voice;

/// <summary>
///     Represents Voice extension, which acts as Discord voice client.
/// </summary>
public sealed class VoiceExtension : BaseExtension
{
	/// <summary>
	///     Gets or sets the active connections.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, VoiceConnection> _activeConnections;

	/// <summary>
	///     Gets or sets the configuration.
	/// </summary>
	private readonly VoiceConfiguration _configuration;

	/// <summary>
	///     Gets or sets the voice server updates.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> _voiceServerUpdates;

	/// <summary>
	///     Gets or sets the voice state updates.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> _voiceStateUpdates;

	/// <summary>
	///     Initializes a new instance of the <see cref="VoiceExtension" /> class.
	/// </summary>
	/// <param name="config">The config.</param>
	internal VoiceExtension(VoiceConfiguration config)
	{
		this._configuration = new(config);
		VoiceRuntimeLogging.EnableDebugLogs = this._configuration.EnableDebugLogging;
		this.IsIncomingEnabled = config.EnableIncoming;

		this._activeConnections = new();
		this._voiceStateUpdates = new();
		this._voiceServerUpdates = new();
	}

	/// <summary>
	///     Gets whether this connection has incoming voice enabled.
	/// </summary>
	public bool IsIncomingEnabled { get; }

	/// <summary>
	///     DO NOT USE THIS MANUALLY.
	/// </summary>
	/// <param name="client">DO NOT USE THIS MANUALLY.</param>
	/// <exception cref="InvalidOperationException" />
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;

		this.Client.VoiceStateUpdated += this.Client_VoiceStateUpdate;
		this.Client.VoiceServerUpdated += this.Client_VoiceServerUpdate;
	}

	/// <summary>
	///     Create a Voice connection for the specified channel.
	/// </summary>
	/// <param name="channel">Channel to connect to.</param>
	/// <returns>Voice connection for this channel.</returns>
	public async Task<VoiceConnection> ConnectAsync(DiscordChannel channel)
	{
		if (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage)
			throw new ArgumentException("Invalid channel specified; needs to be voice or stage channel", nameof(channel));

		if (channel.Guild == null)
			throw new ArgumentException("Invalid channel specified; needs to be guild channel", nameof(channel));

		if (!channel.PermissionsFor(channel.Guild.CurrentMember).HasPermission(Permissions.AccessChannels | Permissions.UseVoice))
			throw new InvalidOperationException("You need AccessChannels and UseVoice permission to connect to this voice channel");

		var gld = channel.Guild;
		if (this._activeConnections.ContainsKey(gld.Id))
			throw new InvalidOperationException("This guild already has a voice connection");

		var vstut = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
		var vsrut = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
		this._voiceStateUpdates[gld.Id] = vstut;
		this._voiceServerUpdates[gld.Id] = vsrut;

		var vsd = new VoiceDispatch
		{
			OpCode = 4,
			Payload = new VoiceStateUpdatePayload
			{
				GuildId = gld.Id,
				ChannelId = channel.Id,
				Deafened = false,
				Muted = false
			}
		};
		var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
		await (channel.Discord as DiscordClient).WsSendAsync(vsj).ConfigureAwait(false);

		var vstu = await vstut.Task.ConfigureAwait(false);
		var vstup = new VoiceStateUpdatePayload
		{
			SessionId = vstu.SessionId,
			UserId = vstu.User.Id
		};
		var vsru = await vsrut.Task.ConfigureAwait(false);
		var vsrup = new VoiceServerUpdatePayload
		{
			Endpoint = vsru.Endpoint,
			GuildId = vsru.Guild.Id,
			Token = vsru.VoiceToken
		};

		var vnc = new VoiceConnection(this.Client, gld, channel, this._configuration, vsrup, vstup);
		vnc.VoiceDisconnected += this.Vnc_VoiceDisconnected;
		await vnc.ConnectAsync().ConfigureAwait(false);
		await vnc.WaitForReadyAsync().ConfigureAwait(false);
		this._activeConnections[gld.Id] = vnc;
		return vnc;
	}

	/// <summary>
	///     Gets a Voice connection for specified guild.
	/// </summary>
	/// <param name="guild">Guild to get Voice connection for.</param>
	/// <returns>Voice connection for the specified guild.</returns>
	public VoiceConnection GetConnection(DiscordGuild guild) => this._activeConnections.TryGetValue(guild.Id, out var value) ? value : null;

	/// <summary>
	///     Vnc_S the voice disconnected.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <returns>A Task.</returns>
	private async Task Vnc_VoiceDisconnected(DiscordGuild guild)
	{
		if (this._activeConnections.ContainsKey(guild.Id))
			this._activeConnections.TryRemove(guild.Id, out _);

		var vsd = new VoiceDispatch
		{
			OpCode = 4,
			Payload = new VoiceStateUpdatePayload
			{
				GuildId = guild.Id,
				ChannelId = null
			}
		};
		var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
		await (guild.Discord as DiscordClient).WsSendAsync(vsj).ConfigureAwait(false);
	}

	/// <summary>
	///     Client_S the voice state update.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task Client_VoiceStateUpdate(DiscordClient client, VoiceStateUpdateEventArgs e)
	{
		var gld = e.Guild;
		if (gld is null)
			return Task.CompletedTask;

		if (e.User is null)
			return Task.CompletedTask;

		if (e.User.Id == this.Client.CurrentUser.Id)
		{
			if (e.After.Channel is null && this._activeConnections.TryRemove(gld.Id, out var ac))
				ac.Disconnect();

			if (this._activeConnections.TryGetValue(e.Guild.Id, out var vnc))
				vnc.TargetChannel = e.Channel;

			if (this._voiceStateUpdates.ContainsKey(gld.Id))
			{
				this.Client.Logger.VoiceDebug(VoiceEvents.VoiceHandshake,
					"[Voice] VOICE_STATE_UPDATE for bot: session_id={SessionId} channel={Channel} sessionNull={SNull} channelNull={CNull}",
					e.SessionId ?? "(null)", e.Channel?.Name ?? "(null)",
					string.IsNullOrWhiteSpace(e.SessionId), e.Channel is null);
			}

			if (!string.IsNullOrWhiteSpace(e.SessionId) && e.Channel is not null && this._voiceStateUpdates.TryRemove(gld.Id, out var xe))
				xe.SetResult(e);
		}

		return Task.CompletedTask;
	}

	/// <summary>
	///     Client_S the voice server update.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task Client_VoiceServerUpdate(DiscordClient client, VoiceServerUpdateEventArgs e)
	{
		var gld = e.Guild;
		if (gld is null)
			return Task.CompletedTask;

		if (this._activeConnections.TryGetValue(e.Guild.Id, out var vnc))
		{
			vnc.ServerData = new()
			{
				Endpoint = e.Endpoint,
				GuildId = e.Guild.Id,
				Token = e.VoiceToken
			};

			var eps = e.Endpoint;
			var epi = eps.LastIndexOf(':');
			string eph;
			var epp = 443;
			if (epi != -1)
			{
				eph = eps[..epi];
				epp = int.Parse(eps[(epi + 1)..]);
			}
			else
				eph = eps;

			vnc.WebSocketEndpoint = new()
			{
				Hostname = eph,
				Port = epp
			};

			vnc.Resume = false;
			_ = vnc.ReconnectAsync().ContinueWith(static (task, state) =>
				{
					if (task.Exception is not null && state is DiscordClient c)
						c.Logger.LogError(task.Exception, "[Voice] ReconnectAsync failed after VOICE_SERVER_UPDATE");
				},
				client,
				TaskScheduler.Default);
		}

		if (this._voiceServerUpdates.ContainsKey(gld.Id))
		{
			this._voiceServerUpdates.TryRemove(gld.Id, out var xe);
			xe?.SetResult(e);
		}

		return Task.CompletedTask;
	}
}


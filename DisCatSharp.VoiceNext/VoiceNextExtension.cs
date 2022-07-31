// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Net;
using DisCatSharp.VoiceNext.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext;

/// <summary>
/// Represents VoiceNext extension, which acts as Discord voice client.
/// </summary>
public sealed class VoiceNextExtension : BaseExtension
{
	/// <summary>
	/// Gets or sets the configuration.
	/// </summary>
	private readonly VoiceNextConfiguration _configuration;

	/// <summary>
	/// Gets or sets the active connections.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, VoiceNextConnection> _activeConnections;

	/// <summary>
	/// Gets or sets the voice state updates.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> _voiceStateUpdates;

	/// <summary>
	/// Gets or sets the voice server updates.
	/// </summary>
	private readonly ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> _voiceServerUpdates;

	/// <summary>
	/// Gets whether this connection has incoming voice enabled.
	/// </summary>
	public bool IsIncomingEnabled { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="VoiceNextExtension"/> class.
	/// </summary>
	/// <param name="config">The config.</param>
	internal VoiceNextExtension(VoiceNextConfiguration config)
	{
		this._configuration = new VoiceNextConfiguration(config);
		this.IsIncomingEnabled = config.EnableIncoming;

		this._activeConnections = new ConcurrentDictionary<ulong, VoiceNextConnection>();
		this._voiceStateUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>>();
		this._voiceServerUpdates = new ConcurrentDictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>>();
	}

	/// <summary>
	/// DO NOT USE THIS MANUALLY.
	/// </summary>
	/// <param name="client">DO NOT USE THIS MANUALLY.</param>
	/// <exception cref="System.InvalidOperationException"/>
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;

		this.Client.VoiceStateUpdated += this.Client_VoiceStateUpdate;
		this.Client.VoiceServerUpdated += this.Client_VoiceServerUpdate;
	}

	/// <summary>
	/// Create a VoiceNext connection for the specified channel.
	/// </summary>
	/// <param name="channel">Channel to connect to.</param>
	/// <returns>VoiceNext connection for this channel.</returns>
	public async Task<VoiceNextConnection> ConnectAsync(DiscordChannel channel)
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

		var vnc = new VoiceNextConnection(this.Client, gld, channel, this._configuration, vsrup, vstup);
		vnc.VoiceDisconnected += this.Vnc_VoiceDisconnected;
		await vnc.ConnectAsync().ConfigureAwait(false);
		await vnc.WaitForReadyAsync().ConfigureAwait(false);
		this._activeConnections[gld.Id] = vnc;
		return vnc;
	}

	/// <summary>
	/// Gets a VoiceNext connection for specified guild.
	/// </summary>
	/// <param name="guild">Guild to get VoiceNext connection for.</param>
	/// <returns>VoiceNext connection for the specified guild.</returns>
	public VoiceNextConnection GetConnection(DiscordGuild guild) => this._activeConnections.ContainsKey(guild.Id) ? this._activeConnections[guild.Id] : null;

	/// <summary>
	/// Vnc_S the voice disconnected.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <returns>A Task.</returns>
	private async Task Vnc_VoiceDisconnected(DiscordGuild guild)
	{
		VoiceNextConnection vnc = null;
		if (this._activeConnections.ContainsKey(guild.Id))
			this._activeConnections.TryRemove(guild.Id, out vnc);

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
	/// Client_S the voice state update.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private Task Client_VoiceStateUpdate(DiscordClient client, VoiceStateUpdateEventArgs e)
	{
		var gld = e.Guild;
		if (gld == null)
			return Task.CompletedTask;

		if (e.User == null)
			return Task.CompletedTask;

		if (e.User.Id == this.Client.CurrentUser.Id)
		{
			if (e.After.Channel == null && this._activeConnections.TryRemove(gld.Id, out var ac))
				ac.Disconnect();

			if (this._activeConnections.TryGetValue(e.Guild.Id, out var vnc))
				vnc.TargetChannel = e.Channel;

			if (!string.IsNullOrWhiteSpace(e.SessionId) && e.Channel != null && this._voiceStateUpdates.TryRemove(gld.Id, out var xe))
				xe.SetResult(e);
		}

		return Task.CompletedTask;
	}

	/// <summary>
	/// Client_S the voice server update.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="e">The e.</param>
	/// <returns>A Task.</returns>
	private async Task Client_VoiceServerUpdate(DiscordClient client, VoiceServerUpdateEventArgs e)
	{
		var gld = e.Guild;
		if (gld == null)
			return;

		if (this._activeConnections.TryGetValue(e.Guild.Id, out var vnc))
		{
			vnc.ServerData = new VoiceServerUpdatePayload
			{
				Endpoint = e.Endpoint,
				GuildId = e.Guild.Id,
				Token = e.VoiceToken
			};

			var eps = e.Endpoint;
			var epi = eps.LastIndexOf(':');
			var eph = string.Empty;
			var epp = 443;
			if (epi != -1)
			{
				eph = eps[..epi];
				epp = int.Parse(eps[(epi + 1)..]);
			}
			else
			{
				eph = eps;
			}
			vnc.WebSocketEndpoint = new ConnectionEndpoint { Hostname = eph, Port = epp };

			vnc.Resume = false;
			await vnc.ReconnectAsync().ConfigureAwait(false);
		}

		if (this._voiceServerUpdates.ContainsKey(gld.Id))
		{
			this._voiceServerUpdates.TryRemove(gld.Id, out var xe);
			xe.SetResult(e);
		}
	}
}

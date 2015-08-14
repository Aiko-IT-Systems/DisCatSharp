using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink.EventArgs;
using DisCatSharp.Net;

namespace DisCatSharp.Lavalink;

/// <summary>
/// Represents the lavalink extension.
/// </summary>
public sealed class LavalinkExtension : BaseExtension
{
	/// <summary>
	/// Triggered whenever a session disconnects.
	/// </summary>
	public event AsyncEventHandler<LavalinkExtension, LavalinkSessionDisconnectedEventArgs> SessionDisconnected
	{
		add => this._sessionDisconnected.Register(value);
		remove => this._sessionDisconnected.Unregister(value);
	}

	private AsyncEvent<LavalinkExtension, LavalinkSessionDisconnectedEventArgs> _sessionDisconnected;

	/// <summary>
	/// Triggered whenever a session connects.
	/// </summary>
	public event AsyncEventHandler<LavalinkExtension, LavalinkSessionConnectedEventArgs> SessionConnected
	{
		add => this._sessionConnected.Register(value);
		remove => this._sessionConnected.Unregister(value);
	}

	private AsyncEvent<LavalinkExtension, LavalinkSessionConnectedEventArgs> _sessionConnected;

	/// <summary>
	/// Gets a dictionary of connected Lavalink sessions for the extension.
	/// </summary>
	public IReadOnlyDictionary<ConnectionEndpoint, LavalinkSession> ConnectedSessions { get; }

	/// <summary>
	/// The internal dictionary of connected Lavalink sessions.
	/// </summary>
	private readonly ConcurrentDictionary<ConnectionEndpoint, LavalinkSession> _connectedSessions = new();

	/// <summary>
	/// Gets the rest client used to communicate with the lavalink server.
	/// </summary>
	private LavalinkRestClient REST { get; set; } = null!;

	/// <summary>
	/// Gets the lavalink configuration.
	/// </summary>
	private LavalinkConfiguration CONFIGURATION { get; set; } = null!;

	/// <summary>
	/// Creates a new instance of this Lavalink extension.
	/// </summary>
	internal LavalinkExtension()
	{
		this.ConnectedSessions = new ReadOnlyConcurrentDictionary<ConnectionEndpoint, LavalinkSession>(this._connectedSessions);
	}

	/// <summary>
	/// DO NOT USE THIS MANUALLY.
	/// </summary>
	/// <param name="client">DO NOT USE THIS MANUALLY.</param>
	/// <exception cref="InvalidOperationException">Thrown when a developer tries to manually initialize it.</exception>
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;
		this._sessionDisconnected = new("LAVALINK_SESSION_DISCONNECTED", TimeSpan.Zero, this.Client.EventErrorHandler);
		this._sessionConnected = new("LAVALINK_SESSION_CONNECTED", TimeSpan.Zero, this.Client.EventErrorHandler);
	}

	/// <summary>
	/// Connect to a Lavalink session.
	/// </summary>
	/// <param name="config">Lavalink client configuration.</param>
	/// <returns>The established Lavalink connection.</returns>
	public async Task<LavalinkSession> ConnectAsync(LavalinkConfiguration config)
	{
		if (this._connectedSessions.TryGetValue(config.SocketEndpoint, out var session))
			return session;

		this.CONFIGURATION ??= config;

		try
		{
			this.REST ??= new(this.CONFIGURATION, this.Client);

			var versionInfo = await this.REST.GetVersionAsync().ConfigureAwait(false);
			if (!versionInfo.Headers.Contains("Lavalink-Api-Version"))
				throw new("Lavalink v4 is required");

			if (versionInfo.Headers.TryGetValues("Lavalink-Api-Version", out var headerValues))
				if (headerValues.First() != "4")
					throw new("Lavalink v4 is required");
		}
		catch (Exception)
		{
			throw new("Something went wrong when determining the lavalink server version :/");
		}

		var con = new LavalinkSession(this.Client, this, config);
		con.SessionDisconnected += this.LavalinkSessionDisconnect;
		con.LavalinkSessionConnected += this.LavalinkSessionConnected;
		con.LavalinkSessionDisconnected += this.LavalinkSessionDisconnected;
		this._connectedSessions[con.NodeEndpoint] = con;
		try
		{
			await con.EstablishConnectionAsync().ConfigureAwait(false);
		}
		catch
		{
			this.LavalinkSessionDisconnect(con);
			throw;
		}

		return con;
	}

	/// <summary>
	/// Gets the Lavalink session connection for the specified endpoint.
	/// </summary>
	/// <param name="endpoint">Endpoint at which the session resides.</param>
	/// <returns>Lavalink session connection.</returns>
	public LavalinkSession? GetSession(ConnectionEndpoint endpoint)
		=> this._connectedSessions.TryGetValue(endpoint, out var ep) ? ep : null;

	/// <summary>
	/// Gets a Lavalink session connection based on load balancing and an optional voice region.
	/// </summary>
	/// <param name="region">The region to compare with the session's <see cref="LavalinkConfiguration.Region"/>, if any.</param>
	/// <returns>The least load affected session connection, or null if no sessions are present.</returns>
	public LavalinkSession? GetIdealSession(DiscordVoiceRegion? region = null)
	{
		if (this._connectedSessions.Count <= 1)
			return this._connectedSessions.Values.FirstOrDefault()!;

		var nodes = this._connectedSessions.Values.ToArray();

		if (region is null)
			return this.FilterByLoad(nodes);

		var regionPredicate = new Func<LavalinkSession, bool>(x => x.Region == region);

		if (nodes.Any(regionPredicate))
			nodes = nodes.Where(regionPredicate).ToArray();

		return nodes.Length <= 1 ? nodes.FirstOrDefault()! : this.FilterByLoad(nodes);
	}

	/// <summary>
	/// Gets a Lavalink guild player from a <see cref="DiscordGuild"/>.
	/// </summary>
	/// <param name="guild">The guild the player is on.</param>
	/// <returns>The found guild player, or null if one could not be found.</returns>
	public LavalinkGuildPlayer? GetGuildPlayer(DiscordGuild guild)
	{
		var nodes = this._connectedSessions.Values;
		var node = nodes.FirstOrDefault(x => x.ConnectedPlayersInternal.ContainsKey(guild.Id));
		return node?.GetGuildPlayer(guild);
	}

	/// <summary>
	/// Filters the by load.
	/// </summary>
	/// <param name="sessions">The sessions.</param>
	private LavalinkSession FilterByLoad(LavalinkSession[] sessions)
	{
		Array.Sort(sessions, (a, b) =>
		{
			//player count
			var aPenaltyCount = a.Statistics.PlayingPlayers;
			var bPenaltyCount = b.Statistics.PlayingPlayers;

			//cpu load
			aPenaltyCount += (int)Math.Pow(1.05d, (100 * (a.Statistics.Cpu.SystemLoad / a.Statistics.Cpu.Cores) * 10) - 10);
			bPenaltyCount += (int)Math.Pow(1.05d, (100 * (b.Statistics.Cpu.SystemLoad / a.Statistics.Cpu.Cores) * 10) - 10);

			//frame load
			if (a.Statistics.Frames!.Deficit > 0)
			{
				//deficit frame load
				aPenaltyCount += (int)((Math.Pow(1.03d, 500f * (a.Statistics.Frames.Deficit / 3000f)) * 600) - 600);

				//null frame load
				aPenaltyCount += (int)((Math.Pow(1.03d, 500f * (a.Statistics.Frames.Deficit / 3000f)) * 300) - 300);
			}

			//frame load
			if (b.Statistics.Frames!.Deficit > 0)
			{
				//deficit frame load
				bPenaltyCount += (int)((Math.Pow(1.03d, 500f * (b.Statistics.Frames.Deficit / 3000f)) * 600) - 600);

				//null frame load
				bPenaltyCount += (int)((Math.Pow(1.03d, 500f * (b.Statistics.Frames.Deficit / 3000f)) * 300) - 300);
			}

			return aPenaltyCount - bPenaltyCount;
		});

		return sessions[0];
	}

	/// <summary>
	/// Fired when a session disconnected and removes it from <see cref="ConnectedSessions"/>.
	/// </summary>
	/// <param name="session">The disconnected session.</param>
	private void LavalinkSessionDisconnect(LavalinkSession session)
		=> this._connectedSessions.TryRemove(session.NodeEndpoint, out _);

	/// <summary>
	/// Fired when a session disconnected.
	/// </summary>
	/// <param name="session">The disconnected session.</param>
	/// <param name="args">The event args.</param>
	private Task LavalinkSessionDisconnected(LavalinkSession session, LavalinkSessionDisconnectedEventArgs args)
		=> this._sessionDisconnected.InvokeAsync(this, args);

	/// <summary>
	/// Fired when a session connected.
	/// </summary>
	/// <param name="session">The connected session.</param>
	/// <param name="args">The event args.</param>
	private Task LavalinkSessionConnected(LavalinkSession session, LavalinkSessionConnectedEventArgs args)
		=> this._sessionConnected.InvokeAsync(this, args);
}

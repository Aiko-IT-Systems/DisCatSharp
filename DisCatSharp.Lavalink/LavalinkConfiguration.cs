using System;
using System.Net;

using DisCatSharp.Entities;
using DisCatSharp.Net;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Lavalink;

/// <summary>
/// Lavalink connection configuration.
/// </summary>
public sealed class LavalinkConfiguration
{
	/// <summary>
	/// Sets the endpoint for Lavalink REST.
	/// <para>Defaults to <c>127.0.0.1</c> on port <c>2333</c>.</para>
	/// </summary>
	public ConnectionEndpoint RestEndpoint { internal get; set; } = new("127.0.0.1", 2333);

	/// <summary>
	/// Sets the endpoint for the Lavalink Websocket connection.
	/// <para>Defaults to <c>127.0.0.1</c> on port <c>2333</c>.</para>
	/// </summary>
	public ConnectionEndpoint SocketEndpoint { internal get; set; } = new("127.0.0.1", 2333);

	/// <summary>
	/// <para>Sets the proxy to use for HTTP and WebSocket connections to Lavalink.</para>
	/// <para>Defaults to <see langword="null"/>.</para>
	/// </summary>
	public IWebProxy Proxy { internal get; set; } = null!;

	/// <summary>
	/// <para>Sets the timeout for HTTP requests.</para>
	/// <para>Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts.</para>
	/// <para>Defaults to <c>20</c> seconds.</para>
	/// </summary>
	public TimeSpan HttpTimeout { internal get; set; } = TimeSpan.FromMinutes(2);

	/// <summary>
	/// Sets whether the connection wrapper should attempt automatic reconnects should the connection drop.
	/// <para>Defaults to true.</para>
	/// </summary>
	public bool SocketAutoReconnect { internal get; set; } = true;

	/// <summary>
	/// Sets the password for the Lavalink connection.
	/// <para>Defaults to <c>youshallnotpass</c>.</para>
	/// </summary>
	public string Password { internal get; set; } = "youshallnotpass";

	/// <summary>
	/// Sets the resume key for the Lavalink connection.
	/// <para>This will allow existing voice sessions to continue for a certain time after the client is disconnected.</para>
	/// </summary>
	internal string? SessionId { get; set; }

	/// <summary>
	/// Sets the time in seconds when all voice sessions are closed after the client disconnects.
	/// <para>Defaults to <c>60</c> seconds.</para>
	/// </summary>
	public int ResumeTimeout { internal get; set; } = 60;

	/// <summary>
	/// Sets the time in milliseconds to wait for Lavalink's voice WebSocket to close after leaving a voice channel.
	/// <para>This will be the delay before the guild connection is removed.</para>
	/// <para>Defaults to <c>3000</c> milliseconds.</para>
	/// </summary>
	public int WebSocketCloseTimeout { internal get; set; } = 3000;

	/// <summary>
	/// Sets the voice region ID for the Lavalink connection.
	/// <para>This should be used if nodes should be filtered by region with <see cref="LavalinkExtension.GetIdealSession(DiscordVoiceRegion)"/>.</para>
	/// </summary>
	public DiscordVoiceRegion Region { internal get; set; }

	/// <summary>
	/// Sets whether the built in queue system should be used.
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool EnableBuiltInQueueSystem { internal get; set; } = false;

	/// <summary>
	/// Sets whether trace should be enabled for more detailed error responses.
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool EnableTrace { internal get; set; } = false;

	/// <summary>
	/// Sets the default volume level players will be created with.
	/// <para>Defaults to <c>70</c> percent.</para>
	/// <para>Min <c>0</c>, max <c>1000</c>.</para>
	/// </summary>
	public int DefaultVolume { internal get; set; } = 70;

	/// <summary>
	/// Creates a new instance of <see cref="LavalinkConfiguration"/>.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public LavalinkConfiguration()
	{ }

	/// <summary>
	/// Creates a new instance of <see cref="LavalinkConfiguration"/>, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Config the properties of which are to be copied.</param>
	public LavalinkConfiguration(LavalinkConfiguration other)
	{
		this.RestEndpoint = new()
		{
			Hostname = other.RestEndpoint.Hostname,
			Port = other.RestEndpoint.Port,
			Secured = other.RestEndpoint.Secured
		};
		this.SocketEndpoint = new()
		{
			Hostname = other.SocketEndpoint.Hostname,
			Port = other.SocketEndpoint.Port,
			Secured = other.SocketEndpoint.Secured
		};
		this.Password = other.Password;
		this.SessionId = other.SessionId;
		this.ResumeTimeout = other.ResumeTimeout;
		this.SocketAutoReconnect = other.SocketAutoReconnect;
		this.Region = other.Region;
		this.WebSocketCloseTimeout = other.WebSocketCloseTimeout;
		this.Proxy = other.Proxy;
		this.HttpTimeout = other.HttpTimeout;
		this.EnableBuiltInQueueSystem = other.EnableBuiltInQueueSystem;
		this.DefaultVolume = other.DefaultVolume;
		this.EnableTrace = other.EnableTrace;
	}
}

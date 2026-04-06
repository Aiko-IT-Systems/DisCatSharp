using System;

namespace DisCatSharp.Lavalink.Bridge;

/// <summary>
///     Configuration for the optional Lavalink transport bridge.
/// </summary>
/// <remarks>
///     When enabled, Lavalink produces Opus frames and exports them over a WebSocket bridge
///     to DisCatSharp.Voice, which handles Discord voice transport (RTP, DAVE E2EE, AEAD, UDP).
/// </remarks>
public sealed class LavalinkBridgeConfiguration
{
	/// <summary>
	///     Creates a new instance of <see cref="LavalinkBridgeConfiguration" />.
	/// </summary>
	public LavalinkBridgeConfiguration()
	{ }

	/// <summary>
	///     Creates a new instance of <see cref="LavalinkBridgeConfiguration" />, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public LavalinkBridgeConfiguration(LavalinkBridgeConfiguration other)
	{
		this.EnableExternalVoiceBridge = other.EnableExternalVoiceBridge;
		this.BridgeEndpoint = other.BridgeEndpoint;
		this.BridgeAuthToken = other.BridgeAuthToken;
		this.ReconnectDelay = other.ReconnectDelay;
		this.MaxReconnectAttempts = other.MaxReconnectAttempts;
	}

	/// <summary>
	///     <para>Enables the external voice bridge mode.</para>
	///     <para>When enabled, DisCatSharp.Lavalink connects to the Lavalink bridge WebSocket
	///     and routes Opus frames through DisCatSharp.Voice instead of Lavalink's built-in Koe transport.</para>
	///     <para>Defaults to false.</para>
	/// </summary>
	public bool EnableExternalVoiceBridge { get; set; } = false;

	/// <summary>
	///     <para>The WebSocket endpoint for the Lavalink bridge.</para>
	///     <para>Example: <c>ws://localhost:2335/bridge/v1</c></para>
	///     <para>Required when <see cref="EnableExternalVoiceBridge" /> is true.</para>
	/// </summary>
	public Uri? BridgeEndpoint { get; set; }

	/// <summary>
	///     <para>The authentication token for the bridge WebSocket connection.</para>
	///     <para>Sent as <c>Authorization: Bearer {token}</c> header.</para>
	///     <para>Required when <see cref="EnableExternalVoiceBridge" /> is true.</para>
	/// </summary>
	public string? BridgeAuthToken { get; set; }

	/// <summary>
	///     <para>Delay between bridge reconnection attempts.</para>
	///     <para>Defaults to 5 seconds.</para>
	/// </summary>
	public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);

	/// <summary>
	///     <para>Maximum number of reconnection attempts before giving up.</para>
	///     <para>Set to -1 for unlimited retries. Defaults to 10.</para>
	/// </summary>
	public int MaxReconnectAttempts { get; set; } = 10;

	/// <summary>
	///     Validates this configuration for use.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when required properties are missing.</exception>
	internal void Validate()
	{
		if (!this.EnableExternalVoiceBridge)
			return;

		if (this.BridgeEndpoint is null)
			throw new InvalidOperationException("BridgeEndpoint must be set when EnableExternalVoiceBridge is true.");

		if (string.IsNullOrWhiteSpace(this.BridgeAuthToken))
			throw new InvalidOperationException("BridgeAuthToken must be set when EnableExternalVoiceBridge is true.");
	}
}

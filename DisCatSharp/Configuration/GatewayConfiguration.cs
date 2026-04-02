using System;

using DisCatSharp.Enums;
using DisCatSharp.Net.Udp;
using DisCatSharp.Net.WebSocket;

namespace DisCatSharp;

/// <summary>
///     Configuration for Discord gateway connection settings.
/// </summary>
public sealed class GatewayConfiguration
{
	/// <summary>
	///     Creates a new gateway configuration with default values.
	/// </summary>
	public GatewayConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another gateway configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public GatewayConfiguration(GatewayConfiguration other)
	{
		this.AutoReconnect = other.AutoReconnect;
		this.ReconnectIndefinitely = other.ReconnectIndefinitely;
		this.ShardId = other.ShardId;
		this.ShardCount = other.ShardCount;
		this.CompressionLevel = other.CompressionLevel;
		this.LargeThreshold = other.LargeThreshold;
		this.Capabilities = other.Capabilities;
		this.MobileStatus = other.MobileStatus;
		this.WebSocketClientFactory = other.WebSocketClientFactory;
		this.UdpClientFactory = other.UdpClientFactory;
		this.Advanced = new(other.Advanced);
	}

	/// <summary>
	///     <para>Sets whether to automatically reconnect in case a connection is lost.</para>
	///     <para>Defaults to <see langword="true" />.</para>
	/// </summary>
	public bool AutoReconnect { internal get; set; } = true;

	/// <summary>
	///     <para>Defines that the client should attempt to reconnect indefinitely.</para>
	///     <para>This is typically a very bad idea to set to <c>true</c>, as it will swallow all connection errors.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool ReconnectIndefinitely { internal get; set; } = false;

	/// <summary>
	///     <para>Sets the ID of the shard to connect to.</para>
	///     <para>If not sharding, or sharding automatically, this value should be left with the default value of 0.</para>
	///     <para>Must be non-negative and less than <see cref="ShardCount" />. The cross-constraint against
	///     <see cref="ShardCount" /> is enforced at connection time — this setter only guards against negative values
	///     since the two properties may be assigned in any order during configuration.</para>
	/// </summary>
	public int ShardId
	{
		internal get;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "ShardId must be greater than or equal to 0.");

			field = value;
		}
	} = 0;

	/// <summary>
	///     <para>
	///         Sets the total number of shards the bot is on. If not sharding, this value should be left with a default
	///         value of 1.
	///     </para>
	///     <para>
	///         If sharding automatically, this value will indicate how many shards to boot. If left default for automatic
	///         sharding, the client will determine the shard count automatically.
	///     </para>
	///     <para>Must be greater than 0. The cross-constraint that <see cref="ShardId" /> must be less than this value
	///     is enforced at connection time.</para>
	/// </summary>
	public int ShardCount
	{
		internal get;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "ShardCount must be greater than 0.");

			field = value;
		}
	} = 1;

	/// <summary>
	///     <para>Sets the level of compression for WebSocket traffic.</para>
	///     <para>
	///         Disabling this option will increase the amount of traffic sent via WebSocket. Setting
	///         <see cref="GatewayCompressionLevel.Payload" /> will enable compression for READY and GUILD_CREATE payloads.
	///         Setting <see cref="GatewayCompressionLevel.Stream" /> will enable compression for the entire WebSocket stream,
	///         drastically reducing amount of traffic.
	///     </para>
	///     <para>Defaults to <see cref="GatewayCompressionLevel.Stream" />.</para>
	/// </summary>
	public GatewayCompressionLevel CompressionLevel { internal get; set; } = GatewayCompressionLevel.Stream;

	/// <summary>
	///     <para>Sets the member count threshold at which guilds are considered large.</para>
	///     <para>Discord only accepts values between 50 and 250 (inclusive). Values outside this range are silently
	///     clamped to the nearest boundary.</para>
	///     <para>Defaults to 250.</para>
	/// </summary>
	public int LargeThreshold
	{
		internal get;
		set => field = Math.Clamp(value, 50, 250);
	} = 250;

	/// <summary>
	///     Gets or sets the gateway capabilities for this connection.
	/// </summary>
	public GatewayCapabilities Capabilities { internal get; set; } = GatewayCapabilities.None;

	/// <summary>
	///     <para>Sets if the bot's status should show the mobile icon.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool MobileStatus { internal get; set; } = false;

	/// <summary>
	///     <para>Sets the factory method used to create instances of WebSocket clients.</para>
	///     <para>
	///         Use <see cref="WebSocketClient.CreateNew" /> and equivalents on other implementations to switch out client
	///         implementations.
	///     </para>
	///     <para>Defaults to <see cref="WebSocketClient.CreateNew" />.</para>
	/// </summary>
	public WebSocketClientFactoryDelegate WebSocketClientFactory
	{
		internal get;
		set => field = value ?? throw new InvalidOperationException("You need to supply a valid WebSocket client factory method.");
	} = WebSocketClient.CreateNew;

	/// <summary>
	///     <para>Sets the factory method used to create instances of UDP clients.</para>
	///     <para>
	///         Use <see cref="DcsUdpClient.CreateNew" /> and equivalents on other implementations to switch out client
	///         implementations.
	///     </para>
	///     <para>Defaults to <see cref="DcsUdpClient.CreateNew" />.</para>
	/// </summary>
	public UdpClientFactoryDelegate UdpClientFactory
	{
		internal get;
		set => field = value ?? throw new InvalidOperationException("You need to supply a valid UDP client factory method.");
	} = DcsUdpClient.CreateNew;

	/// <summary>
	///     <para>Advanced transport tuning options.</para>
	///     <para>Most applications should not need to modify these settings.</para>
	/// </summary>
	public GatewayAdvancedConfiguration Advanced { internal get; set; } = new();

	/// <summary>
	///     Validates cross-property constraints that cannot be checked in individual property setters because the two
	///     properties may be assigned in any order during object initializer construction.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	///     Thrown when <see cref="ShardId" /> is not strictly less than <see cref="ShardCount" />, which would cause
	///     the gateway to reject the IDENTIFY payload with a cryptic <c>4010 Invalid Shard</c> close code.
	/// </exception>
	internal void Validate()
	{
		if (this.ShardId >= this.ShardCount)
			throw new InvalidOperationException(
				$"ShardId ({this.ShardId}) must be less than ShardCount ({this.ShardCount}). " +
				"Shard IDs are zero-based, so a ShardCount of N allows ShardId values 0 through N-1.");
	}
}

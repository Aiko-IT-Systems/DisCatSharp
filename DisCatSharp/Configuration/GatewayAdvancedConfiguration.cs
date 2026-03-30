using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
///     Advanced configuration for Discord gateway transport tuning.
/// </summary>
/// <remarks>
///     Properties in this class control low-level transport behavior and should rarely need adjustment.
///     Additional timeout and tuning knobs will be added here in later phases.
/// </remarks>
public sealed class GatewayAdvancedConfiguration
{
	/// <summary>
	///     Creates a new gateway advanced configuration with default values.
	/// </summary>
	public GatewayAdvancedConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another gateway advanced configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public GatewayAdvancedConfiguration(GatewayAdvancedConfiguration other)
	{
		this.DispatchMode = other.DispatchMode;
		this.DispatchQueueCapacity = other.DispatchQueueCapacity;
	}

	/// <summary>
	///     <para>Controls how user event handlers are dispatched after ordered internal processing.</para>
	///     <para>
	///         Internal cache and state mutations are always processed in FIFO order per shard, regardless of this setting.
	///         This only affects when and how user-facing event handlers are invoked.
	///     </para>
	///     <para>Defaults to <see cref="GatewayDispatchMode.ConcurrentHandlers" />.</para>
	/// </summary>
	public GatewayDispatchMode DispatchMode { internal get; set; } = GatewayDispatchMode.ConcurrentHandlers;

	/// <summary>
	///     <para>Sets the bounded capacity of the internal dispatch queue per shard.</para>
	///     <para>
	///         When the queue is full, back-pressure will be applied to the WebSocket reader, which may cause
	///         the gateway to close the connection if heartbeat ACKs are delayed. Increase this value if your
	///         bot processes events slowly and receives high event volume.
	///     </para>
	///     <para>Defaults to 10,000. Set to 0 for an unbounded queue.</para>
	/// </summary>
	public int DispatchQueueCapacity
	{
		internal get;
		set
		{
			if (value < 0)
				throw new System.ArgumentOutOfRangeException(nameof(value), value, "DispatchQueueCapacity must be 0 (unbounded) or a positive integer.");

			field = value;
		}
	} = 10_000;
}

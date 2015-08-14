using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Uses amplification to create a shuddering effect, where the volume quickly oscillates.
/// </summary>
public sealed class LavalinkTremolo
{
	/// <summary>
	/// The frequency. (<c>>0.0</c>)
	/// </summary>
	[JsonProperty("frequency")]
	public Optional<float> Frequency { get; set; }

	/// <summary>
	/// The tromelo depth. (<c>>0.0</c>)
	/// </summary>
	[JsonProperty("depth")]
	public Optional<float> Depth { get; set; }

	/// <inheritdoc cref="LavalinkTremolo"/>
	/// <param name="frequency">The frequency. (<c>>0.0</c>)</param>
	/// <param name="depth">The tromelo depth. (<c>>0.0</c>)</param>
	public LavalinkTremolo(Optional<float> frequency, Optional<float> depth)
	{
		this.Frequency = frequency;
		this.Depth = depth;
	}
}

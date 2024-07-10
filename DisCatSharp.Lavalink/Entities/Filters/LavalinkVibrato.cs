using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Similar to tremolo. While tremolo oscillates the volume, vibrato oscillates the pitch.
/// </summary>
public sealed class LavalinkVibrato
{
	/// <summary>
	/// The frequency. (<c>>0.0</c>)
	/// </summary>
	[JsonProperty("frequency")]
	public Optional<float> Frequency { get; set; }

	/// <summary>
	/// The vibrato depth. (<c>>0.0</c>)
	/// </summary>
	[JsonProperty("depth")]
	public Optional<float> Depth { get; set; }

	/// <inheritdoc cref="LavalinkVibrato"/>
	/// <param name="frequency">The frequency. (<c>>0.0</c>)</param>
	/// <param name="depth">The vibrato depth. (<c>>0.0</c>)</param>
	public LavalinkVibrato(Optional<float> frequency, Optional<float> depth)
	{
		this.Frequency = frequency;
		this.Depth = depth;
	}
}

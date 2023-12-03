using DisCatSharp.Lavalink.Enums.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// There are 15 bands (0-14) that can be changed. "gain" is the multiplier for the given band. The default value is 0. Valid values range from -0.25 to 1.0, where -0.25 means the given band is completely muted, and 0.25 means it is doubled. Modifying the gain could also change the volume of the output.
/// </summary>
public sealed class LavalinkEqualizer
{
	/// <summary>
	/// The band.
	/// </summary>
	[JsonProperty("band")]
	public LavalinkFilterBand Band { get; set; }

	/// <summary>
	/// The gain (<c>-0.25</c> to <c>1.0</c>)
	/// </summary>
	[JsonProperty("band")]
	public float Gain { get; set; }

	/// <inheritdoc cref="LavalinkEqualizer"/>
	/// <param name="band">The band</param>
	/// <param name="gain">The gain (<c>-0.25</c> to <c>1.0</c>)</param>
	public LavalinkEqualizer(LavalinkFilterBand band, float gain)
	{
		this.Band = band;
		this.Gain = gain;
	}
}

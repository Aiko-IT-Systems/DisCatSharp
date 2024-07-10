using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Changes the speed, pitch, and rate.
/// </summary>
public sealed class LavalinkTimescale
{
	/// <summary>
	/// The playback speed. (<c>>0.0</c> where <c>1.0</c> is default)
	/// </summary>
	[JsonProperty("speed")]
	public Optional<float> Speed { get; set; }

	/// <summary>
	/// The pitch. (<c>>0.0</c> where <c>1.0</c> is default)
	/// </summary>
	[JsonProperty("pitch")]
	public Optional<float> Pitch { get; set; }

	/// <summary>
	/// The rate. (<c>>0.0</c> where <c>1.0</c> is default)
	/// </summary>
	[JsonProperty("rate")]
	public Optional<float> Rate { get; set; }

	/// <inheritdoc cref="LavalinkTimescale"/>
	/// <param name="speed">The playback speed. (<c>>0.0</c> where <c>1.0</c> is default)</param>
	/// <param name="pitch">The pitch. (<c>>0.0</c> where <c>1.0</c> is default)</param>
	/// <param name="rate">The rate. (<c>>0.0</c> where <c>1.0</c> is default)</param>
	public LavalinkTimescale(Optional<float> speed, Optional<float> pitch, Optional<float> rate)
	{
		this.Speed = speed;
		this.Pitch = pitch;
		this.Rate = rate;
	}
}

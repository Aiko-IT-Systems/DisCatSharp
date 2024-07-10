using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Higher frequencies get suppressed, while lower frequencies pass through this filter, thus the name low pass.
/// </summary>
public sealed class LavalinkLowPass
{
	/// <summary>
	/// The smoothing factor (<c>>1.0</c>, Any smoothing values equal to or less than 1.0 will disable the filter)
	/// </summary>
	[JsonProperty("smoothing")]
	public Optional<float> Smoothing { get; set; }

	/// <inheritdoc cref="LavalinkLowPass"/>
	/// <param name="smoothing">The smoothing factor</param>
	public LavalinkLowPass(Optional<float> smoothing)
	{
		this.Smoothing = smoothing;
	}
}

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Rotates the sound around the stereo channels/user headphones (aka Audio Panning).
/// </summary>
public sealed class LavalinkRotation
{
	/// <summary>
	/// The frequency of the audio rotating around the listener in Hz.
	/// </summary>
	[JsonProperty("rotationHz")]
	public Optional<float> RotationHz { get; set; }

	/// <inheritdoc cref="LavalinkRotation"/>
	/// <param name="rotationHz">The frequency of the audio rotating around the listener in Hz.</param>
	public LavalinkRotation(Optional<float> rotationHz)
	{
		this.RotationHz = rotationHz;
	}
}

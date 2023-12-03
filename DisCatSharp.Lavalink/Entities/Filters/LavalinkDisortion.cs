using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Distortion effect. It can generate some pretty unique audio effects.
/// </summary>
public sealed class LavalinkDisortion
{
	/// <summary>
	/// The sin offset
	/// </summary>
	[JsonProperty("sinOffset")]
	public Optional<float> SinOffset { get; set; }

	/// <summary>
	/// The sin scale
	/// </summary>
	[JsonProperty("sinScale")]
	public Optional<float> SinScale { get; set; }

	/// <summary>
	/// The cos offset
	/// </summary>
	[JsonProperty("cosOffset")]
	public Optional<float> CosOffset { get; set; }

	/// <summary>
	/// The cos scale
	/// </summary>
	[JsonProperty("cosScale")]
	public Optional<float> CosScale { get; set; }

	/// <summary>
	/// The tan offset
	/// </summary>
	[JsonProperty("tanOffset")]
	public Optional<float> TanOffset { get; set; }

	/// <summary>
	/// The tan scale
	/// </summary>
	[JsonProperty("tanScale")]
	public Optional<float> TanScale { get; set; }

	/// <summary>
	/// The offset
	/// </summary>
	[JsonProperty("offset")]
	public Optional<float> Offset { get; set; }

	/// <summary>
	/// The scale
	/// </summary>
	[JsonProperty("scale")]
	public Optional<float> Scale { get; set; }

	/// <inheritdoc cref="LavalinkDisortion"/>
	/// <param name="sinOffset">The sin offset</param>
	/// <param name="sinScale">The sin scale</param>
	/// <param name="cosOffset">The cos offset</param>
	/// <param name="cosScale">The cos scale</param>
	/// <param name="tanOffset">The tan offset</param>
	/// <param name="tanScale">The tan scale</param>
	/// <param name="offset">The offset</param>
	/// <param name="scale">The scale</param>
	public LavalinkDisortion(Optional<float> sinOffset, Optional<float> sinScale, Optional<float> cosOffset, Optional<float> cosScale, Optional<float> tanOffset, Optional<float> tanScale, Optional<float> offset, Optional<float> scale)
	{
		this.SinOffset = sinOffset;
		this.SinScale = sinScale;
		this.CosOffset = cosOffset;
		this.CosScale = cosScale;
		this.TanOffset = tanOffset;
		this.TanScale = tanScale;
		this.Offset = offset;
		this.Scale = scale;
	}
}

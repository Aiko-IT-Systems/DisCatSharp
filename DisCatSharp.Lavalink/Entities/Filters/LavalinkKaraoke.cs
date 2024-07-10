using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Uses equalization to eliminate part of a band, usually targeting vocals.
/// </summary>
public sealed class LavalinkKaraoke
{
	/// <summary>
	/// Gets or sets the level (<c>0.0</c> to <c>1.0</c> where <c>0.0</c> is no effect and <c>1.0</c> is full effect).
	/// </summary>
	[JsonProperty("level")]
	public Optional<float> Level { get; set; }

	/// <summary>
	/// Gets or sets the mono level (<c>0.0</c> to <c>1.0</c> where <c>0.0</c> is no effect and <c>1.0</c> is full effect).
	/// </summary>
	[JsonProperty("monoLevel")]
	public Optional<float> MonoLevel { get; set; }

	/// <summary>
	/// Gets or sets the filter band in Hz.
	/// </summary>
	[JsonProperty("filterBand")]
	public Optional<float> FilterBand { get; set; }

	/// <summary>
	/// Gets or sets the filter width.
	/// </summary>
	[JsonProperty("filterWidth")]
	public Optional<float> FilterWidth { get; set; }

	/// <inheritdoc cref="LavalinkKaraoke"/>
	/// <param name="level">The level</param>
	/// <param name="monoLevel">The mono level</param>
	/// <param name="filterBand">The filter band in Hz.</param>
	/// <param name="filterWidth">The filter width.</param>
	public LavalinkKaraoke(Optional<float> level, Optional<float> monoLevel, Optional<float> filterBand, Optional<float> filterWidth)
	{
		this.Level = level;
		this.MonoLevel = monoLevel;
		this.FilterBand = filterBand;
		this.FilterWidth = filterWidth;
	}
}

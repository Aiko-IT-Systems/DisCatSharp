using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Represents various lavalink filters.
/// </summary>
public sealed class LavalinkFilters
{
	/// <summary>
	/// Gets or sets the player volume from <c>0.0</c> to <c>5.0</c>, where <c>1.0</c> is 100%. Values ><c>1.0</c> may cause clipping.
	/// </summary>
	[JsonProperty("volume")]
	public Optional<float> Volume { get; set; }

	/// <summary>
	/// Gets or sets the equalizer.
	/// </summary>
	[JsonProperty("equalizer")]
	public Optional<List<LavalinkEqualizer>> Equalizers { get; set; }

	/// <summary>
	/// Gets or sets the karaoke.
	/// </summary>
	[JsonProperty("karaoke")]
	public Optional<LavalinkKaraoke> Karaoke { get; set; }

	/// <summary>
	/// Gets or sets the timescale.
	/// </summary>
	[JsonProperty("timescale")]
	public Optional<LavalinkTimescale> Timescale { get; set; }

	/// <summary>
	/// Gets or sets the tremolo.
	/// </summary>
	[JsonProperty("tremolo")]
	public Optional<LavalinkTremolo> Tremolo { get; set; }

	/// <summary>
	/// Gets or sets the vibrato.
	/// </summary>
	[JsonProperty("vibrato")]
	public Optional<LavalinkVibrato> Vibrato { get; set; }

	/// <summary>
	/// Gets or sets the rotation.
	/// </summary>
	[JsonProperty("rotation")]
	public Optional<LavalinkRotation> Rotation { get; set; }

	/// <summary>
	/// Gets or sets the disortion.
	/// </summary>
	[JsonProperty("disortion")]
	public Optional<LavalinkDisortion> Disortion { get; set; }

	/// <summary>
	/// Gets or sets the channel-mix.
	/// </summary>
	[JsonProperty("channelMix")]
	public Optional<LavalinkChannelMix> ChannelMix { get; set; }

	/// <summary>
	/// Gets or sets the low-pass.
	/// </summary>
	[JsonProperty("lowPass")]
	public Optional<LavalinkLowPass> LowPass { get; set; }

	/// <summary>
	/// Gets a dictionary of custom plugin filters.
	/// <para><c>Key</c> is plugin name.</para>
	/// <para><c>Value</c> is plugin object.</para>
	/// </summary>
	[JsonProperty("pluginFilters")]
	public Optional<Dictionary<string, IPluginFilter>> PluginFilters { get; set; }
}

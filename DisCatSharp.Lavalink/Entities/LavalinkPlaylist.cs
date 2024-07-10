using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink playlist result.
/// </summary>
public sealed class LavalinkPlaylist
{
	/// <summary>
	/// Gets the lavalink playlist info.
	/// </summary>
	[JsonProperty("info")]
	public LavalinkPlaylistInfo Info { get; internal set; }

	/// <summary>
	/// Gets the lavalink plugin info.
	/// </summary>
	[JsonProperty("pluginInfo")]
	public LavalinkPluginInfo PluginInfo { get; internal set; }

	/// <summary>
	/// Gets the loaded tracks.
	/// </summary>
	[JsonProperty("tracks")]
	public List<LavalinkTrack> Tracks { get; internal set; }
}

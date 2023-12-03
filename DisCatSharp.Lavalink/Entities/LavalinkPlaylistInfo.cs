using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink playlist info.
/// </summary>
public sealed class LavalinkPlaylistInfo
{
	/// <summary>
	/// Gets the name of the playlist.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the selected track.
	/// <para><c>-1</c> if none is selected.</para>
	/// </summary>
	[JsonProperty("selectedTrack")]
	public int SelectedTrack { get; internal set; }
}

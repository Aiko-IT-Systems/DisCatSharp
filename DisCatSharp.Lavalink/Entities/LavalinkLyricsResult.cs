using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
///     Represents a lavalink lyrics result.
/// </summary>
public sealed class LavalinkLyricsResult
{
	/// <summary>
	///     Gets the name of the source where the lyrics were fetched from.
	/// </summary>
	[JsonProperty("sourceName", NullValueHandling = NullValueHandling.Ignore)]
	public string SourceName { get; internal set; }

	/// <summary>
	///     Gets the name of the provider the lyrics was fetched from on the source.
	/// </summary>
	[JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
	public string? Provider { get; internal set; }

	/// <summary>
	///     Gets the lyrics text.
	/// </summary>
	[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
	public string? Text { get; internal set; }

	/// <summary>
	///     Gets the lyrics lines.
	/// </summary>
	[JsonProperty("lines", NullValueHandling = NullValueHandling.Ignore)]
	public List<LyricsLine> Lines { get; internal set; } = [];

	/// <summary>
	///     Gets additional plugin specific data.
	/// </summary>
	[JsonProperty("plugin", NullValueHandling = NullValueHandling.Ignore)]
	public LavalinkPluginInfo Plugin { get; internal set; }
}

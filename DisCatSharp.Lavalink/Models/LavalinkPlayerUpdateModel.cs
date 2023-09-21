using DisCatSharp.Entities;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Entities.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Models;

/// <summary>
/// Represents an easy-to-use model to update a <see cref="LavalinkPlayer"/>.
/// </summary>
public sealed class LavalinkPlayerUpdateModel
{
	/// <summary>
	/// Sets whether to replace the current track.
	/// </summary>
	[JsonIgnore]
	public bool Replace { internal get; set; }

	/// <summary>
	/// Sets the encoded track.
	/// </summary>
	[JsonProperty("encodedTrack")]
	public Optional<string?> EncodedTrack { internal get; set; }

	/// <summary>
	/// Sets the identifier.
	/// </summary>
	[JsonProperty("identifier")]
	public Optional<string> Identifier { internal get; set; }

	/// <summary>
	/// Sets the start or seek position.
	/// </summary>
	[JsonProperty("position")]
	public Optional<int> Position { internal get; set; }

	/// <summary>
	/// Sets the end time.
	/// </summary>
	[JsonProperty("endTime")]
	public Optional<int?> EndTime { internal get; set; }

	/// <summary>
	/// Sets the volume.
	/// </summary>
	[JsonProperty("volume")]
	public Optional<int> Volume { internal get; set; }

	/// <summary>
	/// Sets whether the player is paused.
	/// </summary>
	[JsonProperty("paused")]
	public Optional<bool> Paused { internal get; set; }

	/// <summary>
	/// Gets or sets the filters.
	/// </summary>
	[JsonProperty("filters")]
	public Optional<LavalinkFilters> Filters { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkPlayerUpdateModel"/> class.
	/// </summary>
	internal LavalinkPlayerUpdateModel()
	{ }
}

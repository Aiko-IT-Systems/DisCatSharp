using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a sticker modify payload.
/// </summary>
internal sealed class RestStickerModifyPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Description { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Tags { get; set; }
}

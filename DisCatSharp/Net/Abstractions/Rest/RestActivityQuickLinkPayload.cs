using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents the payload for creating an activity quick link.
/// </summary>
internal sealed class RestActivityQuickLinkPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the caller-defined identifier for the quick link.
	/// </summary>
	[JsonProperty("custom_id")]
	public string CustomId { internal get; set; }

	/// <summary>
	///     Gets or sets the quick link description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { internal get; set; }

	/// <summary>
	///     Gets or sets the quick link title.
	/// </summary>
	[JsonProperty("title")]
	public string Title { internal get; set; }

	/// <summary>
	///     Gets or sets the base64-encoded image.
	/// </summary>
	[JsonProperty("image")]
	public string Image { internal get; set; }
}

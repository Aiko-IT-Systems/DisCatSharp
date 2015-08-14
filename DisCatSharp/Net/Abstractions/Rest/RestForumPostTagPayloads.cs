using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents the forum tag payload.
/// </summary>
internal sealed class RestForumPostTagPayloads : ObservableApiObject
{
	/// <summary>
	/// Sets the tags's new name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { internal get; set; }

	/// <summary>
	/// Sets the tags's new emoji.
	/// </summary>
	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Include)]
	public Optional<ulong?> Emoji { internal get; set; }

	/// <summary>
	/// Sets whether the tag should be mod only.
	/// </summary>
	[JsonProperty("moderated", NullValueHandling = NullValueHandling.Ignore)]
	public bool Moderated { internal get; set; }

	/// <summary>
	/// Gets the unicode emoji of the forum post tag.
	/// </summary>
	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> UnicodeEmojiString { internal get; set; }
}

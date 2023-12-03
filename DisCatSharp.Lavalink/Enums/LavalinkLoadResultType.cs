using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Represents Lavalink track loading results.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LavalinkLoadResultType
{
	/// <summary>
	/// Specifies that track was loaded successfully.
	/// </summary>
	[EnumMember(Value = "track")]
	Track,

	/// <summary>
	/// Specifies that playlist was loaded successfully.
	/// </summary>
	[EnumMember(Value = "playlist")]
	Playlist,

	/// <summary>
	/// Specifies that the result set contains search results.
	/// </summary>
	[EnumMember(Value = "search")]
	Search,

	/// <summary>
	/// Specifies that the search yielded no results.
	/// </summary>
	[EnumMember(Value = "empty")]
	Empty,

	/// <summary>
	/// Specifies that the track failed to load.
	/// </summary>
	[EnumMember(Value = "error")]
	Error
}

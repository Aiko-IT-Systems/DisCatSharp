using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the tag matching mode for a forum channel.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TagMatching
{
	/// <summary>
	/// At least one tag must match ("match_some").
	/// </summary>
	[EnumMember(Value = "match_some")]
	MatchSome,

	/// <summary>
	/// All tags must match ("match_all").
	/// </summary>
	[EnumMember(Value = "match_all")]
	MatchAll
}

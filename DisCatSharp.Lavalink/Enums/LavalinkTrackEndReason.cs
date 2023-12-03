using System.Runtime.Serialization;

using DisCatSharp.Lavalink.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Represents reasons why a <see cref="LavalinkTrack"/> ended.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LavalinkTrackEndReason
{
	/// <summary>
	/// The track was finished.
	/// </summary>
	[EnumMember(Value = "finished")]
	Finished,

	/// <summary>
	/// The track was failed to load.
	/// </summary>
	[EnumMember(Value = "loadFailed")]
	LoadFailed,

	/// <summary>
	/// The track was stopped.
	/// </summary>
	[EnumMember(Value = "stopped")]
	Stopped,

	/// <summary>
	/// The track was replaced.
	/// </summary>
	[EnumMember(Value = "replaced")]
	Replaced,

	/// <summary>
	/// The track was cleaned up.
	/// </summary>
	[EnumMember(Value = "cleanup")]
	Cleanup
}

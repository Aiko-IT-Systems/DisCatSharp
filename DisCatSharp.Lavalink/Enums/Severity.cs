using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Lavalink.Enums;

/// <summary>
/// Represents the severity for exceptions.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum Severity
{
	/// <summary>
	/// The cause is known and expected, indicates that there is nothing wrong with the library itself.
	/// </summary>
	[EnumMember(Value = "common")]
	Common,

	/// <summary>
	/// <para>The cause might not be exactly known, but is possibly caused by outside factors.</para>
	/// <para>For example when an outside service responds in a format that we do not expect.</para>
	/// </summary>
	[EnumMember(Value = "suspicious")]
	Suspicious,

	/// <summary>
	/// <para>The probable cause is an issue with the library or there is no way to tell what the cause might be.</para>
	/// <para>This is the default level and other levels are used in cases where the thrower has more in-depth knowledge about the error.</para>
	/// </summary>
	[EnumMember(Value = "fault")]
	Fault
}

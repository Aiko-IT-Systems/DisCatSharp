using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a join request status type.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum JoinRequestStatusType
{
	/// <summary>
	///     Indicates that the request status is unknown.
	/// </summary>
	[EnumMember(Value = "UNKNOWN")]
	Unknown,

	/// <summary>
	///     Indicates that the request was submitted.
	/// </summary>
	[EnumMember(Value = "SUBMITTED")]
	Submitted,

	/// <summary>
	///     Indicates that the request was approved.
	/// </summary>
	[EnumMember(Value = "APPROVED")]
	Approved,

	/// <summary>
	///     Indicates that the request was rejected.
	/// </summary>
	[EnumMember(Value = "REJECTED")]
	Rejected
}

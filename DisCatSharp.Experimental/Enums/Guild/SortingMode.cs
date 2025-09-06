using System.Runtime.Serialization;

namespace DisCatSharp.Experimental.Enums;

/// <summary>
/// Represents the sorting mode for message search.
/// </summary>
public enum SortingMode
{
	/// <summary>
	/// Sort by relevance.
	/// </summary>
	[EnumMember(Value = "relevance")]
	Relevance,

	/// <summary>
	/// Sort by timestamp.
	/// </summary>
	[EnumMember(Value = "timestamp")]
	Timestamp
}

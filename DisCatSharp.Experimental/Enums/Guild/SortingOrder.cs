using System.Runtime.Serialization;

namespace DisCatSharp.Experimental.Enums;

/// <summary>
/// Represents the sorting order for message search.
/// </summary>
public enum SortingOrder
{
    /// <summary>
    /// Sort in ascending order.
    /// </summary>
	[EnumMember(Value = "asc")]
    Asc,

    /// <summary>
    /// Sort in descending order.
    /// </summary>
    [EnumMember(Value = "desc")]
    Desc
}

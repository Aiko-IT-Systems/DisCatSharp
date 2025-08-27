using System.Runtime.Serialization;

namespace DisCatSharp.Experimental.Enums;

/// <summary>
/// Represents the type of search cursor.
/// </summary>
public enum SearchCursorType
{
    /// <summary>
    /// Represents a score-based cursor.
    /// </summary>
    [EnumMember(Value = "score")]
    Score,

    /// <summary>
    /// Represents a timestamp-based cursor.
    /// </summary>
    [EnumMember(Value = "timestamp")]
    Timestamp
}

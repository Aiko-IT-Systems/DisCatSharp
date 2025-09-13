using System.Runtime.Serialization;

namespace DisCatSharp.Experimental.Enums;

/// <summary>
/// Represents the type of embed that can be searched.
/// </summary>
public enum SearchableEmbedType
{
    /// <summary>
    /// Represents an image embed.
    /// </summary>
    [EnumMember(Value = "image")]
    Image,

    /// <summary>
    /// Represents a video embed.
    /// </summary>
    [EnumMember(Value = "video")]
    Video,

    /// <summary>
    /// Represents a gif embed.
    /// </summary>
    [EnumMember(Value = "gif")]
    Gif,

    /// <summary>
    /// Represents a sound embed.
    /// </summary>
    [EnumMember(Value = "sound")]
    Sound,

    /// <summary>
    /// Represents an article embed.
    /// </summary>
    [EnumMember(Value = "article")]
    Article
}

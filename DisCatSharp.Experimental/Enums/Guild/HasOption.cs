using System.Runtime.Serialization;

namespace DisCatSharp.Experimental.Enums;

/// <summary>
/// Represents the options for the "has" filter in message search.
/// </summary>
public enum HasOption
{
    /// <summary>
    /// Represents messages with links.
    /// </summary>
    [EnumMember(Value = "link")]
    Link,

    /// <summary>
    /// Represents messages with embeds.
    /// </summary>
    [EnumMember(Value = "embed")]
    Embed,

    /// <summary>
    /// Represents messages with files.
    /// </summary>
    [EnumMember(Value = "file")]
    File,

    /// <summary>
    /// Represents messages with images.
    /// </summary>
    [EnumMember(Value = "image")]
    Image,

    /// <summary>
    /// Represents messages with videos.
    /// </summary>
    [EnumMember(Value = "video")]
    Video,

    /// <summary>
    /// Represents messages with sounds.
    /// </summary>
    [EnumMember(Value = "sound")]
    Sound,

    /// <summary>
    /// Represents messages with stickers.
    /// </summary>
    [EnumMember(Value = "sticker")]
    Sticker,

    /// <summary>
    /// Represents messages with polls.
    /// </summary>
    [EnumMember(Value = "poll")]
    Poll,

    /// <summary>
    /// Represents messages with snapshots.
    /// </summary>
    [EnumMember(Value = "snapshot")]
    Snapshot,

    /// <summary>
    /// Represents messages without links.
    /// </summary>
    [EnumMember(Value = "-link")]
    NoLink,

    /// <summary>
    /// Represents messages without embeds.
    /// </summary>
    [EnumMember(Value = "-embed")]
    NoEmbed,

    /// <summary>
    /// Represents messages without files.
    /// </summary>
    [EnumMember(Value = "-file")]
    NoFile,

    /// <summary>
    /// Represents messages without images.
    /// </summary>
    [EnumMember(Value = "-image")]
    NoImage,

    /// <summary>
    /// Represents messages without videos.
    /// </summary>
    [EnumMember(Value = "-video")]
    NoVideo,

    /// <summary>
    /// Represents messages without sounds.
    /// </summary>
    [EnumMember(Value = "-sound")]
    NoSound,

    /// <summary>
    /// Represents messages without stickers.
    /// </summary>
    [EnumMember(Value = "-sticker")]
    NoSticker,

    /// <summary>
    /// Represents messages without polls.
    /// </summary>
    [EnumMember(Value = "-poll")]
    NoPoll,

    /// <summary>
    /// Represents messages without snapshots.
    /// </summary>
    [EnumMember(Value = "-snapshot")]
    NoSnapshot
}

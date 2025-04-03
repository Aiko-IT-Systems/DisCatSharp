using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///    Represents a Discord guild trait.
/// </summary>
public sealed class DiscordGuildTrait
{
    /// <summary>
    ///    Gets whether the emoji is animated.
    /// </summary>
    [JsonProperty("emoji_animated", NullValueHandling = NullValueHandling.Ignore)]
    public bool EmojiAnimated { get; internal set; }

    /// <summary>
    ///    Gets the emoji ID.
    /// </summary>
    [JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Include)]
    public ulong? EmojiId { get; internal set; }

    /// <summary>
    ///    Gets the emoji name.
    /// </summary>
    [JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Include)]
    public string? EmojiName { get; internal set; }

    /// <summary>
    ///    Gets the label.
    /// </summary>
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string Label { get; internal set; }

    /// <summary>
    ///    Gets the position.
    /// </summary>
    [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
    public int Position { get; internal set; }
}

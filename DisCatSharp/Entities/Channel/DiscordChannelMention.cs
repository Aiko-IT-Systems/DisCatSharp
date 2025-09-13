using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a channel mention object in a Discord message.
/// </summary>
public class DiscordChannelMention
{
    /// <summary>
    /// Gets the ID of the channel.
    /// </summary>
    [JsonProperty("id")]
    public ulong Id { get; internal set; }

    /// <summary>
    /// Gets the ID of the guild containing the channel.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Gets the type of the channel.
    /// </summary>
    [JsonProperty("type")]
    public ChannelType Type { get; internal set; }

    /// <summary>
    /// Gets the name of the channel.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }
}

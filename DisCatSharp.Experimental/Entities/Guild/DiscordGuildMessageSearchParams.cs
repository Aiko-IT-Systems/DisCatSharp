using System.Collections.Generic;

using DisCatSharp.Experimental.Enums;
using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents the parameters for searching messages in a guild.
/// </summary>
public sealed class DiscordGuildMessageSearchParams
{
    /// <summary>
    /// Gets or sets the sorting mode.
    /// </summary>
    [JsonProperty("sort_by", NullValueHandling = NullValueHandling.Ignore)]
    public SortingMode? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sorting order.
    /// </summary>
    [JsonProperty("sort_order", NullValueHandling = NullValueHandling.Ignore)]
    public SortingOrder? SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the content to search for.
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the slop value.
    /// Slop comes from elasticsearch and is used to allow for flexibility in matching.
    /// </summary>
    [JsonProperty("slop", NullValueHandling = NullValueHandling.Ignore)]
    public int? Slop { get; set; }

    /// <summary>
    /// Gets or sets the list of contents to search for.
    /// </summary>
    [JsonProperty("contents", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Contents { get; set; }

    /// <summary>
    /// Gets or sets the list of author IDs to filter by.
    /// </summary>
    [JsonProperty("author_id", NullValueHandling = NullValueHandling.Ignore)]
    public List<ulong>? AuthorIds { get; set; }

    /// <summary>
    /// Gets or sets the list of author types to filter by.
    /// </summary>
    [JsonProperty("author_type", NullValueHandling = NullValueHandling.Ignore)]
    public List<AuthorType>? AuthorTypes { get; set; }

    /// <summary>
    /// Gets or sets the list of mentions to filter by.
    /// </summary>
    [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
    public List<ulong>? Mentions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to filter by @everyone mentions.
    /// </summary>
    [JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
    public bool? MentionEveryone { get; set; }

    /// <summary>
    /// Gets or sets the minimum message ID to filter by.
    /// </summary>
    [JsonProperty("min_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? MinId { get; set; }

    /// <summary>
    /// Gets or sets the maximum message ID to filter by.
    /// </summary>
    [JsonProperty("max_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? MaxId { get; set; }

    /// <summary>
    /// Gets or sets the limit of results to return.
    /// </summary>
    [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the offset for pagination.
    /// </summary>
    [JsonProperty("offset", NullValueHandling = NullValueHandling.Ignore)]
    public int? Offset { get; set; }

    /// <summary>
    /// Gets or sets the cursor for pagination.
    /// </summary>
    [JsonProperty("cursor", NullValueHandling = NullValueHandling.Ignore)]
    public string? Cursor { get; set; }

    /// <summary>
    /// Gets or sets the list of "has" options to filter by.
    /// </summary>
    [JsonProperty("has", NullValueHandling = NullValueHandling.Ignore)]
    public List<HasOption>? Has { get; set; }

    /// <summary>
    /// Gets or sets the list of link hostnames to filter by.
    /// </summary>
    [JsonProperty("link_hostname", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? LinkHostnames { get; set; }

    /// <summary>
    /// Gets or sets the list of embed providers to filter by.
    /// </summary>
    [JsonProperty("embed_provider", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? EmbedProviders { get; set; }

    /// <summary>
    /// Gets or sets the list of embed types to filter by.
    /// </summary>
    [JsonProperty("embed_type", NullValueHandling = NullValueHandling.Ignore)]
    public List<SearchableEmbedType>? EmbedTypes { get; set; }

    /// <summary>
    /// Gets or sets the list of attachment extensions to filter by.
    /// </summary>
    [JsonProperty("attachment_extension", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? AttachmentExtensions { get; set; }

    /// <summary>
    /// Gets or sets the attachment filename to filter by.
    /// </summary>
    [JsonProperty("attachment_filename", NullValueHandling = NullValueHandling.Ignore)]
    public string? AttachmentFilename { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to filter by pinned messages.
    /// </summary>
    [JsonProperty("pinned", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Pinned { get; set; }

    /// <summary>
    /// Gets or sets the command ID to filter by.
    /// </summary>
    [JsonProperty("command_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? CommandId { get; set; }

    /// <summary>
    /// Gets or sets the command name to filter by.
    /// </summary>
    [JsonProperty("command_name", NullValueHandling = NullValueHandling.Ignore)]
    public string? CommandName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include NSFW content.
    /// </summary>
    [JsonProperty("include_nsfw", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IncludeNsfw { get; set; }

    /// <summary>
    /// Gets or sets the list of channel IDs to filter by.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public List<ulong>? ChannelIds { get; set; }
}

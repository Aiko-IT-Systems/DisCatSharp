using System.Collections.Generic;
using DisCatSharp.Entities;
using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents the response for a guild message search.
/// </summary>
public sealed class DiscordSearchGuildMessagesResponse : ObservableApiObject
{
    /// <summary>
    /// Gets the analytics ID for the search.
    /// </summary>
    [JsonProperty("analytics_id")]
    public string? AnalyticsId { get; internal set; }

	/// <summary>
	/// Gets the list of messages found in the search.
	/// </summary>
	[JsonProperty("messages")]
	public List<DiscordMessage> Messages { get; internal set; } = [];

    /// <summary>
    /// Gets a value indicating whether a deep historical index is being performed.
    /// </summary>
    [JsonProperty("doing_deep_historical_index")]
    public bool DoingDeepHistoricalIndex { get; internal set; }

    /// <summary>
    /// Gets the total number of results found.
    /// </summary>
    [JsonProperty("total_results")]
    public int TotalResults { get; internal set; }

    /// <summary>
    /// Gets the list of threads found in the search.
    /// </summary>
    [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
    public List<DiscordThreadChannel> Threads { get; internal set; } = [];

    /// <summary>
    /// Gets the list of members found in the search.
    /// </summary>
    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    public List<DiscordMember> Members { get; internal set; } = [];

    /// <summary>
    /// Gets the number of documents indexed.
    /// </summary>
    [JsonProperty("documents_indexed", NullValueHandling = NullValueHandling.Ignore)]
    public int? DocumentsIndexed { get; internal set; }
}

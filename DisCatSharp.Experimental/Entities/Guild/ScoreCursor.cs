using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a score-based cursor for pagination.
/// </summary>
public sealed class ScoreCursor
{
    /// <summary>
    /// Gets or sets the score of the cursor.
    /// </summary>
    [JsonProperty("score")]
    public double Score { get; set; }
}

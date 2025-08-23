using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a timestamp-based cursor for pagination.
/// </summary>
public sealed class TimestampCursor
{
    /// <summary>
    /// Gets or sets the timestamp of the cursor.
    /// </summary>
    [JsonProperty("timestamp")]
    public ulong Timestamp { get; set; }
}

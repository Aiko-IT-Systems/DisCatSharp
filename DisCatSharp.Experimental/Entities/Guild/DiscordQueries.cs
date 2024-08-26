using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Represents a search query.
/// </summary>
public sealed class DiscordQuery
{
	/// <summary>
	///     Builds a OR query.
	///     Array of ulongs, strings, or integers.
	/// </summary>
	[JsonProperty("or_query", NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? OrQuery { get; set; }

	/// <summary>
	///     Builds a AND query
	///     Array of ulongs, strings, or integers.
	/// </summary>
	[JsonProperty("and_query", NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? AndQuery { get; set; }

	/// <summary>
	///     Builds a RANGE query.
	/// </summary>
	[JsonProperty("range", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordRangeQuery? RangeQuery { get; set; }
}

/// <summary>
///     Represents a range query.
/// </summary>
public sealed class DiscordRangeQuery
{
	/// <summary>
	///     Inclusive lower bound value to match.
	///     Ulong or integer.
	/// </summary>
	[JsonProperty("gte", NullValueHandling = NullValueHandling.Ignore)]
	public long? Gte { get; set; }

	/// <summary>
	///     Inclusive upper bound value to match.
	///     Ulong or integer.
	/// </summary>
	[JsonProperty("lte", NullValueHandling = NullValueHandling.Ignore)]
	public long? Lte { get; set; }
}

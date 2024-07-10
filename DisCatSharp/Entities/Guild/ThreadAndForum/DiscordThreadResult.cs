using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord thread result.
/// </summary>
public class DiscordThreadResult : ObservableApiObject
{
	/// <summary>
	/// Gets the returned threads.
	/// </summary>
	[JsonIgnore]
	public Dictionary<ulong, DiscordThreadChannel> ReturnedThreads
		=> this.Threads == null || this.Threads.Count == 0
			? []
			: this.Threads.Select(t => new
			{
				t.Id,
				t
			}).ToDictionary(t => t.Id, t => t.t);

	[JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordThreadChannel> Threads { get; set; }

	/// <summary>
	/// Gets the active members.
	/// </summary>
	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordThreadChannelMember> ActiveMembers { get; internal set; }

	/// <summary>
	/// Whether there are more results.
	/// </summary>
	[JsonProperty("has_more")]
	public bool HasMore { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordThreadResult"/> class.
	/// </summary>
	internal DiscordThreadResult()
		: base()
	{ }
}

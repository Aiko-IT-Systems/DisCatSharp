using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordPoll
{
	[JsonProperty("question")]
	public TransportDiscordPollMediaObject Question { get; internal set; }

	[JsonProperty("answers")]
	public List<TransportDiscordPollAnswerObject> Answers { get; internal set; } = new();

	[JsonProperty("expiry", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? Expiry { get; internal set; }

	[JsonProperty("allow_multiselect")]
	public bool AllowMultiselect { get; internal set; }

	[JsonProperty("layout_type", NullValueHandling = NullValueHandling.Ignore)]
	public int LayoutType { get; internal set; }

	[JsonProperty("results")]
	public TransportDiscordPollResultsObject? Results { get; internal set; }
}

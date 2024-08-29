using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordMessageCall
{
	[JsonProperty("participants", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> Participants { get; internal set; } = [];

	[JsonProperty("ended_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? EndedTimestamp { get; internal set; }
}

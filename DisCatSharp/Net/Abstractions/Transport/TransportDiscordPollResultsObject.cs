using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordPollResultsObject
{
	[JsonProperty("answers")]
	public List<TransportDiscordPollResultAnswerObject> Answers { get; internal set; } = [];

	[JsonProperty("is_finalized")]
	public bool IsFinalized { get; internal set; }
}

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordPollResultAnswerObject
{
	[JsonProperty("answer_id")]
	public int AnswerId { get; internal set; }

	[JsonProperty("votes")]
	public int Votes { get; internal set; }

	[JsonProperty("percentage")]
	public double Percentage { get; internal set; }
}

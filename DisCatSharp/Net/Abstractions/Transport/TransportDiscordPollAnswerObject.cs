using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordPollAnswerObject
{
	[JsonProperty("answer_id")]
	public int AnswerId { get; internal set; }

	[JsonProperty("text")]
	public string Text { get; internal set; }

	[JsonProperty("emoji")]
	public TransportDiscordEmoji? Emoji { get; internal set; }
}

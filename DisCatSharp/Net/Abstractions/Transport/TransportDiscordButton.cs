using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordButton : TransportDiscordMessageComponent
{
	[JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
	public ButtonStyle Style { get; internal set; }

	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmoji? Emoji { get; internal set; }

	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string? CustomId { get; internal set; }

	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string? Url { get; internal set; }

	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; internal set; }
}

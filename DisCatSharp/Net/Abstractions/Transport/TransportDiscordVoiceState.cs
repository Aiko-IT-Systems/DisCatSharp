using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordVoiceState
{
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong UserId { get; internal set; }

	[JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGuildMember? Member { get; internal set; }

	[JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
	public string SessionId { get; internal set; }

	[JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
	public bool Deaf { get; internal set; }

	[JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
	public bool Mute { get; internal set; }

	[JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
	public bool SelfDeaf { get; internal set; }

	[JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
	public bool SelfMute { get; internal set; }

	[JsonProperty("self_stream", NullValueHandling = NullValueHandling.Ignore)]
	public bool? SelfStream { get; internal set; }

	[JsonProperty("self_video", NullValueHandling = NullValueHandling.Ignore)]
	public bool SelfVideo { get; internal set; }

	[JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
	public bool Suppress { get; internal set; }

	[JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public string RequestToSpeakTimestamp { get; internal set; }
}

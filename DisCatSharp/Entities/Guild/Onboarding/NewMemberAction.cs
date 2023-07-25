using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

public sealed class NewMemberAction : ObservableApiObject
{
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	[JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
	public NewMemberActionType ActionType { get; internal set; }

	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmoji Emoji { get; internal set; }
}
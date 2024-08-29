using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordResolvedData
{
	[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<ulong, TransportDiscordUser> Users { get; internal set; }

	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<ulong, TransportDiscordGuildMember> Members { get; internal set; }

	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<ulong, TransportDiscordRole> Roles { get; internal set; }

	[JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<ulong, TransportDiscordChannel> Channels { get; internal set; }

	[JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<ulong, TransportDiscordMessage> Messages { get; internal set; }

	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<ulong, TransportDiscordAttachment> Attachments { get; internal set; }
}

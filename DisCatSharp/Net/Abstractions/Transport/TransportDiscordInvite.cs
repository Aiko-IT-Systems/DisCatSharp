using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordInvite
{
	[JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
	public string Code { get; internal set; }

	[JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGuild? Guild { get; internal set; }

	[JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordChannel? Channel { get; internal set; }

	[JsonProperty("inviter", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? Inviter { get; internal set; }

	[JsonProperty("target_user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? TargetUser { get; internal set; }

	[JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
	public TargetType TargetType { get; internal set; }

	[JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximatePresenceCount { get; internal set; }

	[JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximateMemberCount { get; internal set; }

	[JsonProperty("expires_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? ExpiresAt { get; internal set; }
}

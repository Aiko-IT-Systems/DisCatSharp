using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordThreadMember : NullableSnowflakeObject
{
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; internal set; }

	[JsonProperty("join_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset JoinTimestamp { get; internal set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public ThreadMemberFlags Flags { get; internal set; }
}

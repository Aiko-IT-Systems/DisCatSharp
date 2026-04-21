using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents a payload for requesting channel information for a specific guild.
/// </summary>
internal sealed class RequestChannelInfoPayload : ObservableApiObject
{
	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }

	[JsonProperty("include_status")]
	public bool IncludeStatus { get; set; }

	[JsonProperty("include_voice_start_time")]
	public bool IncludeVoiceStartTime { get; set; }
}

/// <summary>
///     Represents the payload for the channel information event response.
/// </summary>
internal sealed class ChannelInfoEventPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the guild ID associated with the channel information.
	/// </summary>
	[JsonProperty("guild_id")]
	internal ulong GuildId { get; set; }

	/// <summary>
	///     Gets or sets the list of channel information in the guild.
	/// </summary>
	[JsonProperty("channels")]
	internal List<DiscordChannelInfo> Channels { get; set; } = [];
}

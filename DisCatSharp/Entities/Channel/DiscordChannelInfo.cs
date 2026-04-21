using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///    Represents information about a Discord channel, including its ID, status, and voice start time.
/// </summary>
public sealed class DiscordChannelInfo : ObservableApiObject
{
	/// <summary>
	///    Gets the id of the channel.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; internal set; }

	/// <summary>
	///   Gets the status of the channel, if applicable.
	/// </summary>
	[JsonProperty("status")]
	public string? Status { get; internal set; }

	/// <summary>
	///  Gets the voice start time of the channel, if applicable.
	/// </summary>
	[JsonProperty("voice_start_time")]
	public DateTimeOffset? VoiceStartTime { get; internal set; }
}

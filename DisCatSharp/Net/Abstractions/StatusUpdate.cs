using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents data for websocket status update payload.
/// </summary>
internal sealed class StatusUpdate : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the unix millisecond timestamp of when the user went idle.
	/// </summary>
	[JsonProperty("since", NullValueHandling = NullValueHandling.Include)]
	public long? IdleSince { get; set; }

	/// <summary>
	/// Gets or sets whether the user is AFK.
	/// </summary>
	[JsonProperty("afk")]
	public bool IsAfk { get; set; }

	/// <summary>
	/// Gets or sets the status of the user.
	/// </summary>
	[JsonIgnore]
	public UserStatus Status { get; set; } = UserStatus.Online;

	/// <summary>
	/// Gets the status string of the user.
	/// </summary>
	[JsonProperty("status")]
	internal string StatusString =>
		this.Status switch
		{
			UserStatus.Online => "online",
			UserStatus.Idle => "idle",
			UserStatus.DoNotDisturb => "dnd",
			UserStatus.Invisible or UserStatus.Offline => "invisible",
			UserStatus.Streaming => "streaming",
			_ => "online"
		};

	/// <summary>
	/// Gets or sets the game the user is playing.
	/// </summary>
	[JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
	public TransportActivity? Activity { get; set; }

	[JsonIgnore]
	internal DiscordActivity? ActivityInternal;
}

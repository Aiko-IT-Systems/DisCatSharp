using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents data for websocket ready event payload.
/// </summary>
internal sealed class ReadyPayload : ObservableApiObject
{
	/// <summary>
	/// Gets the gateway version the client is connected to.
	/// </summary>
	[JsonProperty("v")]
	public int GatewayVersion { get; private set; }

	/// <summary>
	/// Gets the current user.
	/// </summary>
	[JsonProperty("user")]
	public TransportUser CurrentUser { get; private set; }

	/// <summary>
	/// <para>Gets the current shard.</para>
	/// <para>The first <see langword="int"/> will be the shard id.</para>
	/// <para>The last <see langword="int"/> will be the total shard count.</para>
	/// </summary>
	[JsonProperty("shard")]
	public List<int> Shard { get; private set; } = [];

	/// <summary>
	/// Gets the session type. Should be always "normal".
	/// </summary>
	[JsonProperty("session_type")]
	public string SessionType { get; private set; }

	/// <summary>
	/// Gets the current session's ID.
	/// </summary>
	[JsonProperty("session_id")]
	public string SessionId { get; private set; }

	/// <summary>
	/// The gateway url for resuming connections
	/// </summary>
	[JsonProperty("resume_gateway_url")]
	public string ResumeGatewayUrl { get; internal set; }

	/// <summary>
	/// Gets the guilds available for this shard.
	/// <para>These will be unavailable at the start.</para>
	/// </summary>
	[JsonProperty("guilds")]
	public List<DiscordGuild> Guilds { get; private set; } = [];

	/// <summary>
	/// Gets the closest geo ordered rtc regions recommended for the current user.
	/// </summary>
	[JsonProperty("geo_ordered_rtc_regions")]
	public List<string> GeoOrderedRtcRegions { get; private set; } = [];

	/// <summary>
	/// Gets the current (detected) location(s).
	/// </summary>
	[JsonProperty("current_location")]
	public List<string> CurrentLocation { get; private set; } = [];

	/// <summary>
	/// Gets the application id and flags.
	/// </summary>
	[JsonProperty("application")]
	public TransportApplication Application { get; private set; }

	/// <summary>
	/// Gets debug data sent by Discord. This contains a list of servers to which the client is connected.
	/// </summary>
	[JsonProperty("_trace")]
	public IReadOnlyList<string> Trace { get; private set; } = [];

	/// <summary>
	/// Creates a new instance of <see cref="ReadyPayload"/>.
	/// </summary>
	internal ReadyPayload()
		: base(["user_settings", "relationships", "private_channels", "presences", "guild_join_requests", "auth"])
	{ }
}

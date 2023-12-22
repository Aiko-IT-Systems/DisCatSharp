using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents data for websocket ready event payload.
/// </summary>
internal class ReadyPayload : ObservableApiObject
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
	/// Gets the guilds available for this shard.
	/// </summary>
	[JsonProperty("guilds")]
	public List<DiscordGuild> Guilds { get; private set; } = [];

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
	/// Gets debug data sent by Discord. This contains a list of servers to which the client is connected.
	/// </summary>
	[JsonProperty("_trace")]
	public IReadOnlyList<string> Trace { get; private set; }
}

using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice ready payload.
/// </summary>
internal sealed class VoiceReadyPayload
{
	/// <summary>
	/// Gets or sets the s s r c.
	/// </summary>
	[JsonProperty("ssrc")]
	public uint Ssrc { get; set; }

	/// <summary>
	/// Gets or sets the address.
	/// </summary>
	[JsonProperty("ip")]
	public string Address { get; set; }

	/// <summary>
	/// Gets or sets the port.
	/// </summary>
	[JsonProperty("port")]
	public ushort Port { get; set; }

	/// <summary>
	/// Gets or sets the modes.
	/// </summary>
	[JsonProperty("modes")]
	public IReadOnlyList<string> Modes { get; set; }

	/// <summary>
	/// Gets or sets the heartbeat interval.
	/// </summary>
	[JsonProperty("heartbeat_interval")]
	public int HeartbeatInterval { get; set; }
}

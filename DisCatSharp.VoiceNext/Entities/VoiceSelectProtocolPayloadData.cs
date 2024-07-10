using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice select protocol payload data.
/// </summary>
internal class VoiceSelectProtocolPayloadData
{
	/// <summary>
	/// Gets or sets the address.
	/// </summary>
	[JsonProperty("address")]
	public string Address { get; set; }

	/// <summary>
	/// Gets or sets the port.
	/// </summary>
	[JsonProperty("port")]
	public ushort Port { get; set; }

	/// <summary>
	/// Gets or sets the mode.
	/// </summary>
	[JsonProperty("mode")]
	public string Mode { get; set; }
}

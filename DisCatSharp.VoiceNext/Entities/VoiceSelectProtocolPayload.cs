using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice select protocol payload.
/// </summary>
internal sealed class VoiceSelectProtocolPayload
{
	/// <summary>
	/// Gets or sets the protocol.
	/// </summary>
	[JsonProperty("protocol")]
	public string Protocol { get; set; }

	/// <summary>
	/// Gets or sets the data.
	/// </summary>
	[JsonProperty("data")]
	public VoiceSelectProtocolPayloadData Data { get; set; }
}

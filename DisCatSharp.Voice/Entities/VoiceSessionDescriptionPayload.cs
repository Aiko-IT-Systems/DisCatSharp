using Newtonsoft.Json;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     The voice session description payload.
/// </summary>
internal sealed class VoiceSessionDescriptionPayload
{
	/// <summary>
	///     Gets or sets the secret key.
	/// </summary>
	[JsonProperty("secret_key")]
	public byte[] SecretKey { get; set; }

	/// <summary>
	///     Gets or sets the mode.
	/// </summary>
	[JsonProperty("mode")]
	public string Mode { get; set; }

	/// <summary>
	///     Gets or sets the DAVE protocol version negotiated for this session.
	///     A value of 0 means DAVE E2EE is not active.
	/// </summary>
	[JsonProperty("dave_protocol_version")]
	public int DaveProtocolVersion { get; set; }
}

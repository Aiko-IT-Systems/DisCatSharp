using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice session description payload.
/// </summary>
internal sealed class VoiceSessionDescriptionPayload
{
	/// <summary>
	/// Gets or sets the secret key.
	/// </summary>
	[JsonProperty("secret_key")]
	public byte[] SecretKey { get; set; }

	/// <summary>
	/// Gets or sets the mode.
	/// </summary>
	[JsonProperty("mode")]
	public string Mode { get; set; }
}

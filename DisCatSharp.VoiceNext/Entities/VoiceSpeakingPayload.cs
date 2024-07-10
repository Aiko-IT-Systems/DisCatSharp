using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice speaking payload.
/// </summary>
internal sealed class VoiceSpeakingPayload
{
	/// <summary>
	/// Gets or sets a value indicating whether speaking.
	/// </summary>
	[JsonProperty("speaking")]
	public SpeakingFlags Speaking { get; set; }

	/// <summary>
	/// Gets or sets the delay.
	/// </summary>
	[JsonProperty("delay", NullValueHandling = NullValueHandling.Ignore)]
	public int? Delay { get; set; }

	/// <summary>
	/// Gets or sets the s s r c.
	/// </summary>
	[JsonProperty("ssrc", NullValueHandling = NullValueHandling.Ignore)]
	public uint? Ssrc { get; set; }

	/// <summary>
	/// Gets or sets the user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; set; }
}

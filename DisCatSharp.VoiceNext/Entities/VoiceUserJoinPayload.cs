using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice user join payload.
/// </summary>
internal sealed class VoiceUserJoinPayload
{
	/// <summary>
	/// Gets the user id.
	/// </summary>
	[JsonProperty("user_id")]
	public ulong UserId { get; private set; }

	/// <summary>
	/// Gets the s s r c.
	/// </summary>
	[JsonProperty("audio_ssrc")]
	public uint Ssrc { get; private set; }
}

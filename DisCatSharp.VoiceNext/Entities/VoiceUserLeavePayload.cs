using Newtonsoft.Json;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The voice user leave payload.
/// </summary>
internal sealed class VoiceUserLeavePayload
{
	/// <summary>
	/// Gets or sets the user id.
	/// </summary>
	[JsonProperty("user_id")]
	public ulong UserId { get; set; }
}

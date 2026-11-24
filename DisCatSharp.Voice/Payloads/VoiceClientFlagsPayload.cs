using DisCatSharp.Voice.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Voice gateway OP 18 payload containing client-reported Discord Clips preferences.
/// </summary>
internal sealed class VoiceClientFlagsPayload
{
	/// <summary>
	///     Gets the Discord user identifier whose client reported the flags.
	/// </summary>
	[JsonProperty("user_id")]
	public ulong UserId { get; private set; }

	/// <summary>
	///     Gets the raw client-reported flags.
	/// </summary>
	[JsonProperty("flags")]
	public VoiceClientFlags Flags { get; private set; }
}

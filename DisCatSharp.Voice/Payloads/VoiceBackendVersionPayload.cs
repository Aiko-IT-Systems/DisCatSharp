using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Voice gateway OP 16 response containing informational backend versions.
/// </summary>
internal sealed class VoiceBackendVersionPayload
{
	/// <summary>
	///     Gets the voice backend version.
	/// </summary>
	[JsonProperty("voice")]
	public string Voice { get; private set; } = string.Empty;

	/// <summary>
	///     Gets the RTC worker backend version.
	/// </summary>
	[JsonProperty("rtc_worker")]
	public string RtcWorker { get; private set; } = string.Empty;
}

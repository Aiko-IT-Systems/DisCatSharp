using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Payload for voice gateway OP 11 <c>clients_connect</c>.
///     Provides the set of recognized user IDs that are part of the current DAVE session.
///     Used to validate MLS Add proposals — only recognized users may be added.
/// </summary>
internal sealed class VoiceClientsConnectPayload
{
	/// <summary>
	///     Gets or sets the user IDs of participants recognized by the voice gateway for this DAVE session.
	/// </summary>
	[JsonProperty("user_ids")]
	public ulong[] UserIds { get; set; } = [];
}

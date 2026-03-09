using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Payload for voice gateway OP 24 <c>dave_mls_prepare_epoch</c>.
///     Signals that a new MLS epoch is being prepared, triggering key rotation.
/// </summary>
internal sealed class DavePrepareEpochPayload
{
	/// <summary>
	///     Gets or sets the transition identifier for this epoch change.
	/// </summary>
	[JsonProperty("transition_id")]
	public ushort TransitionId { get; set; }

	/// <summary>
	///     Gets or sets the new MLS epoch number.
	/// </summary>
	[JsonProperty("epoch")]
	public ulong Epoch { get; set; }

	/// <summary>
	///     Gets or sets the DAVE protocol version active for this epoch.
	/// </summary>
	[JsonProperty("protocol_version")]
	public ushort ProtocolVersion { get; set; }
}

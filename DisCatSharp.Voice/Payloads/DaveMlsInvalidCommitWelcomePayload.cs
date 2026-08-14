using Newtonsoft.Json;

namespace DisCatSharp.Voice.Payloads;

/// <summary>
///     Payload for voice gateway OP 31 <c>dave_mls_invalid_commit_welcome</c>.
///     Notifies the server that an MLS commit or Welcome could not be processed.
/// </summary>
internal sealed class DaveMlsInvalidCommitWelcomePayload
{
	/// <summary>
	///     Gets or sets the transition ID whose commit or Welcome could not be processed.
	/// </summary>
	[JsonProperty("transition_id", Required = Required.Always)]
	public required ushort TransitionId { get; set; }

	/// <summary>
	///     Gets or sets an optional diagnostic description sent to the server.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }
}

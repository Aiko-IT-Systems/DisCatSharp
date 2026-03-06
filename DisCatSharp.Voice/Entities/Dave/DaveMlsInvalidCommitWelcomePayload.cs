using Newtonsoft.Json;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Payload for voice gateway OP 31 <c>dave_mls_invalid_commit_welcome</c>.
///     Notifies the client that its MLS commit or welcome message was rejected by the server.
///     The client should reset its MLS state and retry group establishment.
/// </summary>
internal sealed class DaveMlsInvalidCommitWelcomePayload
{
	/// <summary>
	///     Gets or sets an optional error description from the server.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }
}

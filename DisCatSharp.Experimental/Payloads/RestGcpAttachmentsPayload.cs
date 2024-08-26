using System.Collections.Generic;

using DisCatSharp.Experimental.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Payloads;

/// <summary>
///     Represents a gcp attachment payload.
/// </summary>
internal sealed class RestGcpAttachmentsPayload
{
	/// <summary>
	///     Sets the files.
	/// </summary>
	[JsonProperty("files")]
	public List<GcpAttachment> GcpAttachments { get; set; } = [];
}

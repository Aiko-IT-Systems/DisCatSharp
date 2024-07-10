using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions.Rest;

internal sealed class RestGcpAttachmentsPayload
{
	[JsonProperty("files")]
	public List<GcpAttachment> GcpAttachments { get; set; } = [];
}

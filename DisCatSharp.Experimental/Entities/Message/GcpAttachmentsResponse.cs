using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a gcp attachments response.
/// </summary>
public sealed class GcpAttachmentsResponse : ObservableApiObject
{
	/// <summary>
	/// Gets the attachments.
	/// </summary>
	[JsonProperty("attachments")]
	public List<GcpAttachmentUploadInformation> Attachments { get; internal set; } = [];
}

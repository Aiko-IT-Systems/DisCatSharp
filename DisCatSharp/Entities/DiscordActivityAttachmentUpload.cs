using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the response from creating an activity attachment.
/// </summary>
public sealed class DiscordActivityAttachmentUpload : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordActivityAttachmentUpload" /> class.
	/// </summary>
	internal DiscordActivityAttachmentUpload()
	{ }

	/// <summary>
	///     Gets the uploaded attachment metadata.
	/// </summary>
	[JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordAttachment? Attachment { get; internal set; }
}

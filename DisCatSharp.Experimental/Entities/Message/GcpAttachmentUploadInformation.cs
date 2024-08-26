using System;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Represents a gcp attachment upload information.
/// </summary>
public sealed class GcpAttachmentUploadInformation : NullableSnowflakeObject
{
	/// <summary>
	///     Gets or sets the upload url.
	/// </summary>
	[JsonProperty("upload_url")]
	internal Uri UploadUrl { get; set; }

	/// <summary>
	///     Gets the upload filename.
	/// </summary>
	[JsonProperty("upload_filename")]
	public string UploadFilename { get; internal set; }

	/// <summary>
	///     Gets the filename.
	/// </summary>
	[JsonIgnore]
	public string Filename { get; internal set; }

	/// <summary>
	///     Gets the description.
	/// </summary>
	[JsonIgnore]
	public string? Description { get; internal set; }
}

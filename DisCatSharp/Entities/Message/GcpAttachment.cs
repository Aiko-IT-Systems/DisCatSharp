using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

public sealed class GcpAttachment : ObservableApiObject
{
	[JsonProperty("file_size")]
	public int FileSize { get; internal set; }

	[JsonProperty("filename")]
	public string Filename { get; internal set; }

	internal GcpAttachment(string fileName, int fileSize)
	{
		this.Filename = fileName;
		this.FileSize = fileSize;
	}

	internal GcpAttachment(string filename, Stream file)
	{
		this.Filename = filename;
		this.FileSize = int.TryParse(file.Length.ToString(), out var size)
			? size
			: throw new FileLoadException("File size too big", filename);
	}
}

public sealed class GcpAttachmentsResponse : ObservableApiObject
{
	[JsonProperty("attachments")]
	public List<GcpAttachmentUploadInformation> Attachments { get; set; } = [];
}

public sealed class GcpAttachmentUploadInformation : NullableSnowflakeObject
{
	[JsonProperty("upload_url")]
	internal Uri UploadUrl { get; set; }

	[JsonProperty("upload_filename")]
	public string UploadFilename { get; internal set; }

	[JsonIgnore]
	public string Filename { get; internal set; }

	[JsonIgnore]
	public string? Description { get; internal set; }
}

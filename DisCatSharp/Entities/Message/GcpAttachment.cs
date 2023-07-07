// This file is part of the DisCatSharp project, based off DSharpPlus.
// 
// Copyright (c) 2021-2023 AITSYS
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;

using DisCatSharp.Enums;

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
	public List<GcpAttachmentUploadInformation> Attachments { get; set; } = new();
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

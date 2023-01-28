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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.IO;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the File that should be sent to Discord from the <see cref="DisCatSharp.Entities.DiscordMessageBuilder"/>.
/// </summary>
public class DiscordMessageFile
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMessageFile"/> class.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <param name="stream">The stream.</param>
	/// <param name="resetPositionTo">The reset position to.</param>
	/// <param name="fileType">The file type.</param>
	/// <param name="contentType">The content type.</param>
	/// <param name="description">The description.</param>
	internal DiscordMessageFile(string fileName, Stream stream, long? resetPositionTo, string fileType = null, string contentType = null, string description = null)
	{
		this.FileName = fileName;
		this.FileType = fileType;
		this.ContentType = contentType;
		this.Stream = stream;
		this.ResetPositionTo = resetPositionTo;
		this.Description = description;
	}

	/// <summary>
	/// Gets the FileName of the File.
	/// </summary>
	public string FileName { get; internal set; }

	/// <summary>
	/// Gets the description of the File.
	/// </summary>
	public string Description { get; internal set; }

	/// <summary>
	/// Gets the stream of the File.
	/// </summary>
	public Stream Stream { get; internal set; }

	/// <summary>
	/// Gets or sets the file type.
	/// </summary>
	internal string FileType { get; set; }

	/// <summary>
	/// Gets or sets the content type.
	/// </summary>
	internal string ContentType { get; set; }

	/// <summary>
	/// Gets the position the File should be reset to.
	/// </summary>
	internal long? ResetPositionTo { get; set; }
}

using System.IO;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a gcp attachment.
/// </summary>
public sealed class GcpAttachment : ObservableApiObject
{
	/// <summary>
	/// Gets the file size.
	/// </summary>
	[JsonProperty("file_size")]
	public int FileSize { get; internal set; }

	/// <summary>
	/// Gets the filename.
	/// </summary>
	[JsonProperty("filename")]
	public string Filename { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="GcpAttachment"/>.
	/// </summary>
	/// <param name="filename">The file name.</param>
	/// <param name="fileSize">The file size.</param>
	internal GcpAttachment(string filename, int fileSize)
	{
		this.Filename = filename;
		this.FileSize = fileSize;
	}

	/// <summary>
	/// Constructs a new <see cref="GcpAttachment"/>.
	/// </summary>
	/// <param name="filename">The file name.</param>
	/// <param name="file">The file stream.</param>
	/// <exception cref="FileLoadException"></exception>
	internal GcpAttachment(string filename, Stream file)
	{
		this.Filename = filename;
		this.FileSize = int.TryParse(file.Length.ToString(), out var size)
			? size
			: throw new FileLoadException("File size too big", filename);
	}
}

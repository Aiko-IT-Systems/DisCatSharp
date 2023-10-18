using System.IO;

using DisCatSharp.Attributes;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the File that should be sent to Discord from the <see cref="DisCatSharp.Entities.DiscordMessageBuilder"/>.
/// </summary>
public class DiscordMessageFile
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMessageFile"/> class.
	/// </summary>
	/// <param name="filename">The file name.</param>
	/// <param name="stream">The stream.</param>
	/// <param name="resetPositionTo">The reset position to.</param>
	/// <param name="fileType">The file type.</param>
	/// <param name="contentType">The content type.</param>
	/// <param name="description">The description.</param>
	internal DiscordMessageFile(string filename, Stream stream, long? resetPositionTo, string fileType = null, string contentType = null, string description = null)
	{
		this.Filename = filename;
		this.FileType = fileType;
		this.ContentType = contentType;
		this.Stream = stream;
		this.ResetPositionTo = resetPositionTo;
		this.Description = description;
	}

	/// <summary>
	/// Gets the name of the File.
	/// </summary>
	public string Filename { get; internal set; }

	/// <summary>
	/// Gets the FileName of the File. Please use <see cref="Filename"/> in future.
	/// </summary>
	[Deprecated("Naming was incorrect, will be removed in future in favor of Filename")]
	public string FileName
		=> this.Filename;

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

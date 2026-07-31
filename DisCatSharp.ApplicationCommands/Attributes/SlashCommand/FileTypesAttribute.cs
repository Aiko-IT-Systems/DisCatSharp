using System;
using System.Linq;

using DisCatSharp;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
///     Defines allowed file types for an attachment parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class FileTypesAttribute : Attribute
{
	/// <summary>
	///     Defines allowed file types for an attachment parameter.
	/// </summary>
	/// <param name="fileTypes">The file types to allow.</param>
	public FileTypesAttribute(params string[] fileTypes)
	{
		if (fileTypes.Length is 0)
			throw new ArgumentException("At least one file type must be specified.", nameof(fileTypes));
		if (fileTypes.Length > 10)
			throw new ArgumentException("Only up to 10 file types can be specified.", nameof(fileTypes));
		if (!fileTypes.All(Utilities.IsValidFileTypeFilter))
			throw new ArgumentException("Only 'image', 'video', 'audio' and dot-prefixed extensions are supported.", nameof(fileTypes));

		this.FileTypes = [.. fileTypes];
	}

	/// <summary>
	///     Allowed file types.
	/// </summary>
	public string[] FileTypes { get; }
}

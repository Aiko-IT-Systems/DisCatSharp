using System;

using DisCatSharp.Entities;

namespace DisCatSharp.Net.Models;

/// <summary>
///     Represents a model for editing a soundboard sound.
/// </summary>
public sealed class SoundboardSoundEditModel : ObservableApiObject
{
	/// <summary>
	///     Sets the soundboard sound's new name.
	/// </summary>
	public Optional<string> Name
	{
		internal get;
		set
		{
			if (value is { HasValue: true, Value.Length: > 100 })
				throw new ArgumentException("Soundboard sound name cannot exceed 100 characters.", nameof(value));

			field = value;
		}
	}

	/// <summary>
	///     Sets the soundboard sound's volume (between 0.0 and 1.0).
	/// </summary>
	public Optional<double?> Volume
	{
		internal get;
		set
		{
			if (value is { HasValue: true, Value: < 0.0 or > 1.0 })
				throw new ArgumentException("Soundboard sound volume must be between 0.0 and 1.0.", nameof(value));

			field = value;
		}
	}

	/// <summary>
	///     Sets the soundboard sound's emoji id for a custom emoji.
	/// </summary>
	public Optional<ulong?> EmojiId { internal get; set; }

	/// <summary>
	///     Sets the soundboard sound's emoji name for a standard emoji.
	/// </summary>
	public Optional<string?> EmojiName { internal get; set; }
}

using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord guild directory channel.
/// </summary>
public class DiscordGuildDirectoryEntry : DiscordChannel, IEquatable<DiscordGuildDirectoryEntry>
{
	/// <summary>
	/// Gets the description of the directory entry.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// Gets the primary category of the directory entry.
	/// </summary>
	[JsonProperty("primary_category_id", NullValueHandling = NullValueHandling.Ignore)]
	public DirectoryCategory PrimaryCategory { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordGuildDirectoryEntry"/> class.
	/// </summary>
	internal DiscordGuildDirectoryEntry()
	{ }

#region Methods

#endregion

	/// <summary>
	/// Checks whether this <see cref="DiscordGuildDirectoryEntry"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordGuildDirectoryEntry"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordGuildDirectoryEntry);

	/// <summary>
	/// Checks whether this <see cref="DiscordGuildDirectoryEntry"/> is equal to another <see cref="DiscordGuildDirectoryEntry"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordGuildDirectoryEntry"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordGuildDirectoryEntry"/> is equal to this <see cref="DiscordGuildDirectoryEntry"/>.</returns>
	public bool Equals(DiscordGuildDirectoryEntry e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordGuildDirectoryEntry"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordGuildDirectoryEntry"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordGuildDirectoryEntry"/> objects are equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are equal.</returns>
	public static bool operator ==(DiscordGuildDirectoryEntry e1, DiscordGuildDirectoryEntry e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordGuildDirectoryEntry"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are not equal.</returns>
	public static bool operator !=(DiscordGuildDirectoryEntry e1, DiscordGuildDirectoryEntry e2)
		=> !(e1 == e2);
}

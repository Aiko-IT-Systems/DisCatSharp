// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord forum post tag.
/// </summary>
public class ForumPostTag : SnowflakeObject, IEquatable<ForumPostTag>
{
	/// <summary>
	/// Gets the name of this forum post tag.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the emoji id of the forum post tag.
	/// </summary>
	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EmojiId { get; internal set; }

	/// <summary>
	/// Gets the unicode emoji of the forum post tag.
	/// </summary>
	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
	internal string UnicodeEmojiString;


	/// <summary>
	/// Gets whether the tag can only be used by moderators.
	/// </summary>
	[JsonProperty("moderated", NullValueHandling = NullValueHandling.Ignore)]
	public bool Moderated { get; internal set; }

	/// <summary>
	/// Gets the unicode emoji.
	/// </summary>
	public DiscordEmoji UnicodeEmoji
		=> this.UnicodeEmojiString != null ? DiscordEmoji.FromName(this.Discord, $":{this.UnicodeEmojiString}:", false) : null;

	/// <summary>
	/// Checks whether this <see cref="ForumPostTag"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="ForumPostTag"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as ForumPostTag);

	/// <summary>
	/// Checks whether this <see cref="ForumPostTag"/> is equal to another <see cref="ForumPostTag"/>.
	/// </summary>
	/// <param name="e"><see cref="ForumPostTag"/> to compare to.</param>
	/// <returns>Whether the <see cref="ForumPostTag"/> is equal to this <see cref="ForumPostTag"/>.</returns>
	public bool Equals(ForumPostTag e)
		=> e is not null && (ReferenceEquals(this, e) || (this.Id == e.Id && this.Name == e.Name));

	/// <summary>
	/// Gets the hash code for this <see cref="ForumPostTag"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="ForumPostTag"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="ForumPostTag"/> objects are equal.
	/// </summary>
	/// <param name="e1">First forum post tag to compare.</param>
	/// <param name="e2">Second forum post tag to compare.</param>
	/// <returns>Whether the two forum post tags are equal.</returns>
	public static bool operator ==(ForumPostTag e1, ForumPostTag e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordEmoji"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First forum post tag to compare.</param>
	/// <param name="e2">Second forum post tag to compare.</param>
	/// <returns>Whether the two forum post tags are not equal.</returns>
	public static bool operator !=(ForumPostTag e1, ForumPostTag e2)
		=> !(e1 == e2);
}

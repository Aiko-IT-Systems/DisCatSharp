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

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord guild directory channel.
/// </summary>
public class DiscordGuildDirectoryChannel : DiscordChannel, IEquatable<DiscordGuildDirectoryChannel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordGuildDirectoryChannel"/> class.
	/// </summary>
	internal DiscordGuildDirectoryChannel()
	{ }

	[JsonIgnore]
	public IReadOnlyList<DiscordGuildDirectoryEntry> Entries =>
		this.Guild.ChannelsInternal.Values.Where(e => e.ParentId == this.Id).Select(x => x as DiscordGuildDirectoryEntry).ToList();

	#region Methods

	#endregion

	/// <summary>
	/// Checks whether this <see cref="DiscordGuildDirectoryChannel"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordGuildDirectoryChannel"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordGuildDirectoryChannel);

	/// <summary>
	/// Checks whether this <see cref="DiscordGuildDirectoryChannel"/> is equal to another <see cref="DiscordGuildDirectoryChannel"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordGuildDirectoryChannel"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordGuildDirectoryChannel"/> is equal to this <see cref="DiscordGuildDirectoryChannel"/>.</returns>
	public bool Equals(DiscordGuildDirectoryChannel e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordGuildDirectoryChannel"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordGuildDirectoryChannel"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordGuildDirectoryChannel"/> objects are equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are equal.</returns>
	public static bool operator ==(DiscordGuildDirectoryChannel e1, DiscordGuildDirectoryChannel e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordGuildDirectoryChannel"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are not equal.</returns>
	public static bool operator !=(DiscordGuildDirectoryChannel e1, DiscordGuildDirectoryChannel e2)
		=> !(e1 == e2);
}

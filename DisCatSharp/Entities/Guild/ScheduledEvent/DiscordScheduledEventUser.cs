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

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// The discord scheduled event user.
/// </summary>
public class DiscordScheduledEventUser : IEquatable<DiscordScheduledEventUser>
{
	/// <summary>
	/// Gets the client instance this object is tied to.
	/// </summary>
	[JsonIgnore]
	internal BaseDiscordClient Discord { get; set; }

	/// <summary>
	/// Gets the user.
	/// </summary>
	[JsonProperty("user")]
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the member.
	/// Only applicable when requested with `with_member`.
	/// </summary>
	[JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
	internal DiscordMember Member { get; set; }

	/// <summary>
	/// Gets the scheduled event.
	/// </summary>
	[JsonIgnore]
	public DiscordScheduledEvent ScheduledEvent
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) && guild.ScheduledEvents.TryGetValue(this.EventId, out var scheduledEvent) ? scheduledEvent : null;

	/// <summary>
	/// Gets or sets the event id.
	/// </summary>
	[JsonProperty("guild_scheduled_event_id")]
	internal ulong EventId { get; set; }

	/// <summary>
	/// Gets or sets the guild id.
	/// </summary>
	[JsonIgnore]
	internal ulong GuildId { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordScheduledEventUser"/> class.
	/// </summary>
	internal DiscordScheduledEventUser()
	{ }

	/// <summary>
	/// Checks whether this <see cref="DiscordScheduledEventUser"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordScheduledEventUser"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordScheduledEventUser);

	/// <summary>
	/// Checks whether this <see cref="DiscordScheduledEventUser"/> is equal to another <see cref="DiscordScheduledEventUser"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordScheduledEventUser"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordScheduledEventUser"/> is equal to this <see cref="DiscordScheduledEventUser"/>.</returns>
	public bool Equals(DiscordScheduledEventUser e)
		=> e is not null && (ReferenceEquals(this, e) || HashCode.Combine(this.User.Id, this.EventId) == HashCode.Combine(e.User.Id, e.EventId));

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordScheduledEventUser"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordScheduledEventUser"/>.</returns>
	public override int GetHashCode()
		=> HashCode.Combine(this.User.Id, this.EventId);

	/// <summary>
	/// Gets whether the two <see cref="DiscordScheduledEventUser"/> objects are equal.
	/// </summary>
	/// <param name="e1">First event to compare.</param>
	/// <param name="e2">Second event to compare.</param>
	/// <returns>Whether the two events are equal.</returns>
	public static bool operator ==(DiscordScheduledEventUser e1, DiscordScheduledEventUser e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || HashCode.Combine(e1.User.Id, e1.EventId) == HashCode.Combine(e2.User.Id, e2.EventId));
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordScheduledEventUser"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First event to compare.</param>
	/// <param name="e2">Second event to compare.</param>
	/// <returns>Whether the two events are not equal.</returns>
	public static bool operator !=(DiscordScheduledEventUser e1, DiscordScheduledEventUser e2)
		=> !(e1 == e2);
}

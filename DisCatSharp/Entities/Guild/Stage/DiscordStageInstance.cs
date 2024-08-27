using System;
using System.Collections.Generic;

using DisCatSharp.Attributes;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Stage instance.
/// </summary>
public class DiscordStageInstance : SnowflakeObject, IEquatable<DiscordStageInstance>
{
	/// <summary>
	/// Initializes a new instance of <see cref="DiscordStageInstance"/>.
	/// </summary>
	internal DiscordStageInstance()
		: base(["privacy_level"])
	{ }

	/// <summary>
	///     Gets the scheduled event id of the associated stage channel.
	/// </summary>
	[JsonProperty("guild_scheduled_event_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ScheduledEventId { get; internal set; }

	/// <summary>
	///     Gets the scheduled event of the associated stage channel.
	/// </summary>
	[JsonIgnore]
	public DiscordScheduledEvent? ScheduledEvent
		=> this.ScheduledEventId.HasValue
			? this.Guild.ScheduledEvents.GetValueOrDefault(this.ScheduledEventId.Value)
			: null;

	/// <summary>
	///     Gets the invite code of the associated stage channel.
	/// </summary>
	[JsonProperty("invite_code", NullValueHandling = NullValueHandling.Ignore)]
	public string? InviteCode { get; internal set; }

	/// <summary>
	///     Gets the guild id of the associated Stage channel.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	///     Gets the guild to which this channel belongs.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null!;

	/// <summary>
	///     Gets id of the associated Stage channel.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	///     Gets the topic of the Stage instance.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string? Topic { get; internal set; }

	/// <summary>
	///     Gets whether or not stage discovery is disabled.
	/// </summary>
	[JsonProperty("discoverable_disabled", NullValueHandling = NullValueHandling.Ignore), DiscordDeprecated("Will be removed due to the discovery removal.")]
	public bool DiscoverableDisabled { get; internal set; }

	/// <summary>
	///     Checks whether this <see cref="DiscordStageInstance" /> is equal to another <see cref="DiscordStageInstance" />.
	/// </summary>
	/// <param name="e"><see cref="DiscordStageInstance" /> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordStageInstance" /> is equal to this <see cref="DiscordStageInstance" />.</returns>
	public bool Equals(DiscordStageInstance e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	///     Checks whether this <see cref="DiscordStageInstance" /> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordStageInstance" />.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordStageInstance);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordStageInstance" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordStageInstance" />.</returns>
	public override int GetHashCode() => this.Id.GetHashCode();

	/// <summary>
	///     Gets whether the two <see cref="DiscordStageInstance" /> objects are equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are equal.</returns>
	public static bool operator ==(DiscordStageInstance e1, DiscordStageInstance e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	///     Gets whether the two <see cref="DiscordStageInstance" /> objects are not equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are not equal.</returns>
	public static bool operator !=(DiscordStageInstance e1, DiscordStageInstance e2)
		=> !(e1 == e2);
}

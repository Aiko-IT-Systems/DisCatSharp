using System.Collections.Generic;

using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a user presence.
/// </summary>
public sealed class DiscordPresence : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPresence" /> class.
	/// </summary>
	// TODO: Add broadcast field
	internal DiscordPresence()
		: base(["broadcast", "roles", "premium_since", "nick", "game", "processed_at_timestamp"])
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPresence" /> class.
	/// </summary>
	/// <param name="other">The other.</param>
	internal DiscordPresence(DiscordPresence other)
		: base(["broadcast", "roles", "premium_since", "nick", "game", "processed_at_timestamp"])
	{
		this.Discord = other.Discord;
		this.Activity = other.Activity;
		this.RawActivity = other.RawActivity;
		this.InternalActivities = other.InternalActivities;
		this.RawActivities = other.RawActivities;
		this.Status = other.Status;
		this.InternalUser = other.InternalUser;
	}

	/// <summary>
	///     Gets the internal activities.
	/// </summary>
	[JsonIgnore]
	internal List<DiscordActivity>? InternalActivities { get; set; }

	/// <summary>
	///     Gets the discord client.
	/// </summary>
	[JsonIgnore]
	public new DiscordClient Discord { get; set; }

	/// <summary>
	///     Gets the internal user.
	/// </summary>
	[JsonProperty("user")]
	internal UserWithIdOnly InternalUser { get; set; }

	/// <summary>
	///     Gets the user that owns this presence.
	/// </summary>
	[JsonIgnore]
	public DiscordUser User
		=> this.Discord.GetCachedOrEmptyUserInternal(this.InternalUser.Id);

	/// <summary>
	///     Gets the user's current activity.
	/// </summary>
	[JsonIgnore]
	public DiscordActivity? Activity { get; internal set; }

	/// <summary>
	///     Gets the raw activity.
	/// </summary>
	[JsonIgnore]
	internal TransportActivity? RawActivity { get; set; }

	/// <summary>
	///     Gets the user's current activities.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordActivity>? Activities
		=> this.InternalActivities;

	/// <summary>
	///     Gets the raw activities.
	/// </summary>
	[JsonProperty("activities", NullValueHandling = NullValueHandling.Ignore)]
	internal List<TransportActivity>? RawActivities { get; set; }

	/// <summary>
	///     Gets this user's status.
	/// </summary>
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public UserStatus Status { get; internal set; }

	/// <summary>
	///     Gets the guild id for which this presence was set.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong GuildId { get; set; }

	/// <summary>
	///     Gets the guild for which this presence was set.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.GuildId != 0 ? this.Discord.GuildsInternal[this.GuildId] : null;

	/// <summary>
	///     Gets this user's platform-dependent status.
	/// </summary>
	[JsonProperty("client_status", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordClientStatus ClientStatus { get; internal set; }
}

/// <summary>
///     Represents a user with only its id.
/// </summary>
public sealed class UserWithIdOnly : ObservableApiObject
{
	[JsonProperty("id")]
	public ulong Id { get; internal set; }
}

/// <summary>
///     Represents a client status.
/// </summary>
public sealed class DiscordClientStatus : ObservableApiObject
{
	/// <summary>
	///     Gets the user's status set for an active desktop (Windows, Linux, Mac) application session.
	/// </summary>
	[JsonProperty("desktop", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
	public Optional<UserStatus> Desktop { get; internal set; } = UserStatus.Offline;

	/// <summary>
	///     Gets the user's status set for an active mobile (iOS, Android) application session.
	/// </summary>
	[JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
	public Optional<UserStatus> Mobile { get; internal set; } = UserStatus.Offline;

	/// <summary>
	///     Gets the user's status set for an active web (browser, bot account) application session.
	/// </summary>
	[JsonProperty("web", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
	public Optional<UserStatus> Web { get; internal set; } = UserStatus.Offline;
}

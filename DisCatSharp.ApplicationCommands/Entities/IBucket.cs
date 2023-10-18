using System;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Defines the standard contract for bucket feature
/// </summary>
public interface IBucket
{
	/// <summary>
	/// Gets the ID of the user whom this cooldown is associated
	/// </summary>
	ulong UserId { get; }

	/// <summary>
	/// Gets the ID of the channel with which this cooldown is associated
	/// </summary>
	ulong ChannelId { get; }

	/// <summary>
	/// Gets the ID of the guild with which this cooldown is associated
	/// </summary>
	ulong GuildId { get; }

	/// <summary>
	/// Gets the ID of the bucket. This is used to distinguish between cooldown buckets
	/// </summary>
	string BucketId { get; }

	/// <summary>
	/// Gets the remaining number of uses before the cooldown is triggered
	/// </summary>
	int RemainingUses { get; }

	/// <summary>
	/// Gets the maximum number of times this command can be used in a given timespan
	/// </summary>
	int MaxUses { get; }

	/// <summary>
	/// Gets the date and time at which the cooldown resets
	/// </summary>
	DateTimeOffset ResetsAt { get; }

	/// <summary>
	/// Get the time after which this cooldown resets
	/// </summary>
	TimeSpan Reset { get; }
}

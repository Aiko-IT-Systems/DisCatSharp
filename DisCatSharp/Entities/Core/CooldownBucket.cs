using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Entities.Core;

/// <summary>
///     Represents a cooldown bucket.
/// </summary>
// ReSharper disable once ClassCanBeSealed.Global "This class can be inherited from by developers."
public class CooldownBucket : IBucket, IEquatable<CooldownBucket>
{
	/// <summary>
	///     Gets the semaphore used to lock the use value.
	/// </summary>
	internal readonly SemaphoreSlim UsageSemaphore;

	/// <summary>
	///     Gets the remaining uses for this bucket.
	/// </summary>
	internal int RemainingUsesInternal;

	/// <summary>
	///     Creates a new command cooldown bucket.
	/// </summary>
	/// <param name="maxUses">Maximum number of uses for this bucket.</param>
	/// <param name="resetAfter">Time after which this bucket resets.</param>
	/// <param name="commandId">ID of the command</param>
	/// <param name="commandName">Name of the command.</param>
	/// <param name="userId">ID of the user with which this cooldown is associated.</param>
	/// <param name="channelId">ID of the channel with which this cooldown is associated.</param>
	/// <param name="guildId">ID of the guild with which this cooldown is associated.</param>
	/// <param name="memberId">ID of the member with which this cooldown is associated.</param>
	internal CooldownBucket(int maxUses, TimeSpan resetAfter, string commandName, string commandId, ulong userId = 0, ulong channelId = 0, ulong guildId = 0, ulong memberId = 0)
	{
		this.RemainingUsesInternal = maxUses;
		this.MaxUses = maxUses;
		this.ResetsAt = DateTimeOffset.UtcNow + resetAfter;
		this.Reset = resetAfter;
		this.UserId = userId;
		this.ChannelId = channelId;
		this.GuildId = guildId;
		this.MemberId = memberId;
		this.BucketId = MakeId(commandId, commandName, userId, channelId, guildId, memberId);
		this.UsageSemaphore = new(1, 1);
	}

	/// <summary>
	///     The member id for this bucket.
	/// </summary>
	public ulong MemberId { get; }

	/// <summary>
	///     The user id for this bucket.
	/// </summary>
	public ulong UserId { get; }

	/// <summary>
	///     The channel id for this bucket.
	/// </summary>
	public ulong ChannelId { get; }

	/// <summary>
	///     The guild id for this bucket.
	/// </summary>
	public ulong GuildId { get; }

	/// <summary>
	///     The id for this bucket.
	/// </summary>
	public string BucketId { get; }

	/// <summary>
	///     The remaining uses for this bucket.
	/// </summary>
	public int RemainingUses
		=> Volatile.Read(ref this.RemainingUsesInternal);

	/// <summary>
	///     The max uses for this bucket.
	/// </summary>
	public int MaxUses { get; }

	/// <summary>
	///     The datetime offset when this bucket resets.
	/// </summary>
	public DateTimeOffset ResetsAt { get; internal set; }

	/// <summary>
	///     The timespan when this bucket resets.
	/// </summary>
	public TimeSpan Reset { get; internal set; }

	/// <summary>
	///     Checks whether this <see cref="CooldownBucket" /> is equal to another <see cref="CooldownBucket" />.
	/// </summary>
	/// <param name="other"><see cref="CooldownBucket" /> to compare to.</param>
	/// <returns>Whether the <see cref="CooldownBucket" /> is equal to this <see cref="CooldownBucket" />.</returns>
	public bool Equals(CooldownBucket? other)
		=> other is not null && (ReferenceEquals(this, other) || (this.UserId == other.UserId && this.ChannelId == other.ChannelId && this.GuildId == other.GuildId && this.MemberId == other.MemberId));

	/// <summary>
	///     Decrements the remaining use counter.
	/// </summary>
	/// <param name="ctx">The context.</param>
	/// <returns>Whether decrement succeeded or not.</returns>
	internal async Task<bool> DecrementUseAsync(DisCatSharpCommandContext ctx)
	{
		await this.UsageSemaphore.WaitAsync().ConfigureAwait(false);
		ctx.Client.Logger.LogDebug($"[Cooldown::prev_check({ctx.FullCommandName})]:\n\tRemaining: {this.RemainingUses}/{this.MaxUses}\n\tResets: {this.ResetsAt}\n\tNow: {DateTimeOffset.UtcNow}\n\tVars[u,c,g,m]: {this.UserId} {this.ChannelId} {this.GuildId} {this.MemberId}\n\tId: {this.BucketId}");

		var now = DateTimeOffset.UtcNow;
		if (now >= this.ResetsAt)
		{
			Interlocked.Exchange(ref this.RemainingUsesInternal, this.MaxUses);
			this.ResetsAt = now + this.Reset;
		}

		ctx.Client.Logger.LogDebug($"[Cooldown::check({ctx.FullCommandName})]:\n\tRemaining: {this.RemainingUses}/{this.MaxUses}\n\tResets: {this.ResetsAt}\n\tNow: {DateTimeOffset.UtcNow}\n\tVars[u,c,g,m]: {this.UserId} {this.ChannelId} {this.GuildId} {this.MemberId}\n\tId: {this.BucketId}");

		var success = false;
		if (this.RemainingUses > 0)
		{
			Interlocked.Decrement(ref this.RemainingUsesInternal);
			success = true;
		}

		this.UsageSemaphore.Release();
		if (success)
			return success;

		ctx.Client.Logger.LogWarning($"[Cooldown::hit({ctx.FullCommandName})]:\n\tRemaining: {this.RemainingUses}/{this.MaxUses}\n\tResets: {this.ResetsAt}\n\tNow: {DateTimeOffset.UtcNow}\n\tVars[u,c,g,m]: {this.UserId} {this.ChannelId} {this.GuildId} {this.MemberId}\n\tId: {this.BucketId}");

		return success;
	}

	/// <summary>
	///     Checks whether this <see cref="CooldownBucket" /> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="CooldownBucket" />.</returns>
	public override bool Equals(object? obj)
		=> this.Equals(obj as CooldownBucket);

	/// <summary>
	///     Gets the hash code for this <see cref="CooldownBucket" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="CooldownBucket" />.</returns>
	public override int GetHashCode()
		=> HashCode.Combine(this.UserId, this.ChannelId, this.GuildId, this.MemberId);

	/// <summary>
	///     Gets whether the two <see cref="CooldownBucket" /> objects are equal.
	/// </summary>
	/// <param name="bucket1">First bucket to compare.</param>
	/// <param name="bucket2">Second bucket to compare.</param>
	/// <returns>Whether the two buckets are equal.</returns>
	public static bool operator ==(CooldownBucket? bucket1, CooldownBucket? bucket2)
	{
		var null1 = bucket1 is null;
		var null2 = bucket2 is null;

		return (null1 && null2) || (null1 == null2 && null1.Equals(null2));
	}

	/// <summary>
	///     Gets whether the two <see cref="CooldownBucket" /> objects are not equal.
	/// </summary>
	/// <param name="bucket1">First bucket to compare.</param>
	/// <param name="bucket2">Second bucket to compare.</param>
	/// <returns>Whether the two buckets are not equal.</returns>
	public static bool operator !=(CooldownBucket bucket1, CooldownBucket bucket2)
		=> !(bucket1 == bucket2);

	/// <summary>
	///     Creates a bucket ID from given bucket parameters.
	/// </summary>
	/// <param name="commandId">ID of the command</param>
	/// <param name="commandName">Name of the command.</param>
	/// <param name="userId">ID of the user with which this cooldown is associated.</param>
	/// <param name="channelId">ID of the channel with which this cooldown is associated.</param>
	/// <param name="guildId">ID of the guild with which this cooldown is associated.</param>
	/// <param name="memberId">ID of the member with which this cooldown is associated.</param>
	/// <returns>Generated bucket ID.</returns>
	public static string MakeId(string commandId, string commandName, ulong userId = 0, ulong channelId = 0, ulong guildId = 0, ulong memberId = 0)
		=> $"{commandId}:{commandName}::{userId.ToString(CultureInfo.InvariantCulture)}:{channelId.ToString(CultureInfo.InvariantCulture)}:{guildId.ToString(CultureInfo.InvariantCulture)}:{memberId.ToString(CultureInfo.InvariantCulture)}";
}

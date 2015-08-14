using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a rate limit bucket.
/// </summary>
internal sealed class RateLimitBucket : IEquatable<RateLimitBucket>
{
	/// <summary>
	/// Gets the Id of the guild bucket.
	/// </summary>
	public string GuildId { get; }

	/// <summary>
	/// Gets the Id of the channel bucket.
	/// </summary>
	public string ChannelId { get; }

	/// <summary>
	/// Gets the ID of the webhook bucket.
	/// </summary>
	public string WebhookId { get; }

	/// <summary>
	/// Gets the Id of the ratelimit bucket.
	/// </summary>
	public volatile string? BucketId;

	/// <summary>
	/// Gets or sets the ratelimit hash of this bucket.
	/// </summary>
	public string Hash
	{
		get => Volatile.Read(ref this.HashInternal);

		internal set
		{
			this.IsUnlimited = value.Contains(s_unlimitedHash);

			if (this.BucketId is not null && !this.BucketId.StartsWith(value, StringComparison.Ordinal))
			{
				var id = GenerateBucketId(value, this.GuildId, this.ChannelId, this.WebhookId);
				this.BucketId = id;
				this.RouteHashes.Add(id);
			}

			Volatile.Write(ref this.HashInternal, value);
		}
	}

	/// <summary>
	/// Gets the ratelimit hash of this bucket.
	/// </summary>
	internal string HashInternal;

	/// <summary>
	/// Gets the past route hashes associated with this bucket.
	/// </summary>
	public ConcurrentBag<string> RouteHashes { get; }

	/// <summary>
	/// Gets when this bucket was last called in a request.
	/// </summary>
	public DateTimeOffset LastAttemptAt { get; internal set; }

	/// <summary>
	/// Gets the number of uses left before pre-emptive rate limit is triggered.
	/// </summary>
	public int Remaining
		=> this.RemainingInternal;

	/// <summary>
	/// Gets the maximum number of uses within a single bucket.
	/// </summary>
	public int Maximum { get; set; }

	/// <summary>
	/// Gets the timestamp at which the rate limit resets.
	/// </summary>
	public DateTimeOffset Reset { get; internal set; }

	/// <summary>
	/// Gets the time interval to wait before the rate limit resets.
	/// </summary>
	public TimeSpan? ResetAfter { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the ratelimit global.
	/// </summary>
	public bool IsGlobal { get; internal set; } = false;

	/// <summary>
	/// Gets the ratelimit scope.
	/// </summary>
	public string Scope { get; internal set; } = "user";

	/// <summary>
	/// Gets the time interval to wait before the rate limit resets as offset.
	/// </summary>
	internal DateTimeOffset ResetAfterOffset { get; set; }

	/// <summary>
	/// Gets or sets the remaining number of requests to the maximum when the ratelimit is reset.
	/// </summary>
	internal volatile int RemainingInternal;

	/// <summary>
	/// Gets whether this bucket has it's ratelimit determined.
	/// <para>This will be <see langword="false"/> if the ratelimit is determined.</para>
	/// </summary>
	internal volatile bool IsUnlimited;

	/// <summary>
	/// If the initial request for this bucket that is determining the rate limits is currently executing.
	/// This is a int because booleans can't be accessed atomically.
	/// 0 => False, all other values => True
	/// </summary>
	internal volatile int LimitTesting;

	/// <summary>
	/// Task to wait for the rate limit test to finish.
	/// </summary>
	internal volatile Task? LimitTestFinished;

	/// <summary>
	/// If the rate limits have been determined.
	/// </summary>
	internal volatile bool LimitValid;

	/// <summary>
	/// Rate limit reset in ticks, UTC on the next response after the rate limit has been reset.
	/// </summary>
	internal long NextReset;

	/// <summary>
	/// If the rate limit is currently being reset.
	/// This is a int because booleans can't be accessed atomically.
	/// 0 => False, all other values => True
	/// </summary>
	internal volatile int LimitResetting;

	/// <summary>
	/// Gets the unlimited hash.
	/// </summary>
	private static readonly string s_unlimitedHash = "unlimited";

	/// <summary>
	/// Initializes a new instance of the <see cref="RateLimitBucket"/> class.
	/// </summary>
	/// <param name="hash">The hash.</param>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="webhookId">The webhook_id.</param>
	internal RateLimitBucket(string hash, string guildId, string channelId, string webhookId)
	{
		this.Hash = hash;
		this.ChannelId = channelId;
		this.GuildId = guildId;
		this.WebhookId = webhookId;

		this.BucketId = GenerateBucketId(hash, guildId, channelId, webhookId);
		this.RouteHashes = [];
	}

	/// <summary>
	/// Generates an ID for this request bucket.
	/// </summary>
	/// <param name="hash">Hash for this bucket.</param>
	/// <param name="guildId">Guild Id for this bucket.</param>
	/// <param name="channelId">Channel Id for this bucket.</param>
	/// <param name="webhookId">Webhook Id for this bucket.</param>
	/// <returns>Bucket Id.</returns>
	public static string GenerateBucketId(string hash, string guildId, string channelId, string webhookId)
		=> $"{hash}:{guildId}:{channelId}:{webhookId}";

	/// <summary>
	/// Generates the hash key.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <returns>A string.</returns>
	public static string GenerateHashKey(RestRequestMethod method, string route)
		=> $"{method}:{route}";

	/// <summary>
	/// Generates the unlimited hash.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <returns>A string.</returns>
	public static string GenerateUnlimitedHash(RestRequestMethod method, string route)
		=> $"{GenerateHashKey(method, route)}:{s_unlimitedHash}";

	/// <summary>
	/// Returns a string representation of this bucket.
	/// </summary>
	/// <returns>String representation of this bucket.</returns>
	public override string ToString()
	{
		var guildId = this.GuildId != string.Empty ? this.GuildId : "guild_id";
		var channelId = this.ChannelId != string.Empty ? this.ChannelId : "channel_id";
		var webhookId = this.WebhookId != string.Empty ? this.WebhookId : "webhook_id";

		return $"{this.Scope} rate limit bucket [{this.Hash}:{guildId}:{channelId}:{webhookId}] [{this.Remaining}/{this.Maximum}] {(this.ResetAfter.HasValue ? this.ResetAfterOffset : this.Reset)}";
	}

	/// <summary>
	/// Checks whether this <see cref="RateLimitBucket"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="RateLimitBucket"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as RateLimitBucket);

	/// <summary>
	/// Checks whether this <see cref="RateLimitBucket"/> is equal to another <see cref="RateLimitBucket"/>.
	/// </summary>
	/// <param name="e"><see cref="RateLimitBucket"/> to compare to.</param>
	/// <returns>Whether the <see cref="RateLimitBucket"/> is equal to this <see cref="RateLimitBucket"/>.</returns>
	public bool Equals(RateLimitBucket e) => e is not null && (ReferenceEquals(this, e) || this.BucketId == e.BucketId);

	/// <summary>
	/// Gets the hash code for this <see cref="RateLimitBucket"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="RateLimitBucket"/>.</returns>
	public override int GetHashCode()
		=> this.BucketId?.GetHashCode() ?? -1;

	/// <summary>
	/// Sets remaining number of requests to the maximum when the ratelimit is reset
	/// </summary>
	/// <param name="now">The datetime offset.</param>
	internal async Task TryResetLimitAsync(DateTimeOffset now)
	{
		if (this.ResetAfter.HasValue)
			this.ResetAfter = this.ResetAfterOffset - now;

		if (this.NextReset is 0)
			return;

		if (this.NextReset > now.UtcTicks)
			return;

		while (Interlocked.CompareExchange(ref this.LimitResetting, 1, 0) != 0)
			await Task.Yield();

		if (this.NextReset is not 0)
		{
			this.RemainingInternal = this.Maximum;
			this.NextReset = 0;
		}

		this.LimitResetting = 0;
	}

	/// <summary>
	/// Sets the initial values.
	/// </summary>
	/// <param name="max">The max.</param>
	/// <param name="usesLeft">The uses left.</param>
	/// <param name="newReset">The new reset.</param>
	internal void SetInitialValues(int max, int usesLeft, DateTimeOffset newReset)
	{
		this.Maximum = max;
		this.RemainingInternal = usesLeft;
		this.NextReset = newReset.UtcTicks;

		this.LimitValid = true;
		this.LimitTestFinished = null;
		this.LimitTesting = 0;
	}
}

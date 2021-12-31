// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Net
{
    /// <summary>
    /// Represents a rate limit bucket.
    /// </summary>
    internal class RateLimitBucket : IEquatable<RateLimitBucket>
    {
        /// <summary>
        /// Gets the Id of the guild bucket.
        /// </summary>
        public string GuildId { get; internal set; }

        /// <summary>
        /// Gets the Id of the channel bucket.
        /// </summary>
        public string ChannelId { get; internal set; }

        /// <summary>
        /// Gets the ID of the webhook bucket.
        /// </summary>
        public string WebhookId { get; internal set; }

        /// <summary>
        /// Gets the Id of the ratelimit bucket.
        /// </summary>
        public volatile string BucketId;

        /// <summary>
        /// Gets or sets the ratelimit hash of this bucket.
        /// </summary>
        public string Hash
        {
            get => Volatile.Read(ref this._hash);

            internal set
            {
                this._isUnlimited = value.Contains(_unlimitedHash);

                if (this.BucketId != null && !this.BucketId.StartsWith(value))
                {
                    var id = GenerateBucketId(value, this.GuildId, this.ChannelId, this.WebhookId);
                    this.BucketId = id;
                    this.RouteHashes.Add(id);
                }

                Volatile.Write(ref this._hash, value);
            }
        }

        internal string _hash;

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
            => this._remaining;

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
        public TimeSpan? ResetAfter { get; internal set; } = null;

        /// <summary>
        /// Gets a value indicating whether the ratelimit global.
        /// </summary>
        public bool IsGlobal { get; internal set; } = false;

        /// <summary>
        /// Gets the ratelimit scope.
        /// </summary>
        public string Scope { get; internal set; } = "user";

        /// <summary>
        /// Gets the time interval to wait before the rate limit resets as offset
        /// </summary>
        internal DateTimeOffset ResetAfterOffset { get; set; }

        internal volatile int _remaining;

        /// <summary>
        /// Gets whether this bucket has it's ratelimit determined.
        /// <para>This will be <see langword="false"/> if the ratelimit is determined.</para>
        /// </summary>
        internal volatile bool _isUnlimited;

        /// <summary>
        /// If the initial request for this bucket that is deterternining the rate limits is currently executing
        /// This is a int because booleans can't be accessed atomically
        /// 0 => False, all other values => True
        /// </summary>
        internal volatile int _limitTesting;

        /// <summary>
        /// Task to wait for the rate limit test to finish
        /// </summary>
        internal volatile Task _limitTestFinished;

        /// <summary>
        /// If the rate limits have been determined
        /// </summary>
        internal volatile bool _limitValid;

        /// <summary>
        /// Rate limit reset in ticks, UTC on the next response after the rate limit has been reset
        /// </summary>
        internal long _nextReset;

        /// <summary>
        /// If the rate limit is currently being reset.
        /// This is a int because booleans can't be accessed atomically.
        /// 0 => False, all other values => True
        /// </summary>
        internal volatile int _limitResetting;

        private static readonly string _unlimitedHash = "unlimited";

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitBucket"/> class.
        /// </summary>
        /// <param name="Hash">The hash.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="webhook_id">The webhook_id.</param>
        internal RateLimitBucket(string Hash, string GuildId, string ChannelId, string WebhookId)
        {
            this.Hash = Hash;
            this.ChannelId = ChannelId;
            this.GuildId = GuildId;
            this.WebhookId = WebhookId;

            this.BucketId = GenerateBucketId(Hash, GuildId, ChannelId, WebhookId);
            this.RouteHashes = new ConcurrentBag<string>();
        }

        /// <summary>
        /// Generates an ID for this request bucket.
        /// </summary>
        /// <param name="Hash">Hash for this bucket.</param>
        /// <param name="guild_id">Guild Id for this bucket.</param>
        /// <param name="channel_id">Channel Id for this bucket.</param>
        /// <param name="webhook_id">Webhook Id for this bucket.</param>
        /// <returns>Bucket Id.</returns>
        public static string GenerateBucketId(string Hash, string GuildId, string ChannelId, string WebhookId)
            => $"{Hash}:{GuildId}:{ChannelId}:{WebhookId}";

        /// <summary>
        /// Generates the hash key.
        /// </summary>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <returns>A string.</returns>
        public static string GenerateHashKey(RestRequestMethod Method, string Route)
            => $"{Method}:{Route}";

        /// <summary>
        /// Generates the unlimited hash.
        /// </summary>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <returns>A string.</returns>
        public static string GenerateUnlimitedHash(RestRequestMethod Method, string Route)
            => $"{GenerateHashKey(Method, Route)}:{_unlimitedHash}";

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
        /// <param name="Obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="RateLimitBucket"/>.</returns>
        public override bool Equals(object Obj)
            => this.Equals(Obj as RateLimitBucket);

        /// <summary>
        /// Checks whether this <see cref="RateLimitBucket"/> is equal to another <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <param name="E"><see cref="RateLimitBucket"/> to compare to.</param>
        /// <returns>Whether the <see cref="RateLimitBucket"/> is equal to this <see cref="RateLimitBucket"/>.</returns>
        public bool Equals(RateLimitBucket E) => E is not null && (ReferenceEquals(this, E) || this.BucketId == E.BucketId);

        /// <summary>
        /// Gets the hash code for this <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="RateLimitBucket"/>.</returns>
        public override int GetHashCode()
            => this.BucketId.GetHashCode();

        /// <summary>
        /// Sets remaining number of requests to the maximum when the ratelimit is reset
        /// </summary>
        /// <param name="Now"></param>
        internal async Task TryResetLimitAsync(DateTimeOffset Now)
        {
            if (this.ResetAfter.HasValue)
                this.ResetAfter = this.ResetAfterOffset - Now;

            if (this._nextReset == 0)
                return;

            if (this._nextReset > Now.UtcTicks)
                return;

            while (Interlocked.CompareExchange(ref this._limitResetting, 1, 0) != 0)
#pragma warning restore 420
                await Task.Yield();

            if (this._nextReset != 0)
            {
                this._remaining = this.Maximum;
                this._nextReset = 0;
            }

            this._limitResetting = 0;
        }

        /// <summary>
        /// Sets the initial values.
        /// </summary>
        /// <param name="Max">The max.</param>
        /// <param name="UsesLeft">The uses left.</param>
        /// <param name="NewReset">The new reset.</param>
        internal void SetInitialValues(int Max, int UsesLeft, DateTimeOffset NewReset)
        {
            this.Maximum = Max;
            this._remaining = UsesLeft;
            this._nextReset = NewReset.UtcTicks;

            this._limitValid = true;
            this._limitTestFinished = null;
            this._limitTesting = 0;
        }
    }
}

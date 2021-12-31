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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.CommandsNext.Attributes
{
    /// <summary>
    /// Defines a cooldown for this command. This allows you to define how many times can users execute a specific command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class CooldownAttribute : CheckBaseAttribute
    {
        /// <summary>
        /// Gets the maximum number of uses before this command triggers a cooldown for its bucket.
        /// </summary>
        public int MaxUses { get; }

        /// <summary>
        /// Gets the time after which the cooldown is reset.
        /// </summary>
        public TimeSpan Reset { get; }

        /// <summary>
        /// Gets the type of the cooldown bucket. This determines how cooldowns are applied.
        /// </summary>
        public CooldownBucketType BucketType { get; }

        /// <summary>
        /// Gets the cooldown buckets for this command.
        /// </summary>
        private ConcurrentDictionary<string, CommandCooldownBucket> Buckets { get; }

        /// <summary>
        /// Defines a cooldown for this command. This means that users will be able to use the command a specific number of times before they have to wait to use it again.
        /// </summary>
        /// <param name="MaxUses">Number of times the command can be used before triggering a cooldown.</param>
        /// <param name="ResetAfter">Number of seconds after which the cooldown is reset.</param>
        /// <param name="BucketType">Type of cooldown bucket. This allows controlling whether the bucket will be cooled down per user, guild, channel, or globally.</param>
        public CooldownAttribute(int MaxUses, double ResetAfter, CooldownBucketType BucketType)
        {
            this.MaxUses = MaxUses;
            this.Reset = TimeSpan.FromSeconds(ResetAfter);
            this.BucketType = BucketType;
            this.Buckets = new ConcurrentDictionary<string, CommandCooldownBucket>();
        }

        /// <summary>
        /// Gets a cooldown bucket for given command context.
        /// </summary>
        /// <param name="Ctx">Command context to get cooldown bucket for.</param>
        /// <returns>Requested cooldown bucket, or null if one wasn't present.</returns>
        public CommandCooldownBucket GetBucket(CommandContext Ctx)
        {
            var bid = this.GetBucketId(Ctx, out _, out _, out _);
            this.Buckets.TryGetValue(bid, out var bucket);
            return bucket;
        }

        /// <summary>
        /// Calculates the cooldown remaining for given command context.
        /// </summary>
        /// <param name="Ctx">Context for which to calculate the cooldown.</param>
        /// <returns>Remaining cooldown, or zero if no cooldown is active.</returns>
        public TimeSpan GetRemainingCooldown(CommandContext Ctx)
        {
            var bucket = this.GetBucket(Ctx);
            return bucket == null ? TimeSpan.Zero : bucket.RemainingUses > 0 ? TimeSpan.Zero : bucket.ResetsAt - DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Calculates bucket ID for given command context.
        /// </summary>
        /// <param name="Ctx">Context for which to calculate bucket ID for.</param>
        /// <param name="UserId">ID of the user with which this bucket is associated.</param>
        /// <param name="ChannelId">ID of the channel with which this bucket is associated.</param>
        /// <param name="GuildId">ID of the guild with which this bucket is associated.</param>
        /// <returns>Calculated bucket ID.</returns>
        private string GetBucketId(CommandContext Ctx, out ulong UserId, out ulong ChannelId, out ulong GuildId)
        {
            UserId = 0ul;
            if ((this.BucketType & CooldownBucketType.User) != 0)
                UserId = Ctx.User.Id;

            ChannelId = 0ul;
            if ((this.BucketType & CooldownBucketType.Channel) != 0)
                ChannelId = Ctx.Channel.Id;
            if ((this.BucketType & CooldownBucketType.Guild) != 0 && Ctx.Guild == null)
                ChannelId = Ctx.Channel.Id;

            GuildId = 0ul;
            if (Ctx.Guild != null && (this.BucketType & CooldownBucketType.Guild) != 0)
                GuildId = Ctx.Guild.Id;

            var bid = CommandCooldownBucket.MakeId(UserId, ChannelId, GuildId);
            return bid;
        }

        /// <summary>
        /// Executes a check.
        /// </summary>
        /// <param name="Ctx">The command context.</param>
        /// <param name="Help">If true, help - returns true.</param>
        public override async Task<bool> ExecuteCheck(CommandContext Ctx, bool Help)
        {
            if (Help)
                return true;

            var bid = this.GetBucketId(Ctx, out var usr, out var chn, out var gld);
            if (!this.Buckets.TryGetValue(bid, out var bucket))
            {
                bucket = new CommandCooldownBucket(this.MaxUses, this.Reset, usr, chn, gld);
                this.Buckets.AddOrUpdate(bid, bucket, (K, V) => bucket);
            }

            return await bucket.DecrementUseAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Defines how are command cooldowns applied.
    /// </summary>
    public enum CooldownBucketType : int
    {
        /// <summary>
        /// Denotes that the command will have its cooldown applied per-user.
        /// </summary>
        User = 1,

        /// <summary>
        /// Denotes that the command will have its cooldown applied per-channel.
        /// </summary>
        Channel = 2,

        /// <summary>
        /// Denotes that the command will have its cooldown applied per-guild. In DMs, this applies the cooldown per-channel.
        /// </summary>
        Guild = 4,

        /// <summary>
        /// Denotes that the command will have its cooldown applied globally.
        /// </summary>
        Global = 0
    }

    /// <summary>
    /// Represents a cooldown bucket for commands.
    /// </summary>
    public sealed class CommandCooldownBucket : IEquatable<CommandCooldownBucket>
    {
        /// <summary>
        /// Gets the ID of the user with whom this cooldown is associated.
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        /// Gets the ID of the channel with which this cooldown is associated.
        /// </summary>
        public ulong ChannelId { get; }

        /// <summary>
        /// Gets the ID of the guild with which this cooldown is associated.
        /// </summary>
        public ulong GuildId { get; }

        /// <summary>
        /// Gets the ID of the bucket. This is used to distinguish between cooldown buckets.
        /// </summary>
        public string BucketId { get; }

        /// <summary>
        /// Gets the remaining number of uses before the cooldown is triggered.
        /// </summary>
        public int RemainingUses
            => Volatile.Read(ref this._remainingUses);

        private int _remainingUses;

        /// <summary>
        /// Gets the maximum number of times this command can be used in given timespan.
        /// </summary>
        public int MaxUses { get; }

        /// <summary>
        /// Gets the date and time at which the cooldown resets.
        /// </summary>
        public DateTimeOffset ResetsAt { get; internal set; }

        /// <summary>
        /// Gets the time after which this cooldown resets.
        /// </summary>
        public TimeSpan Reset { get; internal set; }

        /// <summary>
        /// Gets the semaphore used to lock the use value.
        /// </summary>
        private SemaphoreSlim UsageSemaphore { get; }

        /// <summary>
        /// Creates a new command cooldown bucket.
        /// </summary>
        /// <param name="MaxUses">Maximum number of uses for this bucket.</param>
        /// <param name="ResetAfter">Time after which this bucket resets.</param>
        /// <param name="UserId">ID of the user with which this cooldown is associated.</param>
        /// <param name="ChannelId">ID of the channel with which this cooldown is associated.</param>
        /// <param name="GuildId">ID of the guild with which this cooldown is associated.</param>
        internal CommandCooldownBucket(int MaxUses, TimeSpan ResetAfter, ulong UserId = 0, ulong ChannelId = 0, ulong GuildId = 0)
        {
            this._remainingUses = MaxUses;
            this.MaxUses = MaxUses;
            this.ResetsAt = DateTimeOffset.UtcNow + ResetAfter;
            this.Reset = ResetAfter;
            this.UserId = UserId;
            this.ChannelId = ChannelId;
            this.GuildId = GuildId;
            this.BucketId = MakeId(UserId, ChannelId, GuildId);
            this.UsageSemaphore = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Decrements the remaining use counter.
        /// </summary>
        /// <returns>Whether decrement succeded or not.</returns>
        internal async Task<bool> DecrementUseAsync()
        {
            await this.UsageSemaphore.WaitAsync().ConfigureAwait(false);

            // if we're past reset time...
            var now = DateTimeOffset.UtcNow;
            if (now >= this.ResetsAt)
            {
                // ...do the reset and set a new reset time
                Interlocked.Exchange(ref this._remainingUses, this.MaxUses);
                this.ResetsAt = now + this.Reset;
            }

            // check if we have any uses left, if we do...
            var success = false;
            if (this.RemainingUses > 0)
            {
                // ...decrement, and return success...
                Interlocked.Decrement(ref this._remainingUses);
                success = true;
            }

            // ...otherwise just fail
            this.UsageSemaphore.Release();
            return success;
        }

        /// <summary>
        /// Returns a string representation of this command cooldown bucket.
        /// </summary>
        /// <returns>String representation of this command cooldown bucket.</returns>
        public override string ToString() => $"Command bucket {this.BucketId}";

        /// <summary>
        /// Checks whether this <see cref="CommandCooldownBucket"/> is equal to another object.
        /// </summary>
        /// <param name="Obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="CommandCooldownBucket"/>.</returns>
        public override bool Equals(object Obj) => this.Equals(Obj as CommandCooldownBucket);

        /// <summary>
        /// Checks whether this <see cref="CommandCooldownBucket"/> is equal to another <see cref="CommandCooldownBucket"/>.
        /// </summary>
        /// <param name="Other"><see cref="CommandCooldownBucket"/> to compare to.</param>
        /// <returns>Whether the <see cref="CommandCooldownBucket"/> is equal to this <see cref="CommandCooldownBucket"/>.</returns>
        public bool Equals(CommandCooldownBucket Other) => Other is not null && (ReferenceEquals(this, Other) || (this.UserId == Other.UserId && this.ChannelId == Other.ChannelId && this.GuildId == Other.GuildId));

        /// <summary>
        /// Gets the hash code for this <see cref="CommandCooldownBucket"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="CommandCooldownBucket"/>.</returns>
        public override int GetHashCode() => HashCode.Combine(this.UserId, this.ChannelId, this.GuildId);

        /// <summary>
        /// Gets whether the two <see cref="CommandCooldownBucket"/> objects are equal.
        /// </summary>
        /// <param name="Bucket1">First bucket to compare.</param>
        /// <param name="Bucket2">Second bucket to compare.</param>
        /// <returns>Whether the two buckets are equal.</returns>
        public static bool operator ==(CommandCooldownBucket Bucket1, CommandCooldownBucket Bucket2)
        {
            var null1 = Bucket1 is null;
            var null2 = Bucket2 is null;

            return (null1 && null2) || (null1 == null2 && null1.Equals(null2));
        }

        /// <summary>
        /// Gets whether the two <see cref="CommandCooldownBucket"/> objects are not equal.
        /// </summary>
        /// <param name="Bucket1">First bucket to compare.</param>
        /// <param name="Bucket2">Second bucket to compare.</param>
        /// <returns>Whether the two buckets are not equal.</returns>
        public static bool operator !=(CommandCooldownBucket Bucket1, CommandCooldownBucket Bucket2)
            => !(Bucket1 == Bucket2);

        /// <summary>
        /// Creates a bucket ID from given bucket parameters.
        /// </summary>
        /// <param name="UserId">ID of the user with which this cooldown is associated.</param>
        /// <param name="ChannelId">ID of the channel with which this cooldown is associated.</param>
        /// <param name="GuildId">ID of the guild with which this cooldown is associated.</param>
        /// <returns>Generated bucket ID.</returns>
        public static string MakeId(ulong UserId = 0, ulong ChannelId = 0, ulong GuildId = 0)
            => $"{UserId.ToString(CultureInfo.InvariantCulture)}:{ChannelId.ToString(CultureInfo.InvariantCulture)}:{GuildId.ToString(CultureInfo.InvariantCulture)}";
    }
}

using System;
using System.Threading.Tasks;

using DisCatSharp.CommandsNext.Entities;
using DisCatSharp.Entities;
using DisCatSharp.Entities.Core;
using DisCatSharp.Enums.Core;
using DisCatSharp.Exceptions;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Defines a cooldown for this command. This allows you to define how many times can users execute a specific command
/// </summary>
/// <remarks>
/// Defines a cooldown for this command. This means that users will be able to use the command a specific number of times before they have to wait to use it again.
/// </remarks>
/// <param name="maxUses">Number of times the command can be used before triggering a cooldown.</param>
/// <param name="resetAfter">Number of seconds after which the cooldown is reset.</param>
/// <param name="bucketType">Type of cooldown bucket. This allows controlling whether the bucket will be cooled down per user, guild, channel, or globally.</param>
/// <param name="cooldownResponderType">The responder type used to respond to cooldown ratelimit hits.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class CooldownAttribute(int maxUses, double resetAfter, CooldownBucketType bucketType, Type? cooldownResponderType = null) : CheckBaseAttribute, ICooldown<CommandContext, CooldownBucket>
{
	/// <summary>
	/// Gets the maximum number of uses before this command triggers a cooldown for its bucket.
	/// </summary>
	public int MaxUses { get; } = maxUses;

	/// <summary>
	/// Gets the time after which the cooldown is reset.
	/// </summary>
	public TimeSpan Reset { get; } = TimeSpan.FromSeconds(resetAfter);

	/// <summary>
	/// Gets the type of the cooldown bucket. This determines how cooldowns are applied.
	/// </summary>
	public CooldownBucketType BucketType { get; } = bucketType;

	/// <summary>
	/// Gets the responder type.
	/// </summary>
	public Type? ResponderType { get; } = cooldownResponderType;

	/// <summary>
	/// Gets a cooldown bucket for given command context.
	/// </summary>
	/// <param name="ctx">Command context to get cooldown bucket for.</param>
	/// <returns>Requested cooldown bucket, or null if one wasn't present.</returns>
	public CooldownBucket GetBucket(CommandContext ctx)
	{
		var bid = this.GetBucketId(ctx, out _, out _, out _);
		ctx.Client.CommandCooldownBuckets.TryGetValue(bid, out var bucket);
		return bucket!;
	}

	/// <summary>
	/// Calculates the cooldown remaining for given command context.
	/// </summary>
	/// <param name="ctx">Context for which to calculate the cooldown.</param>
	/// <returns>Remaining cooldown, or zero if no cooldown is active.</returns>
	public TimeSpan GetRemainingCooldown(CommandContext ctx)
	{
		var bucket = this.GetBucket(ctx);
		return bucket is null
			? TimeSpan.Zero
			: bucket.RemainingUses > 0
				? TimeSpan.Zero
				: bucket.ResetsAt - DateTimeOffset.UtcNow;
	}

	/// <summary>
	/// Calculates bucket ID for given command context.
	/// </summary>
	/// <param name="ctx">Context for which to calculate bucket ID for.</param>
	/// <param name="userId">ID of the user with which this bucket is associated.</param>
	/// <param name="channelId">ID of the channel with which this bucket is associated.</param>
	/// <param name="guildId">ID of the guild with which this bucket is associated.</param>
	/// <returns>Calculated bucket ID.</returns>
	private string GetBucketId(CommandContext ctx, out ulong userId, out ulong channelId, out ulong guildId)
	{
		userId = 0ul;
		if (this.BucketType.HasFlag(CooldownBucketType.User))
			userId = ctx.User.Id;

		channelId = 0ul;
		if (this.BucketType.HasFlag(CooldownBucketType.Channel))
			channelId = ctx.Channel.Id;
		if (this.BucketType.HasFlag(CooldownBucketType.Guild) && ctx.Guild is null)
			channelId = ctx.Channel.Id;

		guildId = 0ul;
		if (ctx.Guild is not null && this.BucketType.HasFlag(CooldownBucketType.Guild))
			guildId = ctx.Guild.Id;

		var bid = CooldownBucket.MakeId(ctx.Command.QualifiedName, "text", userId, channelId, guildId);
		return bid;
	}

	/// <summary>
	/// Executes a check.
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <param name="help">If true, help - returns true.</param>
	public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
	{
		if (help)
			return true;

		var bid = this.GetBucketId(ctx, out var usr, out var chn, out var gld);
		if (ctx.Client.CommandCooldownBuckets.TryGetValue(bid, out var bucket))
			return await this.RespondRatelimitHitAsync(ctx, await bucket.DecrementUseAsync(ctx), bucket);

		bucket = new(this.MaxUses, this.Reset, ctx.Command.QualifiedName, "text", usr, chn, gld);
		ctx.Client.CommandCooldownBuckets.AddOrUpdate(bid, bucket, (k, v) => bucket);

		return await this.RespondRatelimitHitAsync(ctx, await bucket.DecrementUseAsync(ctx), bucket);
	}

	/// <inheritdoc/>
	public async Task<bool> RespondRatelimitHitAsync(CommandContext ctx, bool noHit, CooldownBucket bucket)
	{
		if (noHit)
			return true;

		if (this.ResponderType is null)
		{
			try
			{
				await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:", false));
			}
			catch (UnauthorizedException)
			{
				// ignore
			}

			return false;
		}

		var providerMethod = this.ResponderType.GetMethod(nameof(ICooldownResponder.Responder));
		var providerInstance = Activator.CreateInstance(this.ResponderType);
		await ((Task)providerMethod.Invoke(providerInstance, [ctx])).ConfigureAwait(false);

		return false;
	}
}

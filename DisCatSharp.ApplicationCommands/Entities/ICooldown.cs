using System;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.Enums;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Cooldown feature contract
/// </summary>
/// <typeparam name="TContextType">Type of <see cref="BaseContext"/> in which this cooldown handles</typeparam>
/// <typeparam name="TBucketType">Type of Cooldown bucket</typeparam>
public interface ICooldown<in TContextType, out TBucketType>
	where TContextType : BaseContext
	where TBucketType : CooldownBucket
{
	/// <summary>
	/// Gets the maximum number of uses before this command triggers a cooldown for its bucket.
	/// </summary>
	int MaxUses { get; }

	/// <summary>
	/// Gets the time after which the cooldown is reset.
	/// </summary>
	TimeSpan Reset { get; }

	/// <summary>
	/// Gets the type of the cooldown bucket. This determines how a cooldown is applied.
	/// </summary>
	CooldownBucketType BucketType { get; }

	/// <summary>
	/// Calculates the cooldown remaining for given context.
	/// </summary>
	/// <param name="ctx">Context for which to calculate the cooldown.</param>
	/// <returns>Remaining cooldown, or zero if no cooldown is active</returns>
	TimeSpan GetRemainingCooldown(TContextType ctx);

	/// <summary>
	/// Gets a cooldown bucket for given context
	/// </summary>
	/// <param name="ctx">Command context to get cooldown bucket for.</param>
	/// <returns>Requested cooldown bucket, or null if one wasn't present</returns>
	TBucketType GetBucket(TContextType ctx);
}

using System.Collections.Generic;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a typed before-and-after audit log collection change.
/// </summary>
/// <typeparam name="T">The item type stored by the collection.</typeparam>
/// <remarks>
///     Collection changes are exposed separately from scalar value changes because Discord often uses arrays for
///     special delta payloads such as member role additions or removed tags.
/// </remarks>
public sealed class DiscordAuditLogCollectionChange<T>
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordAuditLogCollectionChange{T}" /> class.
	/// </summary>
	/// <param name="hasBefore">Whether the change included a previous collection.</param>
	/// <param name="before">The previous collection, if supplied.</param>
	/// <param name="hasAfter">Whether the change included a new collection.</param>
	/// <param name="after">The new collection, if supplied.</param>
	internal DiscordAuditLogCollectionChange(bool hasBefore, IReadOnlyList<T>? before, bool hasAfter, IReadOnlyList<T>? after)
	{
		this.HasBefore = hasBefore;
		this.Before = before;
		this.HasAfter = hasAfter;
		this.After = after;
	}

	/// <summary>
	///     Gets whether Discord supplied a previous collection.
	/// </summary>
	public bool HasBefore { get; }

	/// <summary>
	///     Gets the previous collection.
	/// </summary>
	public IReadOnlyList<T>? Before { get; }

	/// <summary>
	///     Gets whether Discord supplied a new collection.
	/// </summary>
	public bool HasAfter { get; }

	/// <summary>
	///     Gets the new collection.
	/// </summary>
	public IReadOnlyList<T>? After { get; }

	/// <summary>
	///     Gets whether the collection was introduced from a previously absent state.
	/// </summary>
	public bool IsCreated
		=> !this.HasBefore && this.HasAfter;

	/// <summary>
	///     Gets whether the collection was reset or removed.
	/// </summary>
	public bool IsReset
		=> this.HasBefore && !this.HasAfter;
}

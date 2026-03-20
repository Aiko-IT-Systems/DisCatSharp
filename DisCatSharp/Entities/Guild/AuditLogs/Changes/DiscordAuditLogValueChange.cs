namespace DisCatSharp.Entities;

/// <summary>
///     Represents a typed before-and-after audit log value change.
/// </summary>
/// <typeparam name="T">The value type stored by the change.</typeparam>
/// <remarks>
///     Discord may omit either side of a change object. Use <see cref="HasBefore" /> and <see cref="HasAfter" /> to
///     distinguish between an explicit value and an omitted one.
/// </remarks>
public sealed class DiscordAuditLogValueChange<T>
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordAuditLogValueChange{T}" /> class.
	/// </summary>
	/// <param name="hasBefore">Whether the change included a previous value.</param>
	/// <param name="before">The previous value, if supplied.</param>
	/// <param name="hasAfter">Whether the change included a new value.</param>
	/// <param name="after">The new value, if supplied.</param>
	internal DiscordAuditLogValueChange(bool hasBefore, T? before, bool hasAfter, T? after)
	{
		this.HasBefore = hasBefore;
		this.Before = before;
		this.HasAfter = hasAfter;
		this.After = after;
	}

	/// <summary>
	///     Gets whether Discord supplied a previous value.
	/// </summary>
	public bool HasBefore { get; }

	/// <summary>
	///     Gets the previous value.
	/// </summary>
	public T? Before { get; }

	/// <summary>
	///     Gets whether Discord supplied a new value.
	/// </summary>
	public bool HasAfter { get; }

	/// <summary>
	///     Gets the new value.
	/// </summary>
	public T? After { get; }

	/// <summary>
	///     Gets whether the value was introduced from a previously absent state.
	/// </summary>
	public bool IsCreated
		=> !this.HasBefore && this.HasAfter;

	/// <summary>
	///     Gets whether the value was reset or removed.
	/// </summary>
	public bool IsReset
		=> this.HasBefore && !this.HasAfter;
}

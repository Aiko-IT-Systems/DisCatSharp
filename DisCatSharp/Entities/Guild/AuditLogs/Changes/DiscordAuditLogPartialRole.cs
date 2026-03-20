namespace DisCatSharp.Entities;

/// <summary>
///     Represents the partial role structure Discord uses inside special audit log role delta payloads.
/// </summary>
/// <remarks>
///     Discord uses this reduced object shape for change keys such as <c>$add</c> and <c>$remove</c> on
///     member-role-related audit log entries.
/// </remarks>
public sealed class DiscordAuditLogPartialRole
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordAuditLogPartialRole" /> class.
	/// </summary>
	/// <param name="id">The role id.</param>
	/// <param name="name">The role name.</param>
	internal DiscordAuditLogPartialRole(ulong id, string? name)
	{
		this.Id = id;
		this.Name = name;
	}

	/// <summary>
	///     Gets the role id.
	/// </summary>
	public ulong Id { get; }

	/// <summary>
	///     Gets the role name, if Discord supplied one.
	/// </summary>
	public string? Name { get; }
}

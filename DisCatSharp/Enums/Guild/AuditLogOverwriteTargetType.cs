namespace DisCatSharp.Enums;

/// <summary>
///     Represents the Discord overwrite target type used by audit log overwrite entries.
/// </summary>
/// <remarks>
///     Discord serializes this value as a string inside the audit log <c>options</c> object.
/// </remarks>
public enum AuditLogOverwriteTargetType
{
	/// <summary>
	///     Indicates that the overwrite target type is unknown or unsupported.
	/// </summary>
	Unknown = 0,

	/// <summary>
	///     Indicates that the overwrite targets a role.
	/// </summary>
	Role = 1,

	/// <summary>
	///     Indicates that the overwrite targets a member.
	/// </summary>
	Member = 2
}

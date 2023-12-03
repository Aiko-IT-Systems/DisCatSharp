namespace DisCatSharp.Enums;

/// <summary>
/// Represents the type of an <see cref="DisCatSharp.Entities.DiscordApplicationRoleConnectionMetadata"/>.
/// </summary>
public enum ApplicationRoleConnectionMetadataType
{
	/// <summary>
	/// The metadata value (`integer`) is less than or equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerLessThanOrÈqual = 1,

	/// <summary>
	/// The metadata value (`integer`) is greater than or equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerGreaterThanOrÈqual = 2,

	/// <summary>
	/// The metadata value (`integer`) is equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerEqual = 3,

	/// <summary>
	/// The metadata value (`integer`) is not equal to the guild's configured value (`integer`).
	/// </summary>
	IntegerNotEqual = 4,

	/// <summary>
	/// The metadata value (`ISO8601 string`) is less than or equal to the guild's configured value (`integer`; `days before current date`).
	/// </summary>
	DatetimeLessThanOrÈqual = 5,

	/// <summary>
	/// The metadata value (`ISO8601 string`) is greater than or equal to the guild's configured value (`integer`; `days before current date`).
	/// </summary>
	DatetimeGreaterThanOrÈqual = 6,

	/// <summary>
	/// The metadata value (`integer`) is equal to the guild's configured value (`integer`; `1`).
	/// </summary>
	BooleanEqual = 7,

	/// <summary>
	/// The metadata value (`integer`) is not equal to the guild's configured value (`integer`; `1`).
	/// </summary>
	BooleanNotEqual = 8
}

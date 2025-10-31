using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
///     Denotes the type of formatting to use for timestamps.
/// </summary>
public enum TimestampFormat : byte
{
	/// <summary>
	///     A short time. e.g. 16:20.
	/// </summary>
	ShortTime = (byte)'t',

	/// <summary>
	///     A medium time. e.g. 16:20:30.
	/// </summary>
	MediumTime = (byte)'T',

	/// <summary>
	///     A short date. e.g. 31/10/2025.
	/// </summary>
	ShortDate = (byte)'d',

	/// <summary>
	///     A long date. e.g. October 31, 2025.
	/// </summary>
	LongDate = (byte)'D',

	/// <summary>
	///     A long date with short time. e.g. October 31, 2025 at 16:20.
	/// </summary>
	LongDateShortTime = (byte)'f',

	/// <summary>
	///     A full date with short time. e.g. Friday, October 31, 2025 at 16:20.
	/// </summary>
	FullDateShortTime = (byte)'F',

	/// <summary>
	///     A short date with short time. e.g. 31/10/2025, 16:20.
	/// </summary>
	ShortDateShortTime = (byte)'s',

	/// <summary>
	///     A short date with medium time. e.g. 31/10/2025, 16:20:30.
	/// </summary>
	ShortDateMediumTime = (byte)'S',

	/// <summary>
	///     The time relative to the client. e.g. 2 hours ago.
	/// </summary>
	RelativeTime = (byte)'R',

	// ───────────────────────────────────────────────
	// LEGACY ALIASES (for backwards compatibility)
	// ───────────────────────────────────────────────

	/// <summary>
	///     Alias for <see cref="MediumTime"/>.
	/// </summary>
	[DiscordDeprecated("Use MediumTime instead.")]
	LongTime = MediumTime,

	/// <summary>
	///     Alias for <see cref="LongDateShortTime"/>.
	/// </summary>
	[DiscordDeprecated("Use LongDateShortTime instead.")]
	ShortDateTime = LongDateShortTime,

	/// <summary>
	///     Alias for <see cref="FullDateShortTime"/>.
	/// </summary>
	[DiscordDeprecated("Use FullDateShortTime instead.")]
	LongDateTime = FullDateShortTime
}

using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a invite flag extensions.
/// </summary>
public static class InviteFlagExtensions
{
	/// <summary>
	///     Calculates whether these invite flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasInviteFlag(this InviteFlags baseFlags, InviteFlags flag) => (baseFlags & flag) == flag;
}

[Flags]
public enum InviteFlags : long
{
	/// <summary>
	///     Invite has no flags
	/// </summary>
	None = 0,

	/// <inheritdoc cref="IsGuestInvite" />
	[Deprecated("Replaced by IsGuestInvite")]
	GuestMembership = IsGuestInvite,

	/// <summary>
	///     Invite grants temporary guest membership.
	///     All channels but the one invited to are hidden and user gets kicked if they leave the voice.
	/// </summary>
	IsGuestInvite = 1L << 0,

	/// <summary>
	///     The invite has been viewed by any user (has been retrieved using the get invite endpoint).
	/// </summary>
	[Deprecated("Replaced by IsViewed")]
	Viewed = IsViewed,

	IsViewed = 1L << 1,

	/// <summary>
	///     Invite is enhanced.
	/// </summary>
	IsEnhanced = 1L << 2,

	/// <summary>
	///     Invite bypasses a clan application.
	/// </summary>
	IsApplicationBypass = 1L << 3,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue

	//AssignableFlagsMask = 9
}

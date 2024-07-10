using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a invite flag extensions.
/// </summary>
public static class InviteFlagExtensions
{
	/// <summary>
	/// Calculates whether these invite flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasInviteFlag(this InviteFlags baseFlags, InviteFlags flag) => (baseFlags & flag) == flag;
}

[Flags]
public enum InviteFlags
{
	/// <summary>
	/// Invite has no flags
	/// </summary>
	None = 0,

	/// <summary>
	/// Invite grants temporary guest membership.
	/// All channels but the one invited to are hidden and user gets kicked if they leave the voice.
	/// </summary>
	GuestMembership = 1 << 0,

	/// <summary>
	/// The invite has been viewed by any user (has been retrieved using the get invite endpoint).
	/// </summary>
	Viewed = 1 << 1
}

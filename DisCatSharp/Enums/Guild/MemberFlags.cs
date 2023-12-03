using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a member flag extensions.
/// </summary>
public static class MemberFlagExtensions
{
	/// <summary>
	/// Calculates whether these member flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasMemberFlag(this MemberFlags baseFlags, MemberFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
/// Represents additional details of a member account.
/// </summary>
[Flags]
public enum MemberFlags : long
{
	/// <summary>
	/// Member has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	/// Member has left and rejoined the guild.
	/// </summary>
	DidRejoin = 1 << 0,

	/// <summary>
	/// Member has completed onboarding.
	/// </summary>
	[DiscordInExperiment]
	CompletedOnboarding = 1 << 1,

	/// <summary>
	/// Member bypasses guild verification requirements.
	/// </summary>
	[DiscordInExperiment]
	BypassesVerification = 1 << 2,

	[DiscordInExperiment]
	Verified = BypassesVerification,

	/// <summary>
	/// Member has started onboarding.
	/// </summary>
	[DiscordInExperiment]
	StartedOnboarding = 1 << 3,

	/// <summary>
	/// Member is a guest.
	/// Temporary members that are not in the guild.
	/// </summary>
	[DiscordInExperiment]
	IsGuest = 1 << 4,

	/// <summary>
	/// Member has started home actions.
	/// </summary>
	[DiscordInExperiment]
	StartedHomeActions = 1 << 5,

	/// <summary>
	/// Member has completed home actions.
	/// </summary>
	[DiscordInExperiment]
	CompletedHomeActions = 1 << 6,

	/// <summary>
	/// Members username or nickname contains words that are not allowed.
	/// </summary>
	[DiscordInExperiment]
	AutomodQuarantinedUsernameOrGuildNickname = 1 << 7,

	/// <summary>
	/// Members user or guild bio contains words that are not allowed.
	/// </summary>
	[DiscordInExperiment]
	AutomodQuarantinedBio = 1 << 8
}

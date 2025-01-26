using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a member flag extensions.
/// </summary>
public static class MemberFlagExtensions
{
	/// <summary>
	///     Calculates whether these member flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasMemberFlag(this MemberFlags baseFlags, MemberFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents additional details of a member account.
/// </summary>
[Flags]
public enum MemberFlags : long
{
	/// <summary>
	///     Member has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///     Member has left and rejoined the guild.
	/// </summary>
	DidRejoin = 1 << 0,

	/// <summary>
	///     Member has completed onboarding.
	/// </summary>
	CompletedOnboarding = 1 << 1,

	/// <summary>
	///     Member bypasses guild verification requirements.
	/// </summary>
	BypassesVerification = 1 << 2,

	/// <summary>
	///     Member has started onboarding.
	/// </summary>
	StartedOnboarding = 1 << 3,

	/// <summary>
	///     Member is a guest.
	///     Temporary members that are not in the guild.
	/// </summary>
	[DiscordInExperiment]
	IsGuest = 1 << 4,

	/// <summary>
	///     Member has started home actions.
	/// </summary>
	StartedHomeActions = 1 << 5,

	/// <summary>
	///     Member has completed home actions.
	/// </summary>
	CompletedHomeActions = 1 << 6,

	/// <summary>
	///     Member's username, display name, or nickname is blocked by AutoMod.
	/// </summary>
	AutomodQuarantinedUsername = 1 << 7,

	/// <inheritdoc cref="AutomodQuarantinedUsername" />
	AutomodQuarantinedUsernameOrGuildNickname = AutomodQuarantinedUsername,

	/// <summary>
	///     Member's user or guild bio is blocked by AutoMod.
	/// </summary>
	[DiscordDeprecated]
	AutomodQuarantinedBio = 1 << 8,

	/// <summary>
	///     Member has dismissed the DM settings upsell.
	/// </summary>
	DmSettingsUpsellAcknowledged = 1 << 9,

	/// <summary>
	///     Members clan tag is blocked by AutoMod.
	/// </summary>
	[DiscordInExperiment]
	AutomodQuarantinedClanTag = 1 << 10
}

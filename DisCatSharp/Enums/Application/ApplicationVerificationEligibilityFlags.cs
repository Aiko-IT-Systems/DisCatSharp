using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents an extension for checking application verification eligibility flags.
/// </summary>
public static class ApplicationVerificationEligibilityFlagExtensions
{
	/// <summary>
	///     Calculates whether these verification eligibility flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasVerificationEligibilityFlag(this ApplicationVerificationEligibilityFlags baseFlags, ApplicationVerificationEligibilityFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents the verification eligibility flags.
/// </summary>
[Flags]
public enum ApplicationVerificationEligibilityFlags : long
{
	/// <summary>
	///     The application has a team.
	/// </summary>
	HasTeam = 1L << 1,

	/// <summary>
	///     The application's name is considered safe.
	/// </summary>
	SafeApplicationName = 1L << 2,

	/// <summary>
	///     The application's description is considered safe.
	/// </summary>
	SafeApplicationDescription = 1L << 3,

	/// <summary>
	///     The application's role connections metadata is considered safe.
	/// </summary>
	SafeApplicationRoleConnectionsMetadata = 1L << 4,

	/// <summary>
	///     The application has a terms of service URL set.
	/// </summary>
	TermsOfService = 1L << 5,

	/// <summary>
	///     The application has a privacy policy URL set.
	/// </summary>
	PrivacyPolicy = 1L << 6,

	/// <summary>
	///     The application has an invite link set.
	/// </summary>
	InviteLink = 1L << 7,

	/// <summary>
	///     The application is not embedded.
	/// </summary>
	NotEmbedded = 1L << 8,

	/// <summary>
	///     The application has no unapproved intents.
	/// </summary>
	NoUnapprovedIntents = 1L << 9,

	/// <summary>
	///     The user is the owner of the team that owns the application.
	/// </summary>
	UserIsTeamOwner = 1L << 10,

	/// <summary>
	///     The team owner is verified.
	/// </summary>
	TeamOwnerIsVerified = 1L << 11,

	/// <summary>
	///     The user has two-factor authentication enabled.
	/// </summary>
	User2FaEnabled = 1L << 12,

	/// <summary>
	///     The user's email is verified.
	/// </summary>
	UserEmailVerified = 1L << 13,

	/// <summary>
	///     All team members' emails are verified.
	/// </summary>
	TeamMembersEmailVerified = 1L << 14,

	/// <summary>
	///     All team members have two-factor authentication enabled.
	/// </summary>
	TeamMembers2FaEnabled = 1L << 15,

	/// <summary>
	///     The application has no blocking issues.
	/// </summary>
	NoBlockingIssues = 1L << 16,

	/// <summary>
	///     The application is eligible for verification.
	/// </summary>
	EligibleForVerification = HasTeam | SafeApplicationName | SafeApplicationDescription | SafeApplicationRoleConnectionsMetadata |
	                          TermsOfService | PrivacyPolicy | InviteLink | NotEmbedded | NoUnapprovedIntents | UserIsTeamOwner |
	                          TeamOwnerIsVerified | User2FaEnabled | UserEmailVerified | TeamMembersEmailVerified | TeamMembers2FaEnabled |
	                          NoBlockingIssues,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
